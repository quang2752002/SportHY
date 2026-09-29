using Dms.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dms.Domain.Entities
{
    /// <summary>
    /// Thực thể cấu hình thể thức thi đấu và luật tính điểm theo môn thể thao hoặc giải đấu cụ thể.
    /// Hỗ trợ cả môn đối kháng (bóng đá, cầu lông, bóng chuyền...) và môn đo thành tích (điền kinh, bơi lội...).
    /// </summary>
    [Table("CauHinhTheThucThiDau")]
    public class CauHinhTheThucThiDau : BaseEntity
    {
        public int MonTheThaoId { get; set; }
        [ForeignKey(nameof(MonTheThaoId))]
        public virtual MonTheThao MonTheThao { get; set; } = null!;

        public int? GiaiDauMonTheThaoId { get; set; }
        [ForeignKey(nameof(GiaiDauMonTheThaoId))]
        public virtual GiaiDauMonTheThao? GiaiDauMonTheThao { get; set; }

        /// <summary>
        /// Loại thể thức cốt lõi:
        /// "SetDiem" (Cầu lông, Bóng chuyền, Bóng bàn, Tennis),
        /// "ThoiGianHiep" (Bóng đá, Futsal, Bóng rổ),
        /// "TinhDiemXepHang" (Điền kinh chạy, Bơi lội, Nhảy xa, Cử tạ, Bắn súng).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LoaiTheThuc { get; set; } = "SetDiem";

        // ==========================================
        // NHÓM 1: CẤU HÌNH ĐỐI KHÁNG (BÓNG ĐÁ, CẦU LÔNG, BÓNG CHUYỀN...)
        // ==========================================
        public int SoHiepToiDa { get; set; } = 3;
        public int? SoHiepThangDeThangTran { get; set; } = 2;
        public int? DiemMoiHiep { get; set; } = 21;
        public int? DiemHiepQuyetDinh { get; set; } = 21;
        public int CachBietDiemToiThieu { get; set; } = 2;
        public int? DiemToiDaMoiHiep { get; set; } = 30;
        public int? ThoiGianHiepChinhPhut { get; set; } = 0;

        // Phân định hòa & Hiệp phụ / Penalty
        public bool ChoPhepHoaVongBang { get; set; } = true;
        public bool ChoPhepHoaKnockout { get; set; } = false;
        public bool CoHiepPhu { get; set; } = false;
        public int? SoHiepPhu { get; set; } = 2;
        public int? ThoiGianHiepPhuPhut { get; set; } = 15;
        public bool CoPenalty { get; set; } = false;
        public int? SoLuotPenaltyMoiDoi { get; set; } = 5;
        public bool CoTieBreak { get; set; } = false;
        public bool CoThePhat { get; set; } = false;

        // Điểm số vòng bảng
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiemThang { get; set; } = 3;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiemHoa { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiemThua { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiemThuaBocCuoc { get; set; } = 0;

        /// <summary>
        /// Bóng chuyền: Thắng 3-0/3-1: 3đ; Thắng 3-2: 2đ; Thua 2-3: 1đ; Thua 0-3: 0đ.
        /// </summary>
        public bool CachTinhDiemTheoSet { get; set; } = false;

        /// <summary>
        /// Thứ tự ưu tiên tiêu chí xếp hạng vòng bảng lưu dưới dạng JSON mảng chuỗi.
        /// Ví dụ: ["Diem","HieuSo","DiemGhiDuoc","DoiDau","SoTranThang"]
        /// </summary>
        [Required]
        public string TieuChiXepHangJson { get; set; } = "[\"Diem\",\"HieuSo\",\"DiemGhiDuoc\",\"DoiDau\",\"SoTranThang\"]";

        // ==========================================
        // NHÓM 2: CẤU HÌNH ĐO THÀNH TÍCH (ĐIỀN KINH, BƠI LỘI, CỬ TẠ, BẮN SÚNG)
        // ==========================================
        [MaxLength(30)]
        public string? LoaiDoThanhTich { get; set; } = "ThoiGian"; // "ThoiGian", "KhoangCach", "KhoiLuong", "DiemSo"

        [MaxLength(20)]
        public string? DonViThanhTich { get; set; } = "giay"; // "giay", "phut_giay", "met", "kg", "diem"

        [MaxLength(30)]
        public string? TieuChiXepHangThanhTich { get; set; } = "CangNhoCangTot"; // "CangNhoCangTot" (Bơi/Chạy), "CangLonCangTot" (Nhảy/Cử tạ/Bắn súng)

        [MaxLength(100)]
        public string? TenTieuChiPhuThanhTich { get; set; } = "Chỉ số phụ";
        public bool TieuChiPhuCangNhoCangTot { get; set; } = true;

        public int SoVdvMoiLuotThi { get; set; } = 8; // Số làn (Lane 1..8)

        [MaxLength(50)]
        public string? QuyCachTienVaoChungKet { get; set; } = "TopNToanVong"; // "ChungKetTrucTiep", "TopNToanVong", "TopMoiLuotVaVeVot"

        public int? SoVdvVaoChungKet { get; set; } = 8;

        [Column(TypeName = "decimal(18,4)")]
        public decimal? KyLucHienTai { get; set; }

        [MaxLength(100)]
        public string? KyLucHienTaiText { get; set; }

        // ==========================================
        // NHÓM 3: CẤU HÌNH THỜI LƯỢNG & XẾP LỊCH THI ĐẤU (TỰ ĐỘNG XẾP LỊCH)
        // ==========================================
        /// <summary>Thời lượng mỗi trận đấu (phút). Mặc định: 60</summary>
        public int ThoiLuongTranPhut { get; set; } = 60;

        /// <summary>Thời gian nghỉ giữa 2 trận đấu / đệm dọn sân (phút). Mặc định: 15</summary>
        public int NghiGiuaTranPhut { get; set; } = 15;

        /// <summary>Số bảng đấu (0: Tự động chia theo số đội, 1: 1 bảng, 2: 2 bảng...)</summary>
        public int SoBang { get; set; } = 0;

        /// <summary>Số đội mỗi bảng (dùng khi chọn tự động chia, mặc định: 4)</summary>
        public int SoDoiMoiBang { get; set; } = 4;

        /// <summary>Số đội mỗi bảng vào vòng Knockout (1: Chỉ Nhất bảng, 2: Nhất và Nhì bảng. Mặc định: 2)</summary>
        public int SoDoiMoiBangVaoVongTrong { get; set; } = 2;

        /// <summary>Số vòng thi đấu đối với môn đo thành tích / Heat (1: Chung kết thẳng, 2: Vòng loại -> Chung kết, 3: Sơ loại -> Bán kết -> Chung kết)</summary>
        public int SoVongThi { get; set; } = 2;

        /// <summary>Phương thức phân nhóm / xếp lượt cho môn Heat: "random" (ngẫu nhiên), "performance_seed" (hạt giống), "registration_order" (thứ tự đăng ký)</summary>
        [MaxLength(50)]
        public string PhuongThucPhanNhom { get; set; } = "random";
    }
}
