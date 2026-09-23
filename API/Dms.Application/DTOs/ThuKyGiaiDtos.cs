using System;
using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    // ==================== COMMON MATCH DETAIL DTOs ====================
    public class SetScoreDto
    {
        public int SetNumber { get; set; }
        public int Score1 { get; set; }
        public int Score2 { get; set; }
    }

    public class MatchEventItemDto
    {
        public int Id { get; set; }
        public int Minute { get; set; }
        public string Type { get; set; } = "goal"; // goal, yellow_card, red_card, foul, sub, point
        public int Team { get; set; } = 1; // 1: Đội 1, 2: Đội 2
        public string? Player { get; set; }
        public string? Notes { get; set; }
    }

    public class SignatureInfoDto
    {
        public string? SignerName { get; set; }
        public string? Role { get; set; }
        public DateTime? SignedAt { get; set; }
    }

    // ==================== DASHBOARD DTOs ====================
    public class ThuKyDashboardDto
    {
        public int GiaiDauId { get; set; }
        public string TenGiaiDau { get; set; } = string.Empty;
        public int TongSoMon { get; set; }
        public int TongSoNoiDung { get; set; }
        public int TongSoTranDau { get; set; }
        public int SoTranDaHoanThanh { get; set; }
        public int SoTranDangDienRa { get; set; }
        public int SoTranChuaDau { get; set; }
        public int SoKetQuaChoDuyet { get; set; }
        public int SoKetQuaDaDuyet { get; set; }
        public int SoKetQuaCanKiemTra { get; set; }
        public int TongSoBienBan { get; set; }
        public int SoBienBanDuChuKy { get; set; }
        public int SoBienBanThieuChuKy { get; set; }
        public int TongSoHuyChuongDaTrao { get; set; }
        public double TiLeHoanThanh { get; set; }

        public List<ThuKyTienDoMonDto> TienDoCacMon { get; set; } = new();
        public List<ThuKyKetQuaDto> KetQuaMoiNhat { get; set; } = new();
        public List<ThuKyKetQuaDto> CanhBaoCanXuLy { get; set; } = new();
        public List<HuyChuongDoanDto> TopHuyChuong { get; set; } = new();
    }

    // ==================== THEO DÕI MÔN VÀ DANH MỤC MÔN DTOs ====================
    public class ThuKyNoiDungDto
    {
        public int Id { get; set; } // GiaiDauMonTheThaoId
        public int GiaiDauId { get; set; }
        public string? TenGiaiDau { get; set; }
        public int MonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public string? MaMonTheThao { get; set; }
        public int DanhMucMonTheThaoId { get; set; }
        public string? MaDanhMucMonTheThao { get; set; }
        public string? TenDanhMucMonTheThao { get; set; }
        public string? TenNoiDung { get; set; }
        public string? GioiTinh { get; set; } // Nam, Nu, HonHop
        public string? LoaiThiDau { get; set; } // CaNhan, Doi, DongDoi
        public string? HinhThucThiDau { get; set; } // VongTron, LoaiTrucTiep, ChiaBang, TinhDiemXepHang
        public int SoDangKy { get; set; }
        public int SoTranDaXep { get; set; }
        public int SoTranDaHoanThanh { get; set; }
        public string TrangThai { get; set; } = "ChuaDau"; // ChuaDau, DangDau, HoanThanh
        public double PhanTramTienDo { get; set; }
    }

    public class ThuKyNoiDungChiTietDto
    {
        public ThuKyNoiDungDto ThongTinChung { get; set; } = new();
        public List<NoiDungDangKyItemDto> DanhSachDangKy { get; set; } = new();
        public List<TranDauDto> DanhSachTranDau { get; set; } = new();
        public List<VongDauDto> DanhSachVongDau { get; set; } = new();
        public List<BangDauDto> DanhSachBangDau { get; set; } = new();
    }

    public class NoiDungDangKyItemDto
    {
        public int DangKyThiDauId { get; set; }
        public string SoDangKy { get; set; } = string.Empty;
        public string? TenDangKy { get; set; }
        public string? TenDonVi { get; set; }
        public string? TenDoi { get; set; }
        public List<string> DanhSachVdv { get; set; } = new();
        public string TrangThai { get; set; } = string.Empty;
        public DateTime NgayDangKy { get; set; }
    }

    // ==================== TIẾN ĐỘ TỪNG MÔN DTOs ====================
    public class ThuKyTienDoMonDto
    {
        public int GiaiDauMonTheThaoId { get; set; }
        public int MonTheThaoId { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public string? MaMon { get; set; }
        public string? IconMon { get; set; }
        public int TongSoNoiDung { get; set; }
        public int TongSoTran { get; set; }
        public int SoTranDaHoanThanh { get; set; }
        public int SoTranDangDienRa { get; set; }
        public int SoTranChuaDau { get; set; }
        public double TiLeHoanThanh { get; set; }
        public int TongSoVdv { get; set; }
        public int SoHuyChuongDaTrao { get; set; }
        public int SoBienBanDaDuyet { get; set; }
        public int SoBienBanChoDuyet { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";
    }

    // ==================== KIỂM TRA & XÁC NHẬN KẾT QUẢ DTOs ====================
    public class ThuKyKetQuaDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public int GiaiDauMonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public string? TenVongDau { get; set; }
        public string? TenBangDau { get; set; }
        public string? TenSanDau { get; set; }
        public DateTime? ThoiGianDuKien { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThaiTranDau { get; set; } = "ChuaDau";

        public int? Doi1DangKyId { get; set; }
        public string? TenDoi1 { get; set; }
        public string? DonViDoi1 { get; set; }
        public int TySoDoi1 { get; set; }

        public int? Doi2DangKyId { get; set; }
        public string? TenDoi2 { get; set; }
        public string? DonViDoi2 { get; set; }
        public int TySoDoi2 { get; set; }

        public string? DoiThang { get; set; }
        public List<SetScoreDto> SetScores { get; set; } = new();

        // Trạng thái kiểm tra của Thư ký
        public string TrangThaiXacNhan { get; set; } = "ChuaCoKetQua"; // ChoXacNhan, DaXacNhan, CanKiemTraLai, ChuaCoKetQua
        public string? NguoiXacNhan { get; set; }
        public DateTime? NgayXacNhan { get; set; }
        public string? GhiChuThuKy { get; set; }
        public bool HasScoresheet { get; set; }
        public bool IsScoresheetComplete { get; set; }
        public int SoChuKy { get; set; }
    }

    // ==================== BIÊN BẢN TRẬN ĐẤU DTOs ====================
    public class ThuKyBienBanDto
    {
        public int TranDauId { get; set; }
        public int SoTran { get; set; }
        public string? TenTran { get; set; }
        public int GiaiDauId { get; set; }
        public string? TenGiaiDau { get; set; }
        public string? TenMonTheThao { get; set; }
        public string? TenNoiDung { get; set; }
        public string? TenVongDau { get; set; }
        public string? TenBangDau { get; set; }
        public string? TenSanDau { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; } = "ChuaDau";

        // Đội 1
        public string? TenDoi1 { get; set; }
        public string? DonViDoi1 { get; set; }
        public int Score1 { get; set; }
        public List<VanDongVienDto> VdvDoi1 { get; set; } = new();

        // Đội 2
        public string? TenDoi2 { get; set; }
        public string? DonViDoi2 { get; set; }
        public int Score2 { get; set; }
        public List<VanDongVienDto> VdvDoi2 { get; set; } = new();

        public string? Winner { get; set; }
        public string? Notes { get; set; }
        public List<SetScoreDto> SetScores { get; set; } = new();
        public List<MatchEventItemDto> Events { get; set; } = new();
        public List<PhanCongTrongTaiItemDto> DanhSachTrongTai { get; set; } = new();
        public Dictionary<string, SignatureInfoDto> Signatures { get; set; } = new();

        // Kiểm tra tính hợp lệ
        public bool DaDuChuKy => Signatures.ContainsKey("referee") && Signatures.ContainsKey("team1") && Signatures.ContainsKey("team2");
        public bool ThuKyDaXacNhan => Signatures.ContainsKey("secretary");
    }

    // ==================== BÁO CÁO & TỔNG HỢP DTOs ====================
    public class HuyChuongDoanDto
    {
        public int DonViId { get; set; }
        public string? MaDonVi { get; set; }
        public string TenDonVi { get; set; } = string.Empty;
        public int SoHuyChuongVang { get; set; }
        public int SoHuyChuongBac { get; set; }
        public int SoHuyChuongDong { get; set; }
        public int TongSoHuyChuong => SoHuyChuongVang + SoHuyChuongBac + SoHuyChuongDong;
        public int TongDiem => (SoHuyChuongVang * 3) + (SoHuyChuongBac * 2) + (SoHuyChuongDong * 1);
        public int XepHang { get; set; }
    }

    public class BangTongSapHuyChuongDto
    {
        public int GiaiDauId { get; set; }
        public string TenGiaiDau { get; set; } = string.Empty;
        public int? DanhMucMonTheThaoId { get; set; }
        public string? TenDanhMucMonTheThao { get; set; }
        public int? MonTheThaoId { get; set; }
        public string? TenMonTheThao { get; set; }
        public int TongSoHuyChuongVang { get; set; }
        public int TongSoHuyChuongBac { get; set; }
        public int TongSoHuyChuongDong { get; set; }
        public int TongSoHuyChuongDaTrao => TongSoHuyChuongVang + TongSoHuyChuongBac + TongSoHuyChuongDong;
        public List<HuyChuongDoanDto> BangXepHang { get; set; } = new();
        public DateTime NgayXuatBaoCao { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Bảng xếp hạng huy chương theo một danh mục môn thể thao
    /// </summary>
    public class BangXepHangTheoDanhMucDto
    {
        public int DanhMucId { get; set; }
        public string MaDanhMuc { get; set; } = string.Empty;
        public string TenDanhMuc { get; set; } = string.Empty;
        public int TongSoMon { get; set; }
        public int TongHuyChuongVang { get; set; }
        public int TongHuyChuongBac { get; set; }
        public int TongHuyChuongDong { get; set; }
        public int TongHuyChuong => TongHuyChuongVang + TongHuyChuongBac + TongHuyChuongDong;
        public List<HuyChuongDoanDto> BangXepHang { get; set; } = new();
    }

    /// <summary>
    /// Bảng xếp hạng huy chương theo một môn thể thao cụ thể
    /// </summary>
    public class BangXepHangTheoMonDto
    {
        public int MonTheThaoId { get; set; }
        public string MaMon { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public int DanhMucId { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public int TongHuyChuongVang { get; set; }
        public int TongHuyChuongBac { get; set; }
        public int TongHuyChuongDong { get; set; }
        public int TongHuyChuong => TongHuyChuongVang + TongHuyChuongBac + TongHuyChuongDong;
        public List<HuyChuongDoanDto> BangXepHang { get; set; } = new();
    }

    public class ThuKyBaoCaoTongHopDto
    {
        public int GiaiDauId { get; set; }
        public string TenGiaiDau { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? DiaDiem { get; set; }
        public int TongSoMon { get; set; }
        public int TongSoNoiDung { get; set; }
        public int TongSoDoan { get; set; }
        public int TongSoVdv { get; set; }
        public int TongSoTranDau { get; set; }
        public int SoTranDaDau { get; set; }
        public int SoTranDangDau { get; set; }
        public int SoTranChuaDau { get; set; }
        public double TiLeHoanThanh { get; set; }
        public int TongHuyChuongDaTrao { get; set; }
        public int TongBienBanDaKiemTra { get; set; }
        public int TongBienBanHopLe { get; set; }

        public BangTongSapHuyChuongDto BangTongSap { get; set; } = new();
        public List<ThuKyTienDoMonDto> TienDoMonList { get; set; } = new();
    }

    // ==================== TRA CỨU DỮ LIỆU DTOs ====================
    public class TraCuuVdvItemDto
    {
        public int Id { get; set; }
        public string MaVdv { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? SoCCCD { get; set; }
        public string? TenDonVi { get; set; }
        public List<string> CacMonThiDau { get; set; } = new();
        public List<string> ThanhTich { get; set; } = new();
    }

    public class TraCuuDonViItemDto
    {
        public int Id { get; set; }
        public string MaDonVi { get; set; } = string.Empty;
        public string TenDonVi { get; set; } = string.Empty;
        public string? NguoiLienHe { get; set; }
        public string? SoDienThoai { get; set; }
        public int SoVdvThamGia { get; set; }
        public int SoMonThamGia { get; set; }
        public int SoHuyChuongVang { get; set; }
        public int SoHuyChuongBac { get; set; }
        public int SoHuyChuongDong { get; set; }
    }

    public class ThuKyTraCuuResultDto
    {
        public string Keyword { get; set; } = string.Empty;
        public List<TraCuuVdvItemDto> VanDongViens { get; set; } = new();
        public List<TraCuuDonViItemDto> DonVis { get; set; } = new();
        public List<ThuKyKetQuaDto> TranDaus { get; set; } = new();
    }

    // ==================== REQUEST DTOs ====================
    public class XacNhanKetQuaRequestDto
    {
        public int TranDauId { get; set; }
        public string? GhiChu { get; set; }
    }

    public class YeuCauKiemTraRequestDto
    {
        public int TranDauId { get; set; }
        public string LyDo { get; set; } = string.Empty;
    }

    public class BulkApproveRequestDto
    {
        public List<int> TranDauIds { get; set; } = new();
        public string? GhiChu { get; set; }
    }
}
