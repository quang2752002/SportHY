using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("ThuKy")]
    public class ThuKy : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string HoTen { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? GioiTinh { get; set; }

        [MaxLength(30)]
        public string? SoDienThoai { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? ChucVu { get; set; }

        [MaxLength(200)]
        public string? DonViCongTac { get; set; }

        public bool TrangThai { get; set; } = true;
    }
}
