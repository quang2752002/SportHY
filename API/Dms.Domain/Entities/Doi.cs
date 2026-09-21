using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("Doi")]
    public class Doi : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Ten { get; set; } = string.Empty;

        public int? DonViId { get; set; }
        [ForeignKey(nameof(DonViId))]
        public virtual DonVi? DonVi { get; set; }

        [MaxLength(200)]
        public string? NguoiQuanLy { get; set; }

        [MaxLength(30)]
        public string? SoDienThoai { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<ThanhVienDoi> ThanhVienDois { get; set; } = new List<ThanhVienDoi>();
        public virtual ICollection<DangKyThiDau> DangKyThiDaus { get; set; } = new List<DangKyThiDau>();
    }
}
