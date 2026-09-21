using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("ThanhVienBang")]
    public class ThanhVienBang : BaseEntity
    {
        public int BangDauId { get; set; }
        [ForeignKey(nameof(BangDauId))]
        public virtual BangDau BangDau { get; set; } = null!;

        public int DangKyThiDauId { get; set; }
        [ForeignKey(nameof(DangKyThiDauId))]
        public virtual DangKyThiDau DangKyThiDau { get; set; } = null!;

        public int? HatGiong { get; set; }

        public int SoTran { get; set; } = 0;

        public int SoThang { get; set; } = 0;

        public int SoHoa { get; set; } = 0;

        public int SoThua { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal DiemGhiDuoc { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal DiemBiGhi { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal HieuSo { get; set; } = 0;

        public int SoSetThang { get; set; } = 0;

        public int SoSetThua { get; set; } = 0;

        [Column(TypeName = "decimal(18,3)")]
        public decimal Diem { get; set; } = 0;

        public int? XepHang { get; set; }
    }
}
