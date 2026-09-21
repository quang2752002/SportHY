using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("KetQuaHiepDau")]
    public class KetQuaHiepDau : BaseEntity
    {
        public int HiepDauId { get; set; }
        [ForeignKey(nameof(HiepDauId))]
        public virtual HiepDau HiepDau { get; set; } = null!;

        public int ThanhPhanTranDauId { get; set; }
        [ForeignKey(nameof(ThanhPhanTranDauId))]
        public virtual ThanhPhanTranDau ThanhPhanTranDau { get; set; } = null!;

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Diem { get; set; }

        [MaxLength(500)]
        public string? GhiChu { get; set; }
    }
}
