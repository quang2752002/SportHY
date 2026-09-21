using Dms.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ nghiệp vụ dành riêng cho Thư Ký Giải (Tournament Secretariat)
    /// Đảm nhiệm theo dõi nội dung, tiến độ môn, kiểm tra kết quả, biên bản, báo cáo và tra cứu dữ liệu.
    /// </summary>
    public interface IThuKyGiaiService
    {
        /// <summary>
        /// Lấy dữ liệu tổng quan cho Dashboard Ban Thư ký (KPIs, cảnh báo, tiến độ các môn, kết quả mới nhất).
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu (nếu null sẽ lấy giải đấu đầu tiên hoặc đang diễn ra).</param>
        /// <returns>Đối tượng DTO chứa toàn bộ số liệu tổng quan của Thư ký giải.</returns>
        Task<ThuKyDashboardDto> GetDashboardAsync(int? giaiDauId);

        /// <summary>
        /// Lấy danh sách toàn bộ nội dung thi đấu theo giải đấu có hỗ trợ lọc và tìm kiếm.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="monTheThaoId">ID môn thể thao (tùy chọn).</param>
        /// <param name="loaiThiDau">Loại thi đấu: CaNhan, Doi, DongDoi (tùy chọn).</param>
        /// <param name="gioiTinh">Giới tính: Nam, Nu, HonHop (tùy chọn).</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên nội dung hoặc mã (tùy chọn).</param>
        /// <returns>Danh sách các nội dung thi đấu kèm thống kê số VĐV và số trận.</returns>
        Task<List<ThuKyNoiDungDto>> GetDanhSachNoiDungAsync(int? giaiDauId, int? monTheThaoId = null, string? loaiThiDau = null, string? gioiTinh = null, string? keyword = null);

        /// <summary>
        /// Lấy thông tin chi tiết của một nội dung thi đấu (danh sách đăng ký, các vòng, bảng và trận đấu).
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">ID phân môn/nội dung trong giải đấu.</param>
        /// <returns>Chi tiết nội dung thi đấu hoặc null nếu không tìm thấy.</returns>
        Task<ThuKyNoiDungChiTietDto?> GetChiTietNoiDungAsync(int giaiDauMonTheThaoId);

        /// <summary>
        /// Lấy danh sách tiến độ thi đấu theo từng môn thể thao của giải đấu.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <returns>Danh sách tiến độ các môn (% hoàn thành, số trận, số huy chương, tình trạng biên bản).</returns>
        Task<List<ThuKyTienDoMonDto>> GetTienDoCacMonAsync(int? giaiDauId);

        /// <summary>
        /// Lấy danh sách kết quả trận đấu phục vụ kiểm tra và rà soát của Thư ký giải.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="monTheThaoId">ID môn thể thao (tùy chọn).</param>
        /// <param name="trangThaiXacNhan">Trạng thái xác nhận: ChoXacNhan, DaXacNhan, CanKiemTraLai, ChuaCoKetQua (tùy chọn).</param>
        /// <param name="keyword">Từ khóa tìm kiếm tên trận hoặc tên đội (tùy chọn).</param>
        /// <returns>Danh sách kết quả trận đấu kèm thông tin tỉ số và trạng thái kiểm tra.</returns>
        Task<List<ThuKyKetQuaDto>> GetDanhSachKetQuaAsync(int? giaiDauId, int? monTheThaoId = null, string? trangThaiXacNhan = null, string? keyword = null);

        /// <summary>
        /// Thư ký giải xác nhận và phê duyệt kết quả chính thức của một trận đấu.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần phê duyệt.</param>
        /// <param name="ghiChu">Ghi chú xác nhận của Thư ký (tùy chọn).</param>
        /// <param name="nguoiXacNhan">Họ tên hoặc tài khoản Thư ký thực hiện phê duyệt.</param>
        /// <returns>Bộ đôi (thành công, thông báo phản hồi).</returns>
        Task<(bool success, string message)> XacNhanKetQuaAsync(int tranDauId, string? ghiChu, string nguoiXacNhan);

        /// <summary>
        /// Thư ký gắn cờ yêu cầu trọng tài hoặc tổ chuyên môn kiểm tra lại kết quả có dấu hiệu bất thường.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra lại.</param>
        /// <param name="lyDo">Lý do cụ thể yêu cầu rà soát.</param>
        /// <param name="nguoiYeuCau">Họ tên hoặc tài khoản Thư ký yêu cầu.</param>
        /// <returns>Bộ đôi (thành công, thông báo phản hồi).</returns>
        Task<(bool success, string message)> YeuCauKiemTraLaiAsync(int tranDauId, string lyDo, string nguoiYeuCau);

        /// <summary>
        /// Thư ký duyệt kết quả hàng loạt cho các trận đấu đã hoàn thành và hợp lệ.
        /// </summary>
        /// <param name="tranDauIds">Danh sách các ID trận đấu cần duyệt.</param>
        /// <param name="nguoiXacNhan">Họ tên hoặc tài khoản Thư ký thực hiện phê duyệt.</param>
        /// <returns>Bộ đôi (thành công, thông báo số lượng trận đã duyệt).</returns>
        Task<(bool success, string message)> XacNhanHangLoatAsync(List<int> tranDauIds, string nguoiXacNhan);

        /// <summary>
        /// Lấy danh sách kiểm tra biên bản thi đấu của các trận.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="monTheThaoId">ID môn thể thao (tùy chọn).</param>
        /// <param name="trangThaiKy">Trạng thái chữ ký: 'DuChuKy', 'ThieuChuKy', 'DaDuyetThuKy' (tùy chọn).</param>
        /// <param name="keyword">Từ khóa tìm kiếm (tùy chọn).</param>
        /// <returns>Danh sách biên bản kèm trạng thái chữ ký và thông tin trận.</returns>
        Task<List<ThuKyBienBanDto>> GetDanhSachBienBanAsync(int? giaiDauId, int? monTheThaoId = null, string? trangThaiKy = null, string? keyword = null);

        /// <summary>
        /// Lấy thông tin chi tiết của một biên bản trận đấu (danh sách VĐV, diễn biến, điểm số từng set, chữ ký 4 bên).
        /// </summary>
        /// <param name="tranDauId">ID trận đấu.</param>
        /// <returns>Chi tiết biên bản điện tử chuẩn hoặc null nếu không tìm thấy.</returns>
        Task<ThuKyBienBanDto?> GetChiTietBienBanAsync(int tranDauId);

        /// <summary>
        /// Thư ký giải ký xác nhận biên bản điện tử chính thức.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu.</param>
        /// <param name="signerName">Họ tên Thư ký ký biên bản.</param>
        /// <param name="username">Tài khoản Thư ký.</param>
        /// <returns>Bộ đôi (thành công, thông báo phản hồi).</returns>
        Task<(bool success, string message)> KyXacNhanBienBanAsync(int tranDauId, string signerName, string username);

        /// <summary>
        /// Tổng hợp báo cáo toàn diện giải đấu (tiến độ, số lượng VĐV, tổng kết huy chương, biên bản).
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <returns>Báo cáo tổng hợp số liệu toàn giải.</returns>
        Task<ThuKyBaoCaoTongHopDto> GetBaoCaoTongHopAsync(int? giaiDauId);

        /// <summary>
        /// Lấy bảng tổng sắp huy chương toàn đoàn (Huy chương Vàng, Bạc, Đồng, Tổng điểm và Xếp hạng).
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <returns>Bảng tổng sắp huy chương theo đơn vị/đoàn tham gia.</returns>
        Task<BangTongSapHuyChuongDto> GetBangTongSapHuyChuongAsync(int? giaiDauId);

        /// <summary>
        /// Tra cứu thông minh đa đối tượng trong giải đấu (Vận động viên, Đơn vị/Đoàn thể thao, Trận đấu & Biên bản).
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm (tên VĐV, CCCD, tên đoàn, mã trận...).</param>
        /// <param name="giaiDauId">ID giải đấu (tùy chọn).</param>
        /// <param name="loaiDoiTuong">Loại đối tượng: All, Vdv, DonVi, TranDau (mặc định: All).</param>
        /// <returns>Kết quả tra cứu tổng hợp đa tiêu chí.</returns>
        Task<ThuKyTraCuuResultDto> TraCuuTongHopAsync(string keyword, int? giaiDauId = null, string? loaiDoiTuong = "All");
    }
}
