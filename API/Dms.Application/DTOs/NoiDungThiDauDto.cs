using System;
using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    // ==================== NOI DUNG THI DAU DTOs ====================
    public class NoiDungThiDauDto
    {
        public int Id { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public int? GiaiDauId { get; set; }
        public int? MonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public string? TenGiaiDau { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string GioiTinh { get; set; } = "HonHop";
        public string LoaiThiDau { get; set; } = "CaNhan";
        public string? HinhThucThiDau { get; set; }
        public int? SoLuongToiThieu { get; set; }
        public int? SoLuongToiDa { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
        public int SoDangKy { get; set; }
        public int SoTranDau { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateNoiDungThiDauDto
    {
        public int? GiaiDauId { get; set; }
        public int? MonTheThaoId { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string GioiTinh { get; set; } = "HonHop";
        public string LoaiThiDau { get; set; } = "CaNhan";
        public string? HinhThucThiDau { get; set; }
        public int? SoLuongToiThieu { get; set; }
        public int? SoLuongToiDa { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
    }

    // ==================== VAN DONG VIEN DTOs ====================
    public class VanDongVienDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public int? DonViId { get; set; }
        public string? TenDonVi { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; } = "Nam";
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? SoCCCD { get; set; }
        public string? DiaChi { get; set; }
        public string? HinhAnh { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateVanDongVienDto
    {
        public string Ma { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public int? DonViId { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; } = "Nam";
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? SoCCCD { get; set; }
        public string? DiaChi { get; set; }
        public string? HinhAnh { get; set; }
        public bool TrangThai { get; set; } = true;
    }

    // ==================== DOI DTOs ====================
    public class DoiDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int? DonViId { get; set; }
        public string? TenDonVi { get; set; }
        public string? NguoiQuanLy { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
        public int SoThanhVien { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateDoiDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int? DonViId { get; set; }
        public string? NguoiQuanLy { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; } = true;
    }

    // ==================== DANG KY THI DAU DTOs ====================
    public class DangKyThiDauDto
    {
        public int Id { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public int? GiaiDauId { get; set; }
        public string? TenGiaiDau { get; set; }
        public int? MonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public int? DoiId { get; set; }
        public string? TenDoi { get; set; }
        public int? DonViId { get; set; }
        public string? TenDonVi { get; set; }
        public string SoDangKy { get; set; } = string.Empty;
        public string? TenDangKy { get; set; }
        public string TrangThai { get; set; } = "DaDuyet";
        public DateTime NgayDangKy { get; set; }
        public string? GhiChu { get; set; }
        public int SoVdv { get; set; }
        public List<int> VanDongVienIds { get; set; } = new();
        public List<string> VanDongVienNames { get; set; } = new();
        public List<ThanhVienDoiChiTietDto> ThanhVienDois { get; set; } = new();
        public List<ThanhVienDoiChiTietDto> ChiTietDangKyThiDaus => ThanhVienDois;
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class ThanhVienDoiChiTietDto
    {
        public int Id { get; set; }
        public int VanDongVienId { get; set; }
        public string? TenVanDongVien { get; set; }
        public string? HoTen { get; set; }
        public string? MaVanDongVien { get; set; }
        public string? SoAo { get; set; }
        public string? ViTri { get; set; }
        public bool LaDoiTruong { get; set; }
    }

    public class CreateUpdateDangKyThiDauDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public int? DoiId { get; set; }
        /// <summary>Nếu true: backend tự tạo Doi mới từ TenDoi + VanDongVienIds rồi gán DoiId</summary>
        public bool TuDongTaoDoi { get; set; } = false;
        /// <summary>Tên đội tự động tạo (dùng khi TuDongTaoDoi = true)</summary>
        public string? TenDoi { get; set; }
        /// <summary>Đơn vị chủ quản đội (dùng khi TuDongTaoDoi = true)</summary>
        public int? DonViId { get; set; }
        public string? SoDangKy { get; set; }
        public string? TenDangKy { get; set; }
        public string TrangThai { get; set; } = "DaDuyet";
        public DateTime NgayDangKy { get; set; } = DateTime.Now;
        public string? GhiChu { get; set; }
        public List<int>? VanDongVienIds { get; set; }
    }


    // ==================== BANG DAU & VONG DAU DTOs ====================
    public class ThanhVienBangDto
    {
        public int Id { get; set; }
        public int BangDauId { get; set; }
        public int DangKyThiDauId { get; set; }
        public string? TenDangKy { get; set; }
        public string? TenDoi { get; set; }
        public string? TenDonVi { get; set; }
        public int? HatGiong { get; set; }
        public int SoTran { get; set; }
        public int SoThang { get; set; }
        public int SoHoa { get; set; }
        public int SoThua { get; set; }
        public decimal DiemGhiDuoc { get; set; }
        public decimal DiemBiGhi { get; set; }
        public decimal Diem { get; set; }
        public int? XepHang { get; set; }
    }

    public class BangDauDto
    {
        public int Id { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int ThuTu { get; set; }
        public int SoDoi { get; set; }
        public List<ThanhVienBangDto> ThanhViens { get; set; } = new();
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateBangDauDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 1;
        public List<int>? DangKyThiDauIds { get; set; }
    }

    public class AssignTeamsToBangDto
    {
        public int BangDauId { get; set; }
        public List<int> DangKyThiDauIds { get; set; } = new();
    }

    public class AutoDistributeBangDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public int SoBang { get; set; } = 2;
        public string TienToBang { get; set; } = "Bảng ";
    }

    public class VongDauDto
    {
        public int Id { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int ThuTu { get; set; }
        public string? LoaiVongDau { get; set; }
        public int SoTran { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateVongDauDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 1;
        public string? LoaiVongDau { get; set; }
        public string? MoTa { get; set; }
    }

    // ==================== TRAN DAU & PHAN CONG TRONG TAI DTOs ====================
    public class PhanCongTrongTaiItemDto
    {
        public int Id { get; set; }
        public int TranDauId { get; set; }
        public int TrongTaiId { get; set; }
        public string? TenTrongTai { get; set; }
        public string? SoDienThoai { get; set; }
        public string? CapBac { get; set; }
        public string? VaiTro { get; set; } // 'TrongTaiChinh' | 'TrongTaiPhu' | 'TrongTaiBan' | 'GiamSat'
        public string? GhiChu { get; set; }
    }

    public class AssignTrongTaiDto
    {
        public int TrongTaiId { get; set; }
        public string? VaiTro { get; set; } = "TrongTaiChinh";
        public string? GhiChu { get; set; }
    }

    public class ThanhPhanTranDauItemDto
    {
        public int Id { get; set; }
        public int TranDauId { get; set; }
        public int DangKyThiDauId { get; set; }
        public string? TenDangKy { get; set; }
        public string? TenDoi { get; set; }
        public string? TenDonVi { get; set; }
        public int? SoLane { get; set; }
        public int? ViTri { get; set; } // 1: Đội 1 (Nhà), 2: Đội 2 (Khách)
        public string TrangThai { get; set; } = "ThamGia";
        public string? GhiChu { get; set; }
    }

    public class TranDauDto
    {
        public int Id { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public int? GiaiDauId { get; set; }
        public string? TenGiaiDau { get; set; }
        public int? MonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public int? DanhMucMonTheThaoId { get; set; }
        public string? TenDanhMucMonTheThao { get; set; }
        public int VongDauId { get; set; }
        public string? TenVongDau { get; set; }
        public int? BangDauId { get; set; }
        public string? TenBangDau { get; set; }
        public int? SanDauId { get; set; }
        public string? TenSanDau { get; set; }
        public string? TenCumSan { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public string? GhiChu { get; set; }

        // Đội 1 (Nhà/Vị trí 1)
        public int? Doi1DangKyId { get; set; }
        public string? TenDoi1 { get; set; }
        public string? DonViDoi1 { get; set; }

        // Đội 2 (Khách/Vị trí 2)
        public int? Doi2DangKyId { get; set; }
        public string? TenDoi2 { get; set; }
        public string? DonViDoi2 { get; set; }

        // Tỷ số và phân định
        public int? TySoDoi1 { get; set; }
        public int? TySoDoi2 { get; set; }
        public int? DiemPenaltyDoi1 { get; set; }
        public int? DiemPenaltyDoi2 { get; set; }
        public bool IsHoa { get; set; }
        public int? DoiThangDangKyId { get; set; }
        public int? DoiThuaDangKyId { get; set; }

        // Tiến trình Knockout
        public int? NextTranDauId { get; set; }
        public int? NextTranDauViTri { get; set; }
        public int? LoserNextTranDauId { get; set; }
        public int? LoserNextTranDauViTri { get; set; }
        public string? MaTranBracket { get; set; }

        public List<ThanhPhanTranDauItemDto> ThanhPhanTranDaus { get; set; } = new();
        public List<PhanCongTrongTaiItemDto> DanhSachTrongTai { get; set; } = new();

        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateTranDauDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public int VongDauId { get; set; }
        public int? BangDauId { get; set; }
        public int? SanDauId { get; set; }
        public int SoTran { get; set; } = 1;
        public string? TenTran { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public string? GhiChu { get; set; }

        // Cặp đấu
        public int? Doi1DangKyId { get; set; }
        public int? Doi2DangKyId { get; set; }

        public int? TySoDoi1 { get; set; }
        public int? TySoDoi2 { get; set; }
        public int? DiemPenaltyDoi1 { get; set; }
        public int? DiemPenaltyDoi2 { get; set; }
        public bool IsHoa { get; set; }
        public int? DoiThangDangKyId { get; set; }
        public int? DoiThuaDangKyId { get; set; }

        public int? NextTranDauId { get; set; }
        public int? NextTranDauViTri { get; set; }
        public int? LoserNextTranDauId { get; set; }
        public int? LoserNextTranDauViTri { get; set; }
        public string? MaTranBracket { get; set; }

        // Phân công trọng tài
        public List<AssignTrongTaiDto>? DanhSachTrongTai { get; set; }
    }

    public class UpdateMatchProgressDto
    {
        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public int? PenaltyScore1 { get; set; }
        public int? PenaltyScore2 { get; set; }
        public int? ExtraTimeScore1 { get; set; }
        public int? ExtraTimeScore2 { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public string? GhiChu { get; set; }
        public bool IsHoa { get; set; }
        public int? DoiThangDangKyId { get; set; }
        public int? DoiThuaDangKyId { get; set; }
    }

    // ==================== AUTO SCHEDULE & CONFLICT CHECK DTOs ====================
    public class AutoScheduleRequestDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public DateTime NgayBatDau { get; set; } = DateTime.Today;
        public string GioBatDauMoiNgay { get; set; } = "08:00";
        public string GioKetThucMoiNgay { get; set; } = "17:30";
        public int ThoiLuongTranPhut { get; set; } = 60;
        public int NghiGiuaTranPhut { get; set; } = 15;
        public List<int> SanDauIds { get; set; } = new();
        public List<int> TrongTaiIds { get; set; } = new();
        public int SoTrongTaiMoiTran { get; set; } = 1;
        public bool TaoBangDauNeuChuaCo { get; set; } = true;
        public int SoDoiMoiBang { get; set; } = 4;
        /// <summary>Số đội mỗi bảng vào vòng trong Knockout (1: Chỉ Nhất bảng; 2: Nhất và Nhì bảng. Mặc định: 2)</summary>
        public int SoDoiMoiBangVaoVongTrong { get; set; } = 2;
        /// <summary>Số đội thứ 3 có thành tích tốt nhất lấy thêm vào vòng Knockout (Mặc định: 0)</summary>
        public int SoDoiThu3TotNhat { get; set; } = 0;
        public bool XoaLichCu { get; set; } = false;
        public bool TranhTrungLichVdv { get; set; } = true;
        /// <summary>Số hiệp đấu mỗi trận (0 = không tự động tạo hiệp)</summary>
        public int? SoHiepDau { get; set; }
        /// <summary>Thời gian mỗi hiệp tính theo phút</summary>
        public int? ThoiGianMoiHiepPhut { get; set; }
        /// <summary>Thời gian nghỉ tối thiểu của VĐV giữa 2 trận liên tiếp (phút). Nếu null sẽ lấy từ CauHinhLichThiDau của môn</summary>
        public int? ThoiGianNghiToiThieuVdvPhut { get; set; }
        /// <summary>Bật chế độ phân bổ xoay tua cân bằng tải Trọng tài</summary>
        public bool CanBangTaiTrongTai { get; set; } = true;
        /// <summary>Bật chế độ chia đều mật độ thi đấu trên các sân</summary>
        public bool CanBangTaiSanDau { get; set; } = true;

        // --- CSP Smart Engine Parameters (Nếu null sẽ tự động ưu tiên lấy từ CauHinhLichThiDau theo Môn Thể Thao) ---
        /// <summary>Số trận tối đa mỗi đội/VĐV thi đấu trong 1 ngày</summary>
        public int? SoTranToiDaMoiDoiMoiNgay { get; set; }
        /// <summary>Mỗi vòng đấu thi đấu vào 1 ngày riêng biệt</summary>
        public bool? MoiVongMotNgay { get; set; }
        /// <summary>Khoảng cách giữa các vòng đấu (giờ)</summary>
        public int? KhoangCachGiuaCacVongGio { get; set; }
        /// <summary>Ưu tiên xếp trận Chung kết vào ngày bế mạc giải</summary>
        public bool? UuTienChungKetNgayCuoi { get; set; }
        /// <summary>Chia ca sáng / chiều (tránh giờ trưa)</summary>
        public bool? ChiaCaThiDau { get; set; }
        /// <summary>Buffer di chuyển VĐV thi đấu nhiều môn (phút)</summary>
        public int? ThoiGianDemDiChuyenPhut { get; set; }
        /// <summary>Số trận tối đa mỗi trọng tài bắt trong 1 ngày</summary>
        public int? SoTranToiDaMoiTrongTaiMoiNgay { get; set; }
        /// <summary>Thời gian đệm dọn sân / vệ sinh giữa các trận (phút)</summary>
        public int? ThoiGianDemDonSanPhut { get; set; }

        // --- Leaderboard / TinhDiemXepHang (Bơi lội, Điền kinh...) Parameters ---
        /// <summary>
        /// Số VĐV / làn / vị trí thi đấu mỗi lượt thi (Heat size).
        /// Ví dụ: 8 làn bơi → SoVdvMoiLuotThi = 8. Chỉ áp dụng cho TinhDiemXepHang.
        /// </summary>
        public int SoVdvMoiLuotThi { get; set; } = 8;

        /// <summary>
        /// Số vòng thi (1: Chung kết thẳng; 2: Vòng loại → Chung kết; 3: Sơ loại → Bán kết → Chung kết).
        /// Chỉ áp dụng cho TinhDiemXepHang.
        /// </summary>
        public int SoVongThi { get; set; } = 1;

        /// <summary>
        /// Phương thức phân nhóm VĐV vào lượt thi:
        /// "random" (bốc thăm ngẫu nhiên) | "registration_order" (theo thứ tự đăng ký) | "performance_seed" (xếp hạt giống - mạnh nhất vào lượt cuối).
        /// Chỉ áp dụng cho TinhDiemXepHang.
        /// </summary>
        public string PhuongThucPhanNhom { get; set; } = "random";

        // --- Round Robin / VongBang (Vòng tròn tính điểm) Parameters ---
        /// <summary>
        /// Số lượt thi đấu vòng tròn (1 = 1 lượt / Lượt đi; 2 = 2 lượt / Lượt đi - Lượt về). Mặc định: 1
        /// </summary>
        public int SoLuotDau { get; set; } = 1;

        /// <summary>
        /// Chế độ chia bảng: "single_group" (1 bảng đấu duy nhất gồm tất cả đội) | "multi_groups" (chia nhiều bảng đấu)
        /// </summary>
        public string CheDoVongBang { get; set; } = "single_group";

        /// <summary>
        /// Số bảng đấu chỉ định khi chia nhiều bảng (0 = tự động tính theo SoDoiMoiBang)
        /// </summary>
        public int SoBang { get; set; } = 0;

        /// <summary>
        /// Điểm số cho trận Thắng (mặc định: 3)
        /// </summary>
        public decimal DiemThang { get; set; } = 3;

        /// <summary>
        /// Điểm số cho trận Hòa (mặc định: 1)
        /// </summary>
        public decimal DiemHoa { get; set; } = 1;

        /// <summary>
        /// Điểm số cho trận Thua (mặc định: 0)
        /// </summary>
        public decimal DiemThua { get; set; } = 0;
    }

    public class AutoScheduleResultDto
    {
        public bool Success { get; set; }
        public int TotalMatchesCreated { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TranDauDto> Matches { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, int>? ThongKeSanDau { get; set; }
        public Dictionary<string, int>? ThongKeTrongTai { get; set; }
        public int SoNgayThiDau { get; set; } = 1;
    }

    public class ConflictCheckRequestDto
    {
        public int? TranDauId { get; set; } // Nếu cập nhật
        public int? SanDauId { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public List<int>? TrongTaiIds { get; set; }
        public List<int>? DangKyThiDauIds { get; set; }
    }

    public class ConflictDetailDto
    {
        public string LoaiXungDot { get; set; } = string.Empty; // "VanDongVien" | "SanDau" | "TrongTai" | "Doi"
        public string ThongBao { get; set; } = string.Empty;
        public int? VanDongVienId { get; set; }
        public string? TenVanDongVien { get; set; }
        public string? MaVanDongVien { get; set; }
        public string? TenDoiHienTai { get; set; }
        public int? TranDauBiTrungId { get; set; }
        public string? TenTranBiTrung { get; set; }
        public string? TenMonTheThao { get; set; }
        public string? TenNoiDung { get; set; }
        public string? TenSanDau { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
    }

    public class ConflictCheckResultDto
    {
        public bool HasConflict { get; set; }
        public List<string> Conflicts { get; set; } = new();
        public List<ConflictDetailDto> ChiTietXungDot { get; set; } = new();
    }

    public class TournamentConflictReportDto
    {
        public bool HasConflict { get; set; }
        public int GiaiDauId { get; set; }
        public string? TenGiaiDau { get; set; }
        public int TotalMatchesChecked { get; set; }
        public int TotalConflicts { get; set; }
        public List<string> Conflicts { get; set; } = new();
        public List<ConflictDetailDto> ChiTietXungDot { get; set; } = new();
    }

    // ==================== HUY CHUONG DTOs ====================
    public class HuyChuongDto
    {
        public int Id { get; set; }
        public int GiaiDauId { get; set; }
        public string? TenGiaiDau { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public int DangKyThiDauId { get; set; }
        public string? TenDangKy { get; set; }
        public string? TenDonVi { get; set; }
        public string? TenVanDongVien { get; set; }
        public string? TenDoi { get; set; }
        public int LoaiHuyChuongId { get; set; }
        public string? TenLoaiHuyChuong { get; set; }
        public int XepHang { get; set; }
        public DateTime? NgayTrao { get; set; }
        public string? GhiChu { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateHuyChuongDto
    {
        public int GiaiDauId { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public int DangKyThiDauId { get; set; }
        public int LoaiHuyChuongId { get; set; }
        public int XepHang { get; set; } = 1;
        public DateTime? NgayTrao { get; set; }
        public string? GhiChu { get; set; }
    }

    // ==================== LOAI HUY CHUONG DTOs ====================
    public class LoaiHuyChuongDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int ThuTu { get; set; }
        public int SoLuongDaTrao { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class CreateUpdateLoaiHuyChuongDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 1;
    }

    // ==================== MANUAL PAIRING DTOs (XẾP CẶP THỦ CÔNG) ====================
    /// <summary>
    /// Thông tin đội hoặc vận động viên tham gia xếp cặp đấu thủ công
    /// </summary>
    public class ManualPairingTeamDto
    {
        public int DangKyThiDauId { get; set; }
        public string TenDangKy { get; set; } = string.Empty;
        public string? TenDoi { get; set; }
        public string? TenDonVi { get; set; }
        public string? SoDangKy { get; set; }
        public int? BangDauId { get; set; }
        public string? TenBangDau { get; set; }
        public List<string> VanDongVienNames { get; set; } = new();
    }

    /// <summary>
    /// Thông tin VĐV / đoàn tham gia trong một lượt thi đấu nhiều làn (Heat / Điền kinh / Bơi lội)
    /// </summary>
    public class ManualPairingParticipantDto
    {
        public int DangKyThiDauId { get; set; }
        public string? TenDangKy { get; set; }
        public string? TenDoi { get; set; }
        public string? TenDonVi { get; set; }
        public int? SoLane { get; set; }
        public int? ViTri { get; set; }
        public List<string> VanDongVienNames { get; set; } = new();
    }

    /// <summary>
    /// Thông tin một cặp đấu hoặc lượt thi hiển thị trong layout xếp cặp
    /// </summary>
    public class ManualMatchPairDto
    {
        public int? TranDauId { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public int? VongDauId { get; set; }
        public string? TenVongDau { get; set; }
        public int? BangDauId { get; set; }
        public string? TenBangDau { get; set; }
        public int? SanDauId { get; set; }
        public string? TenSanDau { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
        public int? DiemDoi1 { get; set; }
        public int? DiemDoi2 { get; set; }
        public int? DiemPenaltyDoi1 { get; set; }
        public int? DiemPenaltyDoi2 { get; set; }
        public int? DoiThangDangKyId { get; set; }
        public int? DoiThuaDangKyId { get; set; }
        public string? MaTranBracket { get; set; }
        public int? NextTranDauId { get; set; }
        public int? NextTranDauViTri { get; set; }
        public int? LoserNextTranDauId { get; set; }
        public int? LoserNextTranDauViTri { get; set; }
        public int VongThuTu { get; set; } = 1;
        public bool IsHeat { get; set; }
        public ManualPairingTeamDto? Doi1 { get; set; }
        public ManualPairingTeamDto? Doi2 { get; set; }
        public List<ManualPairingParticipantDto> DanhSachVdv { get; set; } = new();
    }

    /// <summary>
    /// Dữ liệu khởi tạo cho màn hình xếp cặp thi đấu thủ công
    /// </summary>
    public class ManualPairingDataDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public string? HinhThucThiDau { get; set; }
        public bool LaMonDongDoi { get; set; }
        public List<VongDauDto> VongDaus { get; set; } = new();
        public List<BangDauDto> BangDaus { get; set; } = new();
        public List<SanDauDto> SanDaus { get; set; } = new();
        public List<ManualPairingTeamDto> AllTeams { get; set; } = new();
        public List<ManualPairingTeamDto> UnpairedTeams { get; set; } = new();
        public List<ManualMatchPairDto> ExistingPairs { get; set; } = new();
    }

    /// <summary>
    /// Request lưu danh sách các cặp đấu xếp thủ công
    /// </summary>
    public class SaveManualPairingRequestDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public int? VongDauId { get; set; }
        public int? BangDauId { get; set; }
        public List<ManualPairSaveItemDto> Pairs { get; set; } = new();
    }

    /// <summary>
    /// Dữ liệu từng cặp đấu trong yêu cầu lưu
    /// </summary>
    public class ManualPairSaveItemDto
    {
        public int? TranDauId { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public int? VongDauId { get; set; }
        public int? BangDauId { get; set; }
        public int? SanDauId { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public int? Doi1DangKyId { get; set; }
        public int? Doi2DangKyId { get; set; }
    }
}
