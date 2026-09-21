using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("ThanhPhanTranDau")]
    public class ThanhPhanTranDau : BaseEntity
    {
        public int TranDauId { get; set; }
        [ForeignKey(nameof(TranDauId))]
        public virtual TranDau TranDau { get; set; } = null!;

        public int DangKyThiDauId { get; set; }
        [ForeignKey(nameof(DangKyThiDauId))]
        public virtual DangKyThiDau DangKyThiDau { get; set; } = null!;

        public int? SoLane { get; set; }

        public int? ThuTuThiDau { get; set; }

        public int? ViTri { get; set; }

        [Required]
        [MaxLength(30)]
        public string TrangThai { get; set; } = "ThamGia";

        [MaxLength(500)]
        public string? GhiChu { get; set; }

        public virtual ICollection<KetQuaHiepDau> KetQuaHiepDaus { get; set; } = new List<KetQuaHiepDau>();
        public virtual ICollection<KetQuaTranDau> KetQuaTranDaus { get; set; } = new List<KetQuaTranDau>();
    }
}
