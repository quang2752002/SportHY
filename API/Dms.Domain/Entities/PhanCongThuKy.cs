using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    /// <summary>
    /// Bản ghi phân công một thư ký vào một giải đấu.
    /// </summary>
    [Table("PhanCongThuKy")]
    public class PhanCongThuKy : BaseEntity
    {
        public int GiaiDauId { get; set; }

        [ForeignKey(nameof(GiaiDauId))]
        public virtual GiaiDau GiaiDau { get; set; } = null!;

        public int ThuKyId { get; set; }

        [ForeignKey(nameof(ThuKyId))]
        public virtual ThuKy ThuKy { get; set; } = null!;

        [MaxLength(100)]
        public string? VaiTro { get; set; }

        [MaxLength(500)]
        public string? GhiChu { get; set; }
    }
}
