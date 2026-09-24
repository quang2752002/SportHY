namespace Dms.Domain.Enums
{
    /// <summary>
    /// Các khóa cấu hình hệ thống lưu trong bảng SystemSetting
    /// </summary>
    public enum SystemSettingKey
    {
        /// <summary>Màu nền Footer (Mã Hex, RGB hoặc CSS class)</summary>
        Footer_BgColor = 1,

        /// <summary>Màu chữ Footer</summary>
        Footer_TextColor = 2,

        /// <summary>Tên cơ quan / Sở / Đơn vị tổ chức</summary>
        Footer_OrganizationName = 3,

        /// <summary>Số điện thoại Hotline liên hệ</summary>
        Footer_Hotline = 4,

        /// <summary>Địa chỉ Email hỗ trợ / Ban tổ chức</summary>
        Footer_Email = 5,

        /// <summary>Khung giờ hỗ trợ kỹ thuật</summary>
        Footer_SupportHours = 6,

        /// <summary>Mô tả ngắn ở phần chân trang</summary>
        Footer_Description = 7,

        /// <summary>Thông tin bản quyền ở chân trang</summary>
        Footer_Copyright = 8
    }
}
