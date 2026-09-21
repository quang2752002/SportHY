using System;

namespace Dms.Application.DTOs
{
    public class DanhMucMonTheThaoDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
        public int SoMonTheThao { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateDanhMucMonTheThaoDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
