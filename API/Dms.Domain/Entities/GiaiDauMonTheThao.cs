using Dms.Domain.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("GiaiDauMonTheThao")]
    public class GiaiDauMonTheThao : BaseEntity
    {
        public int GiaiDauId { get; set; }
        [ForeignKey(nameof(GiaiDauId))]
        public virtual GiaiDau GiaiDau { get; set; } = null!;

        public int MonTheThaoId { get; set; }
        [ForeignKey(nameof(MonTheThaoId))]
        public virtual MonTheThao MonTheThao { get; set; } = null!;

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public int? NguoiDieuHanhId { get; set; }
        [ForeignKey(nameof(NguoiDieuHanhId))]
        public virtual TrongTai? NguoiDieuHanh { get; set; }

        // Các collection trực tiếp (không qua NoiDungThiDau nữa)
        public virtual ICollection<DangKyThiDau> DangKyThiDaus { get; set; } = new List<DangKyThiDau>();
        public virtual ICollection<BangDau> BangDaus { get; set; } = new List<BangDau>();
        public virtual ICollection<VongDau> VongDaus { get; set; } = new List<VongDau>();
        public virtual ICollection<TranDau> TranDaus { get; set; } = new List<TranDau>();
        public virtual ICollection<HuyChuong> HuyChuongs { get; set; } = new List<HuyChuong>();
    }
}
