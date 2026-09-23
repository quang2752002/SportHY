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
        /// Lấy các giải đấu mà một thư ký đang được phân công và còn hiệu lực.
        /// </summary>
        /// <param name="thuKyId">ID hồ sơ thư ký.</param>
        /// <returns>Danh sách giải đấu được phân công cho thư ký.</returns>
        Task<List<GiaiDauDto>> GetAssignedTournamentsAsync(int thuKyId);

        /// <summary>
        /// Lấy danh sách thư ký hoạt động và trạng thái phân công của họ trong một giải đấu.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <returns>Danh sách thư ký dùng cho màn hình phân công.</returns>
        Task<List<ThuKyPhanCongDto>> GetSecretaryAssignmentsForManagerAsync(int giaiDauId);

        /// <summary>
        /// Lưu danh sách thư ký được phân công vào giải đấu, sử dụng xóa mềm cho các phân công bị bỏ.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="thuKyIds">Danh sách ID thư ký được chọn.</param>
        /// <returns>Bộ đôi cho biết kết quả và thông báo nghiệp vụ.</returns>
        Task<(bool success, string message)> SaveSecretaryAssignmentsAsync(int giaiDauId, List<int> thuKyIds);

        /// <summary>
        /// Kiểm tra một thư ký có được phân công vào giải đấu hay không.
        /// </summary>
        /// <param name="thuKyId">ID hồ sơ thư ký.</param>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <returns>True nếu phân công đang hoạt động.</returns>
        Task<bool> IsSecretaryAssignedAsync(int thuKyId, int giaiDauId);

        /// <summary>
        /// Lấy ID giải đấu của một trận đấu.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu.</param>
        /// <returns>ID giải đấu hoặc null nếu không tìm thấy.</returns>
        Task<int?> GetTournamentIdByMatchAsync(int tranDauId);

        /// <summary>
        /// Lấy ID giải đấu của một nội dung thi đấu.
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">ID môn thể thao thuộc giải.</param>
        /// <returns>ID giải đấu hoặc null nếu không tìm thấy.</returns>
        Task<int?> GetTournamentIdByContentAsync(int giaiDauMonTheThaoId);

        /// <summary>
        /// Lấy dữ liệu tổng quan cho Dashboard Ban Thư ký (KPIs, cảnh báo, tiến độ các môn, kết quả mới nhất).
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu (nếu null sẽ lấy giải đấu đầu tiên hoặc đang diễn ra).</param>
        /// <returns>Đối tượng DTO chứa toàn bộ số liệu tổng quan của Thư ký giải.</returns>
        Task<ThuKyDashboardDto> GetDashboardAsync(int? giaiDauId);

        /// <summary>
        /// Lấy danh sách các môn thể thao thuộc giải đấu, kèm danh mục môn và thống kê tiến độ, có hỗ trợ lọc và tìm kiếm.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="monTheThaoId">ID môn thể thao (tùy chọn).</param>
        /// <param name="loaiThiDau">Loại thi đấu: CaNhan, Doi, DongDoi (tùy chọn).</param>
        /// <param name="gioiTinh">Giới tính: Nam, Nu, HonHop (tùy chọn).</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo tên/mã môn hoặc tên/mã danh mục (tùy chọn).</param>
        /// <returns>Danh sách môn thể thao kèm danh mục, thống kê số đăng ký và số trận.</returns>
        Task<List<ThuKyNoiDungDto>> GetDanhSachNoiDungAsync(int? giaiDauId, int? monTheThaoId = null, string? loaiThiDau = null, string? gioiTinh = null, string? keyword = null);

        /// <summary>
        /// Lấy thông tin chi tiết của một môn thể thao trong giải đấu, gồm danh mục, danh sách đăng ký,
        /// các vòng, bảng và trận đấu.
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">ID liên kết môn thể thao với giải đấu.</param>
        /// <returns>Chi tiết môn thể thao hoặc null nếu không tìm thấy.</returns>
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
        /// Lấy bảng tổng sắp huy chương toàn đoàn (Huy chương Vàng, Bạc, Đồng, Tổng điểm và Xếp hạng) có hỗ trợ lọc theo danh mục hoặc môn thể thao cụ thể.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu (tùy chọn; null hoặc 0 để tổng hợp toàn giải).</param>
        /// <param name="danhMucMonTheThaoId">ID danh mục môn thể thao cần lọc (tùy chọn).</param>
        /// <param name="monTheThaoId">ID môn thể thao cần lọc (tùy chọn).</param>
        /// <returns>Bảng tổng sắp huy chương theo đơn vị/đoàn tham gia theo tiêu chí lọc.</returns>
        Task<BangTongSapHuyChuongDto> GetBangTongSapHuyChuongAsync(int? giaiDauId, int? danhMucMonTheThaoId = null, int? monTheThaoId = null);

        /// <summary>
        /// Lấy danh sách bảng xếp hạng huy chương phân loại theo từng Danh mục môn thể thao.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu (tùy chọn; null hoặc 0 để tính toàn bộ).</param>
        /// <returns>Danh sách các bảng xếp hạng huy chương gom nhóm theo từng Danh mục môn thể thao.</returns>
        Task<List<BangXepHangTheoDanhMucDto>> GetBangXepHangTheoDanhMucAsync(int? giaiDauId);

        /// <summary>
        /// Lấy kết quả trao huy chương riêng cho từng MonTheThao trong từng giải đấu.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu; null hoặc 0 để lấy kết quả từng môn của mọi giải, không gộp các giải với nhau.</param>
        /// <param name="danhMucMonTheThaoId">ID danh mục môn thể thao (tùy chọn để lọc môn thuộc danh mục).</param>
        /// <returns>Danh sách bảng kết quả theo từng giải và MonTheThao, giữ riêng người hoặc đội nhận từng huy chương.</returns>
        Task<List<BangXepHangTheoMonDto>> GetBangXepHangTheoMonAsync(int? giaiDauId, int? danhMucMonTheThaoId = null);

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
