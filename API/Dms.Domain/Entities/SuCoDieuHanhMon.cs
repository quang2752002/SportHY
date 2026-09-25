using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("SuCoDieuHanhMon")]
    public class SuCoDieuHanhMon : BaseEntity
    {
        public int GiaiDauMonTheThaoId { get; set; }

        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao GiaiDauMonTheThao { get; set; } = null!;

        public int? TranDauId { get; set; }

        [ForeignKey(nameof(TranDauId))]
        public virtual TranDau? TranDau { get; set; }

        [Required]
        [MaxLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string MoTa { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string MucDo { get; set; } = "BinhThuong";

        [Required]
        [MaxLength(30)]
        public string TrangThai { get; set; } = "Moi";

        [MaxLength(256)]
        public string? NguoiPhuTrach { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? GhiChuXuLy { get; set; }

        public DateTime? ThoiGianGiaiQuyet { get; set; }
    }
}
