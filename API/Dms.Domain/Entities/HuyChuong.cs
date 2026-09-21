using Dms.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("HuyChuong")]
    public class HuyChuong : BaseEntity
    {
        public int GiaiDauId { get; set; }
        [ForeignKey(nameof(GiaiDauId))]
        public virtual GiaiDau GiaiDau { get; set; } = null!;

        public int GiaiDauMonTheThaoId { get; set; }
        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao GiaiDauMonTheThao { get; set; } = null!;

        public int DangKyThiDauId { get; set; }
        [ForeignKey(nameof(DangKyThiDauId))]
        public virtual DangKyThiDau DangKyThiDau { get; set; } = null!;

        public int LoaiHuyChuongId { get; set; }
        [ForeignKey(nameof(LoaiHuyChuongId))]
        public virtual LoaiHuyChuong LoaiHuyChuong { get; set; } = null!;

        public int XepHang { get; set; }

        public DateTime? NgayTrao { get; set; }

        [MaxLength(1000)]
        public string? GhiChu { get; set; }
    }
}
