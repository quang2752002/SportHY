using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("CumSan")]
    public class CumSan : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DiaChi { get; set; }

        public int? SoLuongSan { get; set; }

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<SanDau> SanDaus { get; set; } = new List<SanDau>();
    }
}
