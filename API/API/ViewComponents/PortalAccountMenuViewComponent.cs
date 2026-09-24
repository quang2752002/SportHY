using Microsoft.AspNetCore.Mvc;

namespace API.ViewComponents
{
    public class PortalAccountMenuViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string currentArea, string portalLabel, string? linkedName = null)
        {
            var taskLinks = currentArea switch
            {
                "TruongBanTrongTai" => new List<PortalTaskLink>
                {
                    new("Tổng quan giải", "/TruongBanTrongTai/Home", "fa-solid fa-chart-pie", "text-primary"),
                    new("Phân công trọng tài", "/TruongBanTrongTai/PhanCong", "fa-solid fa-clipboard-user", "text-warning"),
                    new("Lịch làm việc & tiến độ", "/TruongBanTrongTai/LichLamViec", "fa-solid fa-calendar-days", "text-success")
                },
                "TrongTai" => new List<PortalTaskLink>
                {
                    new("Nhiệm vụ Phân công", "/TrongTai/Home", "fa-solid fa-calendar-check", "text-warning"),
                    new("Ghi nhận Kết quả", "/TrongTai/KetQua", "fa-solid fa-clipboard-list", "text-primary"),
                    new("Biên bản Trận đấu", "/TrongTai/BienBan", "fa-solid fa-file-signature", "text-success")
                },
                _ => new List<PortalTaskLink>()
            };

            return View(new PortalAccountMenuViewModel(
                currentArea,
                portalLabel,
                User.Identity?.Name ?? "Tài khoản",
                linkedName,
                taskLinks));
        }
    }

    public class PortalAccountMenuViewModel
    {
        public PortalAccountMenuViewModel(
            string currentArea,
            string portalLabel,
            string userName,
            string? linkedName,
            List<PortalTaskLink> taskLinks)
        {
            CurrentArea = currentArea;
            PortalLabel = portalLabel;
            UserName = userName;
            LinkedName = linkedName;
            TaskLinks = taskLinks;
        }

        public string CurrentArea { get; }
        public string PortalLabel { get; }
        public string UserName { get; }
        public string? LinkedName { get; }
        public List<PortalTaskLink> TaskLinks { get; }
    }

    public class PortalTaskLink
    {
        public PortalTaskLink(string label, string url, string icon, string iconClass)
        {
            Label = label;
            Url = url;
            Icon = icon;
            IconClass = iconClass;
        }

        public string Label { get; }
        public string Url { get; }
        public string Icon { get; }
        public string IconClass { get; }
    }
}
