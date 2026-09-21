using System;

namespace Dms.Application.DTOs
{
    public class MonTheThaoDto
    {
        public int Id { get; set; }
        public int DanhMucId { get; set; }
        public string? TenDanhMuc { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool LaMonDongDoi { get; set; } = false;
        public string GioiTinh { get; set; } = "HonHop";
        public string HinhThucThiDau { get; set; } = "LoaiTrucTiep";
        public int? SoLuongVanDongVienToiThieu { get; set; }
        public int? SoLuongVanDongVienToiDa { get; set; }
        public int? SoDoiToiDa { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateMonTheThaoDto
    {
        public int DanhMucId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool LaMonDongDoi { get; set; } = false;
        public string? GioiTinh { get; set; } = "HonHop";
        public string? HinhThucThiDau { get; set; }
        public int? SoLuongVanDongVienToiThieu { get; set; }
        public int? SoLuongVanDongVienToiDa { get; set; }
        public int? SoDoiToiDa { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
