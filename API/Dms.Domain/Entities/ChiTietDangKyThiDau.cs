using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("ChiTietDangKyThiDau")]
    public class ChiTietDangKyThiDau : BaseEntity
    {
        public int DangKyThiDauId { get; set; }
        [ForeignKey(nameof(DangKyThiDauId))]
        public virtual DangKyThiDau DangKyThiDau { get; set; } = null!;

        public int VanDongVienId { get; set; }
        [ForeignKey(nameof(VanDongVienId))]
        public virtual VanDongVien VanDongVien { get; set; } = null!;

        public int? SoThuTu { get; set; }

        [MaxLength(100)]
        public string? VaiTro { get; set; }
    }
}
