using Dms.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("DangKyThiDau")]
    public class DangKyThiDau : BaseEntity
    {
        public int GiaiDauMonTheThaoId { get; set; }
        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao GiaiDauMonTheThao { get; set; } = null!;

        public int? DoiId { get; set; }
        [ForeignKey(nameof(DoiId))]
        public virtual Doi? Doi { get; set; }

        [Required]
        [MaxLength(50)]
        public string SoDangKy { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? TenDangKy { get; set; }

        [Required]
        [MaxLength(30)]
        public string TrangThai { get; set; } = "DaDuyet";

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [MaxLength(1000)]
        public string? GhiChu { get; set; }

        public virtual ICollection<ChiTietDangKyThiDau> ChiTietDangKyThiDaus { get; set; } = new List<ChiTietDangKyThiDau>();
        public virtual ICollection<ThanhVienBang> ThanhVienBangs { get; set; } = new List<ThanhVienBang>();
        public virtual ICollection<ThanhPhanTranDau> ThanhPhanTranDaus { get; set; } = new List<ThanhPhanTranDau>();
        public virtual HuyChuong? HuyChuong { get; set; }
    }
}
