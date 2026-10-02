using System;
using System.Collections.Generic;

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
        public string? LoaiThiDau { get; set; } = "DongDoi";
        public int? SoLuongVdvToiThieu => SoLuongVanDongVienToiThieu;
        public int? SoLuongVdvToiDa => SoLuongVanDongVienToiDa;
        public string HinhThucThiDau { get; set; } = "LoaiTrucTiep";
        public int? SoLuongVanDongVienToiThieu { get; set; }
        public int? SoLuongVanDongVienToiDa { get; set; }
        public int? SoDoiToiDa { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
        public List<DieuLeMonTheThaoDto> DieuLeMonTheThaos { get; set; } = new();
    }

    public class CreateUpdateMonTheThaoDto
    {
        public int DanhMucId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool LaMonDongDoi { get; set; } = false;
        public string? GioiTinh { get; set; } = "HonHop";
        public string? LoaiThiDau { get; set; } = "DongDoi";
        public string? HinhThucThiDau { get; set; }
        public int? SoLuongVanDongVienToiThieu { get; set; }
        public int? SoLuongVanDongVienToiDa { get; set; }
        public int? SoLuongVdvToiThieu { get; set; }
        public int? SoLuongVdvToiDa { get; set; }
        public int? SoDoiToiDa { get; set; }
        public bool TrangThai { get; set; } = true;
        public CreateUpdateCauHinhTheThucDto? CauHinhTheThuc { get; set; }
        public List<DieuLeMonTheThaoDto>? DieuLeMonTheThaos { get; set; }
    }

    public class DieuLeMonTheThaoDto
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? TepDinhKem { get; set; }
        public int ThuTu { get; set; }
        public bool TrangThai { get; set; } = true;
    }

    public class UploadedDieuLeMonTheThaoFileDto
    {
        public string Url { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
