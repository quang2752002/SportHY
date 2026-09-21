namespace Dms.Domain.Common
{
    public static class AppRoles
    {
        public const string Admin = "Admin";                     // Quản trị toàn bộ phần mềm, cấu hình tham số chung, cấp tài khoản và sao lưu dữ liệu
        public const string Manager = "Manager";                 // Quản lý chung và quản lý 1 giải đấu cụ thể
        public const string HeadReferee = "HeadReferee";         // Trưởng ban trọng tài: Phân công trọng tài, giám sát tiến độ
        public const string Referee = "Referee";                 // Trọng tài: Cập nhật nhật ký trận đấu (tỷ số, thẻ phạt, diễn biến) theo thời gian thực
        public const string Secretary = "Secretary";             // Thư ký giải: Theo dõi nội dung thi đấu, biên bản, xuất kết quả PDF/Excel
        public const string Delegation = "Delegation";           // Đơn vị trực thuộc (Sở/Xã/Đơn vị): Quản lý & đăng ký VĐV, đội thi đấu
        public const string SportCoordinator = "SportCoordinator"; // Người điều hành môn: Theo dõi các nội dung, lịch thi đấu, tiến độ, kết quả của môn

        public static readonly string[] AllRoles = new[]
        {
            Admin,
            Manager,
            HeadReferee,
            Referee,
            Secretary,
            Delegation,
            SportCoordinator
        };
    }
}
