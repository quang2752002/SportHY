using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("PhanCongDieuHanhMon")]
    public class PhanCongDieuHanhMon : BaseEntity
    {
        public int GiaiDauId { get; set; }
        [ForeignKey(nameof(GiaiDauId))]
        public virtual GiaiDau GiaiDau { get; set; } = null!;

        public int DanhMucId { get; set; }
        [ForeignKey(nameof(DanhMucId))]
        public virtual DanhMucMonTheThao DanhMucMonTheThao { get; set; } = null!;

        public int NguoiDieuHanhMonId { get; set; }
        [ForeignKey(nameof(NguoiDieuHanhMonId))]
        public virtual NguoiDieuHanhMon NguoiDieuHanhMon { get; set; } = null!;
    }
}
