namespace Dms.Application.DTOs
{
    // ==================== THU KY DTOs ====================
    public class ThuKyDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? ChucVu { get; set; }
        public string? DonViCongTac { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateThuKyDto
    {
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? ChucVu { get; set; }
        public string? DonViCongTac { get; set; }
        public bool TrangThai { get; set; } = true;
    }

    /// <summary>
    /// Thông tin một thư ký và trạng thái được phân công vào giải đấu đang chọn.
    /// </summary>
    public class ThuKyPhanCongDto
    {
        public int ThuKyId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? ChucVu { get; set; }
        public string? DonViCongTac { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public bool DaPhanCong { get; set; }
    }
}
