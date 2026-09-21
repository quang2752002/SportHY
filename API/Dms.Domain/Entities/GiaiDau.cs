using Dms.Domain.Common;
using Dms.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("GiaiDau")]
    public class GiaiDau : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Ma { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Ten { get; set; } = string.Empty;

        [MaxLength(350)]
        public string? Slug { get; set; }

        [MaxLength(1000)]
        public string? HinhAnh { get; set; }

        [MaxLength(2000)]
        public string? MoTa { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public DateTime? HanDangKy { get; set; }

        [MaxLength(500)]
        public string? DiaDiem { get; set; }

        [Required]
        public PhamViGiaiDau PhamVi { get; set; } = PhamViGiaiDau.TatCa;

        [Required]
        public TrangThaiGiaiDau TrangThai { get; set; } = TrangThaiGiaiDau.Nhap;

        public int? TruongBanTrongTaiId { get; set; }
        [ForeignKey(nameof(TruongBanTrongTaiId))]
        public virtual TrongTai? TruongBanTrongTai { get; set; }

        public virtual ICollection<GiaiDauKhoi> GiaiDauKhois { get; set; } = new List<GiaiDauKhoi>();
        public virtual ICollection<GiaiDauMonTheThao> GiaiDauMonTheThaos { get; set; } = new List<GiaiDauMonTheThao>();
        public virtual ICollection<HuyChuong> HuyChuongs { get; set; } = new List<HuyChuong>();
        public virtual ICollection<DieuLeGiaiDau> DieuLeGiaiDaus { get; set; } = new List<DieuLeGiaiDau>();
    }
}
