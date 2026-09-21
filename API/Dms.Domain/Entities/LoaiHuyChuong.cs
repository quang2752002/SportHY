using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("LoaiHuyChuong")]
    public class LoaiHuyChuong : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; } = string.Empty;

        public int ThuTu { get; set; }

        public virtual ICollection<HuyChuong> HuyChuongs { get; set; } = new List<HuyChuong>();
    }
}
