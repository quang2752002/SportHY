using Dms.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("HiepDau")]
    public class HiepDau : BaseEntity
    {
        public int TranDauId { get; set; }
        [ForeignKey(nameof(TranDauId))]
        public virtual TranDau TranDau { get; set; } = null!;

        public int SoHiep { get; set; }

        [Required]
        [MaxLength(30)]
        public string LoaiHiep { get; set; } = "HiepChinh"; // "HiepChinh", "HiepPhu", "Penalty", "TieBreak", "SetQuyetDinh"

        [MaxLength(100)]
        public string? TenHiep { get; set; } // "Hiệp 1", "Set 1", "Hiệp phụ 1", "Loạt penalty"

        public DateTime? ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        [Required]
        [MaxLength(30)]
        public string TrangThai { get; set; } = "ChuaDau";

        [MaxLength(500)]
        public string? GhiChu { get; set; }

        public virtual ICollection<KetQuaHiepDau> KetQuaHiepDaus { get; set; } = new List<KetQuaHiepDau>();
    }
}
