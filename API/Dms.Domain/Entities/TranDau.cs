using Dms.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    [Table("TranDau")]
    public class TranDau : BaseEntity
    {
        public int GiaiDauMonTheThaoId { get; set; }
        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao GiaiDauMonTheThao { get; set; } = null!;

        public int VongDauId { get; set; }
        [ForeignKey(nameof(VongDauId))]
        public virtual VongDau VongDau { get; set; } = null!;

        public int? BangDauId { get; set; }
        [ForeignKey(nameof(BangDauId))]
        public virtual BangDau? BangDau { get; set; }

        public int? SanDauId { get; set; }
        [ForeignKey(nameof(SanDauId))]
        public virtual SanDau? SanDau { get; set; }

        public int SoTran { get; set; }

        [MaxLength(300)]
        public string? TenTran { get; set; }

        public DateTime? ThoiGianDuKien { get; set; }

        public DateTime? ThoiGianBatDau { get; set; }

        public DateTime? ThoiGianKetThuc { get; set; }

        [Required]
        [MaxLength(30)]
        public string TrangThai { get; set; } = "ChuaDau";

        [MaxLength(30)]
        public string TrangThaiDuyetKetQua { get; set; } = "ChoDuyet";

        public DateTime? ThoiGianDuyetKetQua { get; set; }

        [MaxLength(256)]
        public string? NguoiDuyetKetQua { get; set; }

        [MaxLength(1000)]
        public string? GhiChuDuyetKetQua { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? GhiChu { get; set; }

        // --- TIẾN TRÌNH NHÁNH ĐẤU KNOCKOUT (DAG BRACKET PROGRESSION) ---
        public int? NextTranDauId { get; set; }
        [ForeignKey(nameof(NextTranDauId))]
        public virtual TranDau? NextTranDau { get; set; }

        public int? NextTranDauViTri { get; set; } // 1: Đội 1, 2: Đội 2 trong trận tiếp theo

        public int? LoserNextTranDauId { get; set; } // Trận tranh hạng 3 cho đội thua Bán kết
        [ForeignKey(nameof(LoserNextTranDauId))]
        public virtual TranDau? LoserNextTranDau { get; set; }

        public int? LoserNextTranDauViTri { get; set; } // 1: Đội 1, 2: Đội 2 trong trận tranh hạng 3

        public int? DoiThangDangKyId { get; set; }
        [ForeignKey(nameof(DoiThangDangKyId))]
        public virtual DangKyThiDau? DoiThangDangKy { get; set; }

        public int? DoiThuaDangKyId { get; set; }
        [ForeignKey(nameof(DoiThuaDangKyId))]
        public virtual DangKyThiDau? DoiThuaDangKy { get; set; }

        public bool IsHoa { get; set; } = false;

        public int? TySoDoi1 { get; set; }
        public int? TySoDoi2 { get; set; }

        public int? DiemPenaltyDoi1 { get; set; }
        public int? DiemPenaltyDoi2 { get; set; }

        [MaxLength(50)]
        public string? MaTranBracket { get; set; } // Ví dụ: "QF1", "QF2", "SF1", "SF2", "FINAL", "BRONZE"

        [MaxLength(50)]
        public string? MaTranHienThi { get; set; } // Ví dụ: "M01", "BD-01", "CK"

        public bool IsLichCoDinh { get; set; } = false;

        public virtual ICollection<ThanhPhanTranDau> ThanhPhanTranDaus { get; set; } = new List<ThanhPhanTranDau>();
        public virtual ICollection<HiepDau> HiepDaus { get; set; } = new List<HiepDau>();
        public virtual ICollection<PhanCongTrongTai> PhanCongTrongTais { get; set; } = new List<PhanCongTrongTai>();
    }
}
