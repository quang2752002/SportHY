using System;

namespace Dms.Application.DTOs
{
    public class KhoiDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
        public int SoDonVi { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateKhoiDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}
