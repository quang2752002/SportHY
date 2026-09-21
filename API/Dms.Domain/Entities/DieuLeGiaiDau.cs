using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("DieuLeGiaiDau")]
    public class DieuLeGiaiDau : BaseEntity
    {
        public int GiaiDauId { get; set; }
        [ForeignKey(nameof(GiaiDauId))]
        public virtual GiaiDau GiaiDau { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        public string NoiDung { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? TepDinhKem { get; set; }

        public int ThuTu { get; set; } = 0;

        public bool TrangThai { get; set; } = true;
    }
}
