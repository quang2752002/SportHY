using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("VongDau")]
    public class VongDau : BaseEntity
    {
        public int GiaiDauMonTheThaoId { get; set; }
        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao GiaiDauMonTheThao { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Ten { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string LoaiVong { get; set; } = string.Empty;

        public int ThuTu { get; set; }

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        public virtual ICollection<TranDau> TranDaus { get; set; } = new List<TranDau>();
    }
}
