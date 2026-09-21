using System;
using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    // ==================== CUM SAN DTOs ====================
    public class CumSanDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public int? SoLuongSan { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
        public int SoSanHienCo { get; set; } // Số lượng sân đấu thực tế thuộc cụm sân
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateCumSanDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public int? SoLuongSan { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
    }

    // ==================== SAN DAU DTOs ====================
    public class SanDauDto
    {
        public int Id { get; set; }
        public int CumSanId { get; set; }
        public string? TenCumSan { get; set; }
        public int? MonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? LoaiSan { get; set; }
        public int? SoSan { get; set; }
        public int? SucChua { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateSanDauDto
    {
        public int CumSanId { get; set; }
        public int? MonTheThaoId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? LoaiSan { get; set; }
        public int? SoSan { get; set; }
        public int? SucChua { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
