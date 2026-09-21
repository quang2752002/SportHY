using System;

namespace Dms.Application.DTOs
{
    public class DonViDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int? KhoiId { get; set; }
        public string? TenKhoi { get; set; }
        public int? DonViChaId { get; set; }
        public string? TenDonViCha { get; set; }
        public string? LoaiDonVi { get; set; }
        public string? DiaChi { get; set; }
        public string? NguoiDaiDien { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public bool TrangThai { get; set; } = true;
        public int SoVanDongVien { get; set; }
        public int SoDoi { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateDonViDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int? KhoiId { get; set; }
        public int? DonViChaId { get; set; }
        public string? LoaiDonVi { get; set; }
        public string? DiaChi { get; set; }
        public string? NguoiDaiDien { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? MoTa { get; set; }
        public string? HinhAnh { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
