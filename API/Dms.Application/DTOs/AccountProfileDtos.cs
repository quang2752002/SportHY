namespace Dms.Application.DTOs
{
    public class AccountProfileDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<AccountLinkedProfileDto> LinkedProfiles { get; set; } = new();
    }

    public class AccountLinkedProfileDto
    {
        public string LoaiHoSo { get; set; } = string.Empty;
        public string? Ma { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? ChucDanh { get; set; }
        public string? DonViCongTac { get; set; }
        public string? DiaChi { get; set; }
        public string? NguoiDaiDien { get; set; }
        public string? NhomDonVi { get; set; }
        public string? DonViCha { get; set; }
        public string? MoTa { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }
}
