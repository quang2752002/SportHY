using Dms.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("VanDongVien")]
    public class VanDongVien : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string HoTen { get; set; } = string.Empty;

        public int? DonViId { get; set; }
        [ForeignKey(nameof(DonViId))]
        public virtual DonVi? DonVi { get; set; }

        public DateTime? NgaySinh { get; set; }

        [Required]
        [MaxLength(20)]
        public string GioiTinh { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? SoDienThoai { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? SoCCCD { get; set; }

        [MaxLength(500)]
        public string? DiaChi { get; set; }

        [MaxLength(1000)]
        public string? HinhAnh { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<ThanhVienDoi> ThanhVienDois { get; set; } = new List<ThanhVienDoi>();
        public virtual ICollection<ChiTietDangKyThiDau> ChiTietDangKyThiDaus { get; set; } = new List<ChiTietDangKyThiDau>();
        public virtual ICollection<LichSuChuyenDoi> LichSuChuyenDois { get; set; } = new List<LichSuChuyenDoi>();
    }
}
