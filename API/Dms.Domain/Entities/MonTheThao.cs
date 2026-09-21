using Dms.Domain.Common;
using Dms.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("MonTheThao")]
    public class MonTheThao : BaseEntity
    {
        public int DanhMucId { get; set; }
        [ForeignKey(nameof(DanhMucId))]
        public virtual DanhMucMonTheThao DanhMuc { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? MoTa { get; set; }

        public bool LaMonDongDoi { get; set; } = false;

        [Required]
        [MaxLength(20)]
        public string GioiTinh { get; set; } = "HonHop";

        public HinhThucThiDau HinhThucThiDau { get; set; } = HinhThucThiDau.LoaiTrucTiep;

        public int? SoLuongVanDongVienToiThieu { get; set; }
        public int? SoLuongVanDongVienToiDa { get; set; }
        public int? SoDoiToiDa { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<GiaiDauMonTheThao> GiaiDauMonTheThaos { get; set; } = new List<GiaiDauMonTheThao>();
        public virtual ICollection<DieuLeMonTheThao> DieuLeMonTheThaos { get; set; } = new List<DieuLeMonTheThao>();
        public virtual ICollection<SanDau> SanDaus { get; set; } = new List<SanDau>();
        public virtual CauHinhLichThiDau? CauHinhLichThiDau { get; set; }
    }
}
