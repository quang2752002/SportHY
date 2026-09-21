using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("DonVi")]
    public class DonVi : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Ten { get; set; } = string.Empty;

        public int? KhoiId { get; set; }
        [ForeignKey(nameof(KhoiId))]
        public virtual Khoi? Khoi { get; set; }

        public int? DonViChaId { get; set; }
        [ForeignKey(nameof(DonViChaId))]
        public virtual DonVi? DonViCha { get; set; }

        [MaxLength(30)]
        public string? LoaiDonVi { get; set; }

        [MaxLength(500)]
        public string? DiaChi { get; set; }

        [MaxLength(200)]
        public string? NguoiDaiDien { get; set; }

        [MaxLength(30)]
        public string? SoDienThoai { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        [MaxLength(1000)]
        public string? HinhAnh { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<DonVi> DonViCon { get; set; } = new List<DonVi>();
        public virtual ICollection<VanDongVien> VanDongViens { get; set; } = new List<VanDongVien>();
        public virtual ICollection<Doi> Dois { get; set; } = new List<Doi>();
    }
}
