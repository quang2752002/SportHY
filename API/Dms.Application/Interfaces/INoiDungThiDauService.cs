using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface INoiDungThiDauService
    {
        Task<PagedResult<NoiDungThiDauDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, int? giaiDauMonTheThaoId = null, bool? trangThai = null);
        Task<IEnumerable<NoiDungThiDauDto>> GetAllAsync(int? giaiDauMonTheThaoId = null);
        Task<NoiDungThiDauDto?> GetByIdAsync(int id);
        Task<NoiDungThiDauDto> CreateAsync(CreateUpdateNoiDungThiDauDto dto, string? createdBy = null);
        Task<NoiDungThiDauDto?> UpdateAsync(int id, CreateUpdateNoiDungThiDauDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }

    public interface IVanDongVienService
    {
        Task<PagedResult<VanDongVienDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, int? donViId = null, bool? trangThai = null);
        Task<IEnumerable<VanDongVienDto>> GetAllAsync(int? donViId = null);
        Task<VanDongVienDto?> GetByIdAsync(int id);
        Task<VanDongVienDto> CreateAsync(CreateUpdateVanDongVienDto dto, string? createdBy = null);
        Task<VanDongVienDto?> UpdateAsync(int id, CreateUpdateVanDongVienDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }

    public interface IDoiService
    {
        Task<PagedResult<DoiDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, int? donViId = null, bool? trangThai = null);
        Task<IEnumerable<DoiDto>> GetAllAsync(int? donViId = null);
        Task<DoiDto?> GetByIdAsync(int id);
        Task<DoiDto> CreateAsync(CreateUpdateDoiDto dto, string? createdBy = null);
        Task<DoiDto?> UpdateAsync(int id, CreateUpdateDoiDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }

    public interface IDangKyThiDauService
    {
        /// <summary>
        /// Lấy danh sách hồ sơ đăng ký thi đấu có phân trang theo các điều kiện lọc (từ khóa, giải đấu, môn, đơn vị, trạng thái).
        /// </summary>
        /// <param name="pageIndex">Số trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng bản ghi trên một trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo số đăng ký hoặc tên đăng ký</param>
        /// <param name="giaiDauId">Lọc theo mã định danh giải đấu</param>
        /// <param name="giaiDauMonTheThaoId">Lọc theo môn thi đấu trong giải</param>
        /// <param name="donViId">Lọc theo đơn vị / đoàn</param>
        /// <param name="trangThai">Lọc theo trạng thái hồ sơ</param>
        /// <returns>Danh sách phân trang các hồ sơ đăng ký thi đấu kèm thông tin chi tiết</returns>
        Task<PagedResult<DangKyThiDauDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            int? donViId = null,
            string? trangThai = null);

        /// <summary>
        /// Lấy tất cả danh sách hồ sơ đăng ký thi đấu theo giải đấu, môn và đơn vị.
        /// </summary>
        /// <param name="giaiDauId">Mã định danh giải đấu</param>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn thi đấu trong giải</param>
        /// <param name="donViId">Mã định danh đơn vị / đoàn</param>
        /// <returns>Tập hợp các hồ sơ đăng ký thi đấu</returns>
        Task<IEnumerable<DangKyThiDauDto>> GetAllAsync(int? giaiDauId = null, int? giaiDauMonTheThaoId = null, int? donViId = null);

        /// <summary>
        /// Lấy thông tin chi tiết một hồ sơ đăng ký thi đấu theo Id.
        /// </summary>
        /// <param name="id">Mã định danh hồ sơ đăng ký thi đấu</param>
        /// <returns>Chi tiết hồ sơ đăng ký thi đấu hoặc null nếu không tồn tại</returns>
        Task<DangKyThiDauDto?> GetByIdAsync(int id);

        /// <summary>
        /// Tạo mới hồ sơ đăng ký thi đấu, kiểm tra tính hợp lệ về thời hạn, điều kiện môn thi, giới tính, số lượng VĐV và trùng lặp VĐV.
        /// </summary>
        /// <param name="dto">Dữ liệu đăng ký thi đấu</param>
        /// <param name="createdBy">Tài khoản tạo hồ sơ</param>
        /// <param name="isPrivileged">Cờ đặc quyền bỏ qua kiểm tra thời hạn đăng ký</param>
        /// <returns>Hồ sơ đăng ký đã tạo</returns>
        Task<DangKyThiDauDto> CreateAsync(CreateUpdateDangKyThiDauDto dto, string? createdBy = null, bool isPrivileged = false);

        /// <summary>
        /// Cập nhật thông tin hồ sơ đăng ký thi đấu.
        /// </summary>
        /// <param name="id">Mã định danh hồ sơ</param>
        /// <param name="dto">Dữ liệu cần cập nhật</param>
        /// <param name="updatedBy">Tài khoản thực hiện cập nhật</param>
        /// <param name="isPrivileged">Cờ đặc quyền cho phép cập nhật khi quá hạn</param>
        /// <returns>Hồ sơ đăng ký sau cập nhật hoặc null nếu không tìm thấy</returns>
        Task<DangKyThiDauDto?> UpdateAsync(int id, CreateUpdateDangKyThiDauDto dto, string? updatedBy = null, bool isPrivileged = false);

        /// <summary>
        /// Xóa mềm hồ sơ đăng ký thi đấu.
        /// </summary>
        /// <param name="id">Mã định danh hồ sơ</param>
        /// <param name="isPrivileged">Cờ đặc quyền</param>
        /// <returns>True nếu xóa thành công, ngược lại False</returns>
        Task<bool> DeleteAsync(int id, bool isPrivileged = false);
    }

    public interface ITranDauService
    {
        /// <summary>
        /// Lấy danh sách trận đấu phân trang theo các tiêu chí tìm kiếm và bộ lọc (giải đấu, danh mục môn, môn thi đấu, vòng, bảng, sân, ngày, trạng thái).
        /// </summary>
        Task<PagedResult<TranDauDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            int? vongDauId = null,
            int? bangDauId = null,
            int? sanDauId = null,
            DateTime? ngay = null,
            string? trangThai = null,
            int? danhMucMonTheThaoId = null);

        /// <summary>
        /// Lấy tất cả danh sách trận đấu thỏa mãn các điều kiện lọc (giải đấu, danh mục môn, môn thi đấu, vòng, bảng, sân, ngày).
        /// </summary>
        Task<IEnumerable<TranDauDto>> GetAllAsync(
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            int? vongDauId = null,
            int? bangDauId = null,
            int? sanDauId = null,
            DateTime? ngay = null,
            int? danhMucMonTheThaoId = null);

        /// <summary>
        /// Lấy các trận đấu mà trọng tài được phân công, hoặc toàn bộ trận khi người gọi có quyền giám sát.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu cần lọc; null để không lọc theo giải.</param>
        /// <param name="trongTaiId">ID trọng tài; bắt buộc khi không cho phép xem toàn bộ.</param>
        /// <param name="allowAllMatches">Cho phép trả về toàn bộ trận đấu của giải.</param>
        /// <returns>Danh sách trận đấu phù hợp với phạm vi truy cập.</returns>
        Task<IEnumerable<TranDauDto>> GetAccessibleMatchesAsync(int? giaiDauId, int? trongTaiId, bool allowAllMatches);

        /// <summary>
        /// Kiểm tra người gọi có thể truy cập trận dựa trên quyền giám sát hoặc phân công trọng tài thực tế.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra.</param>
        /// <param name="trongTaiId">ID trọng tài đang đăng nhập; null nếu không gắn hồ sơ trọng tài.</param>
        /// <param name="allowAllMatches">Cho phép bỏ qua kiểm tra phân công cho quản lý hoặc quản trị viên.</param>
        /// <returns>True nếu có quyền truy cập; ngược lại false.</returns>
        Task<bool> CanAccessMatchAsync(int tranDauId, int? trongTaiId, bool allowAllMatches);

        /// <summary>
        /// Lấy kết quả đã lưu của từng thành phần trong một lượt thi thành tích để mở lại và chỉnh sửa.
        /// </summary>
        /// <param name="tranDauId">ID lượt thi cần lấy kết quả.</param>
        /// <returns>Danh sách làn, trạng thái, thành tích chính, chỉ số phụ và thứ hạng đã lưu.</returns>
        Task<List<HeatParticipantResultDto>> GetHeatResultsByMatchIdAsync(int tranDauId);

        /// <summary>
        /// Lưu tỷ số và tiến độ trận đấu mà không thay đổi danh sách đội hoặc phân công trọng tài.
        /// </summary>
        /// <param name="id">ID trận đấu cần cập nhật.</param>
        /// <param name="dto">Tỷ số, trạng thái, ghi chú và kết quả cần lưu.</param>
        /// <param name="updatedBy">Tài khoản thực hiện cập nhật.</param>
        /// <returns>True nếu trận được cập nhật; false nếu không tìm thấy trận.</returns>
        Task<bool> UpdateMatchProgressAsync(int id, UpdateMatchProgressDto dto, string? updatedBy = null);

        /// <summary>
        /// Cập nhật nội dung biên bản mà không thay đổi kết quả, thời gian, đội hoặc phân công trận đấu.
        /// </summary>
        /// <param name="id">ID trận đấu có biên bản cần cập nhật.</param>
        /// <param name="ghiChu">Nội dung biên bản đã tuần tự hóa.</param>
        /// <param name="updatedBy">Tài khoản thực hiện cập nhật.</param>
        /// <returns>True nếu biên bản được cập nhật; false nếu không tìm thấy trận.</returns>
        Task<bool> UpdateMatchReportAsync(int id, string ghiChu, string? updatedBy = null);

        Task<TranDauDto?> GetByIdAsync(int id);
        Task<TranDauDto> CreateAsync(CreateUpdateTranDauDto dto, string? createdBy = null);
        Task<TranDauDto?> UpdateAsync(int id, CreateUpdateTranDauDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
        Task<bool> ClearByGiaiDauMonTheThaoAsync(int giaiDauMonTheThaoId);
        Task<AutoScheduleResultDto> AutoScheduleAsync(AutoScheduleRequestDto request, string? createdBy = null);
        Task<ConflictCheckResultDto> CheckConflictAsync(ConflictCheckRequestDto request);
        Task<TournamentConflictReportDto> CheckAllConflictsAsync(int giaiDauId);

        /// <summary>
        /// Lấy dữ liệu khởi tạo cho giao diện xếp cặp đấu thủ công bằng kéo thả (các đội chưa xếp, cặp đấu hiện có, vòng/bảng/sân).
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">ID liên kết giải đấu và môn thể thao</param>
        /// <param name="vongDauId">Tùy chọn lọc theo vòng đấu</param>
        /// <param name="bangDauId">Tùy chọn lọc theo bảng đấu</param>
        /// <returns>Dữ liệu đầy đủ phục vụ layout xếp cặp và danh sách đội chưa xếp</returns>
        Task<ManualPairingDataDto> GetManualPairingDataAsync(int giaiDauMonTheThaoId, int? vongDauId = null, int? bangDauId = null);

        /// <summary>
        /// Lưu danh sách các cặp đấu được xếp thủ công bởi người dùng (tạo mới hoặc cập nhật TranDau và ThanhPhanTranDau).
        /// </summary>
        /// <param name="request">Dữ liệu các cặp đấu cần lưu</param>
        /// <param name="username">Tài khoản người thực hiện thao tác</param>
        /// <returns>True nếu lưu thành công</returns>
        Task<bool> SaveManualPairingAsync(SaveManualPairingRequestDto request, string? username = null);

        /// <summary>
        /// Ghi nhận kết quả trận đấu từ trọng tài: thẩm định tỷ số theo luật môn thể thao,
        /// tự động cập nhật bảng xếp hạng nếu là vòng bảng, tự động đưa đội thắng/thua vào vòng Knockout tiếp theo,
        /// hoặc tự động trao huy chương nếu là trận chung kết/tranh hạng 3.
        /// </summary>
        /// <param name="request">Dữ liệu kết quả trận đấu gửi từ trọng tài</param>
        /// <param name="username">Tài khoản người thực hiện thao tác</param>
        /// <returns>Đối tượng CompleteMatchResponseDto chứa trạng thái và thông báo kết quả cập nhật</returns>
        Task<CompleteMatchResponseDto> CompleteMatchResultAsync(CompleteMatchRequestDto request, string username);

        /// <summary>
        /// Tự động quét và chốt thứ hạng các đội từ Vòng Bảng (Top 1, Top 2, Top 3...) 
        /// và điền vào các vị trí tương ứng trong các trận đấu Vòng Loại Trực Tiếp (Knockout: Tứ kết, Bán kết, Chung kết).
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn thi đấu trong giải</param>
        /// <param name="forceAdvance">Nếu true, ép buộc chốt theo BXH hiện tại ngay cả khi các bảng chưa đấu xong 100%</param>
        /// <param name="username">Tài khoản người thực hiện thao tác</param>
        /// <returns>Đối tượng AdvanceGroupStageResultDto chứa kết quả, thông báo và danh sách bảng chưa hoàn thành</returns>
        Task<AdvanceGroupStageResultDto> AdvanceGroupStageWinnersAsync(int giaiDauMonTheThaoId, bool forceAdvance = false, string? username = null);
    }

    public interface IBangDauService
    {
        Task<IEnumerable<BangDauDto>> GetAllAsync(int? giaiDauMonTheThaoId = null);
        Task<BangDauDto?> GetByIdAsync(int id);
        Task<BangDauDto> CreateAsync(CreateUpdateBangDauDto dto, string? createdBy = null);
        Task<BangDauDto?> UpdateAsync(int id, CreateUpdateBangDauDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
        Task<bool> AssignTeamsAsync(AssignTeamsToBangDto dto, string? updatedBy = null);
        Task<IEnumerable<BangDauDto>> AutoDistributeAsync(AutoDistributeBangDto dto, string? createdBy = null);
    }

    public interface IVongDauService
    {
        Task<IEnumerable<VongDauDto>> GetAllAsync(int? giaiDauMonTheThaoId = null);
        Task<VongDauDto?> GetByIdAsync(int id);
        Task<VongDauDto> CreateAsync(CreateUpdateVongDauDto dto, string? createdBy = null);
        Task<VongDauDto?> UpdateAsync(int id, CreateUpdateVongDauDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }

    public interface IHuyChuongService
    {
        Task<IEnumerable<HuyChuongDto>> GetAllAsync(int? giaiDauId = null);
        Task<HuyChuongDto?> GetByIdAsync(int id);
        Task<HuyChuongDto> CreateAsync(CreateUpdateHuyChuongDto dto, string? createdBy = null);
        Task<HuyChuongDto?> UpdateAsync(int id, CreateUpdateHuyChuongDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
