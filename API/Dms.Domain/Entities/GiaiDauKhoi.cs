using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("GiaiDauKhoi")]
    public class GiaiDauKhoi : BaseEntity
    {
        public int GiaiDauId { get; set; }
        [ForeignKey(nameof(GiaiDauId))]
        public virtual GiaiDau GiaiDau { get; set; } = null!;

        public int KhoiId { get; set; }
        [ForeignKey(nameof(KhoiId))]
        public virtual Khoi Khoi { get; set; } = null!;
    }
}
