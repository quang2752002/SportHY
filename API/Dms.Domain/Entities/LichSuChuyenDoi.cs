using Dms.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("LichSuChuyenDoi")]
    public class LichSuChuyenDoi : BaseEntity
    {
        public int VanDongVienId { get; set; }
        [ForeignKey(nameof(VanDongVienId))]
        public virtual VanDongVien VanDongVien { get; set; } = null!;

        public int? DoiCuId { get; set; }
        [ForeignKey(nameof(DoiCuId))]
        public virtual Doi? DoiCu { get; set; }

        public int DoiMoiId { get; set; }
        [ForeignKey(nameof(DoiMoiId))]
        public virtual Doi DoiMoi { get; set; } = null!;

        public DateTime NgayChuyen { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? LyDo { get; set; }

        [MaxLength(200)]
        public string? NguoiXacNhan { get; set; }

        [MaxLength(500)]
        public string? GhiChu { get; set; }
    }
}
