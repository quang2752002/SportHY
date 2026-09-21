using Dms.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("ThanhVienDoi")]
    public class ThanhVienDoi : BaseEntity
    {
        public int DoiId { get; set; }
        [ForeignKey(nameof(DoiId))]
        public virtual Doi Doi { get; set; } = null!;

        public int VanDongVienId { get; set; }
        [ForeignKey(nameof(VanDongVienId))]
        public virtual VanDongVien VanDongVien { get; set; } = null!;

        [MaxLength(20)]
        public string? SoAo { get; set; }

        [MaxLength(100)]
        public string? ViTri { get; set; }

        public bool LaDoiTruong { get; set; } = false;

        public DateTime? NgayThamGia { get; set; }
    }
}
