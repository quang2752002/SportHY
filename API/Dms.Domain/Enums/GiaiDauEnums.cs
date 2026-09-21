using System.ComponentModel;
using System.Reflection;

namespace Dms.Domain.Enums
{
    /// <summary>
    /// Phạm vi áp dụng của giải đấu
    /// </summary>
    public enum PhamViGiaiDau
    {
        [Description("Tất cả đơn vị")]
        TatCa = 1,

        [Description("Theo khối ngành")]
        TheoKhoi = 2
    }

    /// <summary>
    /// Trạng thái của giải đấu
    /// </summary>
    public enum TrangThaiGiaiDau
    {
        [Description("Bản nháp")]
        Nhap = 1,

        [Description("Sắp diễn ra")]
        SapDienRa = 2,

        [Description("Đang diễn ra")]
        DangDienRa = 3,

        [Description("Đã kết thúc")]
        KetThuc = 4,

        [Description("Đã hủy")]
        Huy = 5
    }

    /// <summary>
    /// Hình thức / thể thức thi đấu của môn thể thao
    /// </summary>
    public enum HinhThucThiDau
    {
        [Description("Loại trực tiếp (Knockout)")]
        LoaiTrucTiep = 1,

        [Description("Vòng tròn tính điểm / Vòng bảng (Round Robin)")]
        VongBang = 2,

        [Description("Kết hợp vòng bảng và loại trực tiếp (Group Stage + Knockout)")]
        KetHopVongBangVaLoaiTrucTiep = 3,

        [Description("Nhánh thắng - Nhánh thua (Double Elimination)")]
        NhanhThangNhanhThua = 4,

        [Description("Hệ Thụy Sĩ (Swiss System)")]
        HeThuySi = 5,

        [Description("Tính điểm xếp hạng / Tính giờ - Thành tích (Leaderboard / Time Trial)")]
        TinhDiemXepHang = 6,

        [Description("Khác")]
        Khac = 7
    }

    public static class EnumExtensions
    {
        /// <summary>
        /// Lấy nội dung Description tiếng Việt của Enum
        /// </summary>
        public static string GetDescription(this System.Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute != null ? attribute.Description : value.ToString();
        }
    }
}
