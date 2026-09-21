using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("Khoi")]
    public class Khoi : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<DonVi> DonVis { get; set; } = new List<DonVi>();
        public virtual ICollection<GiaiDauKhoi> GiaiDauKhois { get; set; } = new List<GiaiDauKhoi>();
    }
}
