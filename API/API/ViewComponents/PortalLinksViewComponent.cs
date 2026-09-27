using Dms.Application.Common;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.ViewComponents
{
    public class PortalLinksViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITruongBanTrongTaiService _refereeAccessService;

        public PortalLinksViewComponent(
            UserManager<ApplicationUser> userManager,
            ITruongBanTrongTaiService refereeAccessService)
        {
            _userManager = userManager;
            _refereeAccessService = refereeAccessService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? currentArea = null)
        {
            currentArea ??= ViewContext.RouteData.Values["area"]?.ToString();

            var isCurrentTrongTai = string.Equals(currentArea, "TrongTai", StringComparison.OrdinalIgnoreCase);
            var isCurrentTruongBan = string.Equals(currentArea, "TruongBanTrongTai", StringComparison.OrdinalIgnoreCase);

            // Bỏ "Chuyển cổng" ở tất cả các màn khác, chỉ giữ chuyển đổi giữa Trọng tài và Trưởng ban trọng tài
            if (!isCurrentTrongTai && !isCurrentTruongBan)
            {
                return View(new PortalLinksViewModel(currentArea, new List<PortalLinkItem>()));
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return View(new PortalLinksViewModel(currentArea, new List<PortalLinkItem>()));
            }

            var isAdmin = UserClaimsPrincipal.IsInRole(AppRoles.Admin);
            var isManager = UserClaimsPrincipal.IsInRole(AppRoles.Manager);
            var isAdminOrManager = isAdmin || isManager;
            var isSecretary = UserClaimsPrincipal.IsInRole(AppRoles.Secretary);
            var hasHeadRefereeRole = UserClaimsPrincipal.IsInRole(AppRoles.HeadReferee);

            var referee = await _refereeAccessService.GetRefereeByUserIdOrNameAsync(
                user.TrongTaiId,
                user.UserName,
                user.Email);

            var links = new List<PortalLinkItem>();

            if (isCurrentTrongTai)
            {
                // Khi đang ở màn Trọng tài: Cho phép chuyển sang Trưởng ban trọng tài nếu có thẩm quyền
                var managesTournament = referee != null &&
                    (await _refereeAccessService.GetManagedTournamentsAsync(referee.Id, false)).Count > 0;

                if (isAdminOrManager || hasHeadRefereeRole || managesTournament)
                {
                    links.Add(new PortalLinkItem("TruongBanTrongTai", "Trưởng ban trọng tài", "/TruongBanTrongTai/Home", "fa-solid fa-whistle", "text-warning"));
                }
            }
            else if (isCurrentTruongBan)
            {
                // Khi đang ở màn Trưởng ban trọng tài: Cho phép chuyển sang Trọng tài
                var hasRefereeTournament = referee != null &&
                    (await _refereeAccessService.GetRefereeTournamentIdsAsync(referee.Id)).Count > 0;

                if (isAdminOrManager || isSecretary || hasHeadRefereeRole || referee != null || user.TrongTaiId.HasValue || hasRefereeTournament)
                {
                    links.Add(new PortalLinkItem("TrongTai", "Trọng tài", "/TrongTai/Home", "fa-solid fa-flag", "text-warning"));
                }
            }

            return View(new PortalLinksViewModel(currentArea, links));
        }
    }

    public class PortalLinksViewModel
    {
        public PortalLinksViewModel(string? currentArea, List<PortalLinkItem>? links = null)
        {
            CurrentArea = currentArea;
            Links = links ?? new List<PortalLinkItem>();
        }

        public string? CurrentArea { get; }
        public List<PortalLinkItem> Links { get; }
        public bool IsPortalPage => !string.IsNullOrWhiteSpace(CurrentArea);
        public List<PortalLinkItem> LinksToDisplay => IsPortalPage
            ? Links.Where(link => !string.Equals(link.Area, CurrentArea, StringComparison.OrdinalIgnoreCase)).ToList()
            : Links;
        public string Heading => IsPortalPage ? "Chuyển cổng" : "Cổng truy cập";
    }

    public class PortalLinkItem
    {
        public PortalLinkItem(string area, string label, string url, string icon, string iconClass)
        {
            Area = area;
            Label = label;
            Url = url;
            Icon = icon;
            IconClass = iconClass;
        }

        public string Area { get; }
        public string Label { get; }
        public string Url { get; }
        public string Icon { get; }
        public string IconClass { get; }
    }
}
