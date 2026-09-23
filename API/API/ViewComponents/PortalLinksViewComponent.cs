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

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return View(new PortalLinksViewModel(currentArea));
            }

            var isAdmin = UserClaimsPrincipal.IsInRole(AppRoles.Admin);
            var isManager = UserClaimsPrincipal.IsInRole(AppRoles.Manager);
            var isAdminOrManager = isAdmin || isManager;
            var isSecretary = UserClaimsPrincipal.IsInRole(AppRoles.Secretary);
            var referee = await _refereeAccessService.GetRefereeByUserIdOrNameAsync(
                user.TrongTaiId,
                user.UserName,
                user.Email);

            var links = new List<PortalLinkItem>();

            if (isAdmin)
            {
                links.Add(new PortalLinkItem("Admin", "Quản trị hệ thống", "/Admin/GiaiDau", "fa-solid fa-sliders", "text-primary"));
            }

            if (isAdminOrManager)
            {
                links.Add(new PortalLinkItem("Manager", "Quản lý giải", "/Manager/GiaiDau", "fa-solid fa-trophy", "text-warning"));
                links.Add(new PortalLinkItem("DonVi", "Quản lý đoàn", "/DonVi/Home", "fa-solid fa-building-columns", "text-success"));
                links.Add(new PortalLinkItem("DieuHanhMon", "Điều hành môn", "/DieuHanhMon/Home", "fa-solid fa-volleyball", "text-info"));
                links.Add(new PortalLinkItem("ThuKy", "Thư ký giải", "/ThuKy/Home", "fa-solid fa-clipboard-check", "text-primary"));
                links.Add(new PortalLinkItem("TrongTai", "Trọng tài", "/TrongTai/Home", "fa-solid fa-flag", "text-warning"));
                links.Add(new PortalLinkItem("TruongBanTrongTai", "Trưởng ban trọng tài", "/TruongBanTrongTai/Home", "fa-solid fa-whistle", "text-warning"));
            }
            else
            {
                if (UserClaimsPrincipal.IsInRole(AppRoles.Delegation))
                {
                    links.Add(new PortalLinkItem("DonVi", "Quản lý đoàn", "/DonVi/Home", "fa-solid fa-building-columns", "text-success"));
                }

                if (UserClaimsPrincipal.IsInRole(AppRoles.SportCoordinator))
                {
                    links.Add(new PortalLinkItem("DieuHanhMon", "Điều hành môn", "/DieuHanhMon/Home", "fa-solid fa-volleyball", "text-info"));
                }

                if (isSecretary)
                {
                    links.Add(new PortalLinkItem("ThuKy", "Thư ký giải", "/ThuKy/Home", "fa-solid fa-clipboard-check", "text-primary"));
                }

                if (isSecretary || (referee != null && (await _refereeAccessService.GetRefereeTournamentIdsAsync(referee.Id)).Count > 0))
                {
                    links.Add(new PortalLinkItem("TrongTai", "Trọng tài", "/TrongTai/Home", "fa-solid fa-flag", "text-warning"));
                }

                var hasHeadRefereeRole = UserClaimsPrincipal.IsInRole(AppRoles.HeadReferee);
                var managesTournament = referee != null &&
                    (await _refereeAccessService.GetManagedTournamentsAsync(referee.Id, false)).Count > 0;
                if (hasHeadRefereeRole || managesTournament)
                {
                    links.Add(new PortalLinkItem("TruongBanTrongTai", "Trưởng ban trọng tài", "/TruongBanTrongTai/Home", "fa-solid fa-whistle", "text-warning"));
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
