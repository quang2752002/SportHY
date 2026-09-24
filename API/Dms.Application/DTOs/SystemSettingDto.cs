namespace Dms.Application.DTOs
{
    public class SystemSettingDto
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Created { get; set; }
    }

    /// <summary>
    /// DTO chứa các thông số cấu hình hiển thị Footer
    /// </summary>
    public class FooterSettingDto
    {
        /// <summary>Màu nền Footer (Hex, rgb hoặc class css)</summary>
        public string BgColor { get; set; } = "#0f172a";

        /// <summary>Màu chữ Footer</summary>
        public string TextColor { get; set; } = "#94a3b8";

        /// <summary>Tên cơ quan / Sở / Đơn vị tổ chức</summary>
        public string OrganizationName { get; set; } = "Sở Văn Hóa & Thể Thao TP.HCM";

        /// <summary>Hotline hỗ trợ</summary>
        public string Hotline { get; set; } = "(028) 3822 5588 - 0909 123 456";

        /// <summary>Email liên hệ</summary>
        public string Email { get; set; } = "btc.thethao@sportdms.vn";

        /// <summary>Thời gian hỗ trợ kỹ thuật</summary>
        public string SupportHours { get; set; } = "08:00 - 20:00 hàng ngày";

        /// <summary>Đoạn giới thiệu ngắn</summary>
        public string Description { get; set; } = "Hệ thống phần mềm quản lý, điều hành và tổ chức các đại hội thể thao, giải đấu thể dục thể thao chuyên nghiệp và phong trào toàn diện.";

        /// <summary>Dòng thông tin bản quyền</summary>
        public string Copyright { get; set; } = "© 2026 SPORT DMS. Nền Tảng Điều Hành & Quản Lý Giải Đấu Thể Thao. Tất cả các quyền được bảo lưu.";
    }

    /// <summary>
    /// Request cập nhật cấu hình Footer từ trang Admin
    /// </summary>
    public class UpdateFooterSettingRequest
    {
        public string BgColor { get; set; } = "#0f172a";
        public string? TextColor { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string Hotline { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SupportHours { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Copyright { get; set; }
    }
}
