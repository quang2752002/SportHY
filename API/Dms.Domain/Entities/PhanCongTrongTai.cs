using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("PhanCongTrongTai")]
    public class PhanCongTrongTai : BaseEntity
    {
        public int TranDauId { get; set; }
        [ForeignKey(nameof(TranDauId))]
        public virtual TranDau TranDau { get; set; } = null!;

        public int TrongTaiId { get; set; }
        [ForeignKey(nameof(TrongTaiId))]
        public virtual TrongTai TrongTai { get; set; } = null!;

        [MaxLength(100)]
        public string? VaiTro { get; set; }

        [MaxLength(500)]
        public string? GhiChu { get; set; }
    }
}
