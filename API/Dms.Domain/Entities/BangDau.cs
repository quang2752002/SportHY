using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("BangDau")]
    public class BangDau : BaseEntity
    {
        public int GiaiDauMonTheThaoId { get; set; }
        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao GiaiDauMonTheThao { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Ten { get; set; } = string.Empty;

        public int ThuTu { get; set; } = 1;

        public virtual ICollection<ThanhVienBang> ThanhVienBangs { get; set; } = new List<ThanhVienBang>();
        public virtual ICollection<TranDau> TranDaus { get; set; } = new List<TranDau>();
    }
}
