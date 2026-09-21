namespace Dms.Application.DTOs
{
    // ==================== TRONG TAI DTOs ====================
    public class TrongTaiDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? CapBac { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateTrongTaiDto
    {
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? CapBac { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
