using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("KetQuaTranDau")]
    public class KetQuaTranDau : BaseEntity
    {
        public int ThanhPhanTranDauId { get; set; }
        [ForeignKey(nameof(ThanhPhanTranDauId))]
        public virtual ThanhPhanTranDau ThanhPhanTranDau { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string LoaiKetQua { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,4)")]
        public decimal? GiaTri { get; set; }

        [MaxLength(30)]
        public string? DonVi { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Diem { get; set; }

        public int? XepHang { get; set; }

        public bool KyLuc { get; set; } = false;

        [MaxLength(500)]
        public string? KetQuaText { get; set; }

        [MaxLength(1000)]
        public string? GhiChu { get; set; }
    }
}
