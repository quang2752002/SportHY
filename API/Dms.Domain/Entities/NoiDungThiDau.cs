using Dms.Domain.Common;
using Dms.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("NoiDungThiDau")]
    public class NoiDungThiDau : BaseEntity
    {
        public int GiaiDauMonTheThaoId { get; set; }
        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao GiaiDauMonTheThao { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Ten { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string GioiTinh { get; set; } = "HonHop";

        [Required]
        [MaxLength(30)]
        public string LoaiThiDau { get; set; } = "CaNhan";

        public HinhThucThiDau? HinhThucThiDau { get; set; }

        public int? SoLuongToiThieu { get; set; }

        public int? SoLuongToiDa { get; set; }

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<DangKyThiDau> DangKyThiDaus { get; set; } = new List<DangKyThiDau>();
        public virtual ICollection<BangDau> BangDaus { get; set; } = new List<BangDau>();
        public virtual ICollection<VongDau> VongDaus { get; set; } = new List<VongDau>();
        public virtual ICollection<TranDau> TranDaus { get; set; } = new List<TranDau>();
        public virtual ICollection<HuyChuong> HuyChuongs { get; set; } = new List<HuyChuong>();
    }
}
