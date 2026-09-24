using Dms.Application.DTOs;
using Dms.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ dành riêng cho Trưởng Ban Trọng Tài và phân công trọng tài giải đấu
    /// </summary>
    public interface ITruongBanTrongTaiService
    {
        /// <summary>
        /// Xác định thông tin trọng tài tương ứng với tài khoản đang đăng nhập
        /// </summary>
        /// <param name="trongTaiId">ID trọng tài liên kết với User</param>
        /// <param name="userName">Tên đăng nhập (dùng để tra cứu mã trọng tài)</param>
        /// <param name="email">Email tài khoản</param>
        /// <returns>Đối tượng trọng tài nếu tìm thấy, ngược lại trả về null</returns>
        Task<TrongTai?> GetRefereeByUserIdOrNameAsync(int? trongTaiId, string? userName, string? email);

        /// <summary>
        /// Lấy danh sách các giải đấu mà Trưởng ban được phân công phụ trách (hoặc tất cả nếu là Admin/Manager)
        /// </summary>
        /// <param name="refereeId">ID của Trưởng ban trọng tài</param>
        /// <param name="isAdminOrManager">Có quyền Admin hoặc Manager hay không</param>
        /// <returns>Danh sách giải đấu được quản lý</returns>
        Task<List<GiaiDau>> GetManagedTournamentsAsync(int? refereeId, bool isAdminOrManager);

        /// <summary>
        /// Lấy các giải đấu đang có ít nhất một trận được phân công cho trọng tài.
        /// </summary>
        /// <param name="refereeId">ID hồ sơ trọng tài liên kết với tài khoản.</param>
        /// <returns>Danh sách ID giải đấu có phân công trọng tài còn hiệu lực.</returns>
        Task<List<int>> GetRefereeTournamentIdsAsync(int refereeId);

        /// <summary>
        /// Kiểm tra trọng tài có được phân công điều hành một trận đấu cụ thể hay không.
        /// </summary>
        /// <param name="refereeId">ID hồ sơ trọng tài.</param>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra.</param>
        /// <returns>True nếu trọng tài có phân công còn hiệu lực trong trận đấu.</returns>
        Task<bool> IsRefereeAssignedToMatchAsync(int refereeId, int tranDauId);

        /// <summary>
        /// Kiểm tra trận đấu có thuộc giải đấu được chỉ định hay không.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra.</param>
        /// <param name="giaiDauId">ID giải đấu cần đối chiếu.</param>
        /// <returns>True nếu trận đấu thuộc giải đấu và các bản ghi liên quan chưa bị xóa mềm.</returns>
        Task<bool> IsMatchInTournamentAsync(int tranDauId, int giaiDauId);

        /// <summary>
        /// Lấy thông tin thống kê tổng quan (Dashboard) cho Trưởng ban trọng tài trong một giải đấu
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu</param>
        /// <returns>Số liệu KPI: Tổng số trọng tài, môn, trận đấu, tiến độ và cảnh báo trùng lịch</returns>
        Task<TruongBanDashboardDto> GetDashboardAsync(int giaiDauId);

        /// <summary>
        /// Lấy danh sách trọng tài trong hệ thống có lọc và thống kê số trận đã bắt tại giải đấu đang chọn
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu (để tính số trận phân công)</param>
        /// <param name="keyword">Từ khóa tìm kiếm (tên, mã, SĐT, email)</param>
        /// <param name="capBac">Lọc theo cấp bậc trọng tài</param>
        /// <param name="trangThai">Lọc theo trạng thái hoạt động</param>
        /// <returns>Danh sách trọng tài kèm bản đồ số trận đã phân công</returns>
        Task<RefereeListDto> GetRefereeListAsync(int? giaiDauId, string? keyword, string? capBac, bool? trangThai);

        /// <summary>
        /// Lấy thông tin hồ sơ và lịch sử phân công của trọng tài, có thể giới hạn lịch sử theo một giải.
        /// </summary>
        /// <param name="id">ID hồ sơ trọng tài.</param>
        /// <param name="giaiDauId">ID giải cần giới hạn lịch sử; null để lấy lịch sử của mọi giải.</param>
        /// <returns>Thông tin trọng tài và lịch sử phù hợp, hoặc null nếu không tìm thấy hồ sơ.</returns>
        Task<RefereeDetailsDto?> GetRefereeDetailsAsync(int id, int? giaiDauId = null);

        /// <summary>
        /// Thêm mới hoặc cập nhật thông tin trọng tài trong hệ thống
        /// </summary>
        /// <param name="model">Thông tin trọng tài</param>
        /// <returns>Kết quả thực hiện (thành công/thất bại và thông báo)</returns>
        Task<(bool success, string message)> CreateOrUpdateRefereeAsync(TrongTai model);

        /// <summary>
        /// Lấy danh sách trận đấu và chi tiết 5 vị trí phân công trọng tài (Chính, Phụ 1, Phụ 2, Bàn, Giám sát)
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu</param>
        /// <param name="monTheThaoId">ID môn thể thao (nếu lọc theo môn)</param>
        /// <param name="status">Trạng thái trận đấu</param>
        /// <param name="date">Ngày thi đấu</param>
        /// <param name="trongTaiId">ID trọng tài được chỉ định (nếu lọc theo trọng tài)</param>
        /// <returns>Danh sách trận đấu kèm thông tin trọng tài phân công</returns>
        Task<List<MatchAssignmentDto>> GetMatchAssignmentsAsync(int giaiDauId, int? monTheThaoId, string? status, DateTime? date, int? trongTaiId = null);

        /// <summary>
        /// Kiểm tra xung đột trùng lịch thi đấu tức thời khi phân công một trọng tài vào một trận đấu
        /// </summary>
        /// <param name="trongTaiId">ID trọng tài cần kiểm tra</param>
        /// <param name="tranDauId">ID trận đấu dự kiến phân công</param>
        /// <returns>Thông tin có bị trùng/sát giờ dưới 90 phút hay không kèm danh sách trận xung đột</returns>
        Task<ConflictResultDto> CheckConflictAsync(int trongTaiId, int tranDauId);

        /// <summary>
        /// Gán, thay đổi hoặc hủy phân công một trọng tài cho vị trí cụ thể trong một trận đấu, kiểm soát giới hạn số trận/ngày và đệm nghỉ hồi sức
        /// </summary>
        /// <param name="tranDauId">ID trận đấu</param>
        /// <param name="vaiTro">Vai trò phân công (Trọng tài chính, phụ, bàn, giám sát)</param>
        /// <param name="trongTaiId">ID trọng tài (null hoặc 0 nếu muốn hủy phân công vị trí này)</param>
        /// <param name="force">Bỏ qua cảnh báo quá tải/xung đột khi Trưởng ban chủ động xác nhận</param>
        /// <returns>Kết quả thực hiện kèm họ tên trọng tài đã gán</returns>
        Task<(bool success, string message, string? refereeName)> AssignRefereeAsync(int tranDauId, string vaiTro, int? trongTaiId, bool force = false);

        /// <summary>
        /// Lập bản nháp tự động phân công các vị trí trọng tài cho những trận đấu đã có lịch trong phạm vi yêu cầu.
        /// Ưu tiên phân công trọng tài chính cho toàn bộ trận trước các vị trí phụ. Hàm không ghi dữ liệu;
        /// chỉ trả về các vị trí được đề xuất sau khi kiểm tra trùng giờ, thời gian nghỉ tối thiểu,
        /// giới hạn số trận mỗi ngày và danh sách trọng tài được chọn.
        /// </summary>
        /// <param name="request">Phạm vi giải đấu, môn, ngày, vai trò, trọng tài được chọn và các thay đổi nháp hiện có.</param>
        /// <returns>Kết quả nháp, danh sách vị trí được đề xuất và các vị trí chưa thể gán.</returns>
        Task<AutoAssignRefereesResultDto> AutoAssignRefereesAsync(AutoAssignRefereesRequestDto request);

        /// <summary>
        /// Xác nhận và lưu các thay đổi phân công trọng tài từ bản nháp.
        /// Các phân công bị thay thế hoặc hủy đều được xóa mềm; Trưởng ban trọng tài không được phép xuất hiện trong kết quả lưu.
        /// </summary>
        /// <param name="request">Giải đấu và danh sách thay đổi nháp cần lưu.</param>
        /// <param name="savedBy">Tên tài khoản thực hiện để lưu lịch sử thay đổi.</param>
        /// <returns>Kết quả lưu gồm số bản ghi tạo mới và số bản ghi xóa mềm.</returns>
        Task<SaveRefereeAssignmentDraftResultDto> SaveRefereeAssignmentDraftAsync(SaveRefereeAssignmentDraftRequestDto request, string? savedBy = null);

        /// <summary>
        /// Lấy lịch trình làm việc và tiến độ hoàn thành nhiệm vụ theo từng trọng tài trong giải đấu
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu</param>
        /// <param name="trongTaiId">Lọc theo trọng tài cụ thể (nếu có)</param>
        /// <param name="date">Lọc theo ngày đấu</param>
        /// <param name="monTheThaoId">Lọc theo môn thi đấu</param>
        /// <returns>Danh sách nhóm theo trọng tài kèm các ca làm việc chi tiết</returns>
        Task<List<RefereeScheduleGroupDto>> GetRefereeSchedulesAsync(int giaiDauId, int? trongTaiId, DateTime? date, int? monTheThaoId);

        /// <summary>
        /// Công cụ quét tự động toàn giải đấu để phát hiện tất cả các ca trùng lịch phân công đa môn (khoảng cách &lt; 90 phút)
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu cần quét</param>
        /// <returns>Danh sách các cặp trận bị xung đột thời gian</returns>
        Task<List<ScanConflictItemDto>> ScanConflictsAsync(int giaiDauId);

        /// <summary>
        /// Lấy danh sách các danh mục môn thể thao của giải đấu phục vụ màn hình phân công người điều hành tại phân hệ Manager
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu</param>
        /// <returns>Danh sách danh mục môn kèm người điều hành hiện tại</returns>
        Task<List<CategoryAssignmentViewModelDto>> GetCategoryAssignmentsForManagerAsync(int giaiDauId);

        /// <summary>
        /// Phân công Trưởng ban trọng tài cho giải đấu (thao tác từ phân hệ Manager)
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu</param>
        /// <param name="trongTaiId">ID trọng tài được chỉ định làm Trưởng ban</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<(bool success, string message)> AssignHeadRefereeForManagerAsync(int giaiDauId, int? trongTaiId);

        /// <summary>
        /// Phân công Người điều hành môn cho một Danh mục môn thể thao trong giải đấu (thao tác từ phân hệ Manager)
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu</param>
        /// <param name="danhMucId">ID danh mục môn thể thao</param>
        /// <param name="trongTaiId">ID trọng tài được chỉ định làm Người điều hành</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<(bool success, string message)> AssignSportCoordinatorForManagerAsync(int giaiDauId, int danhMucId, int? trongTaiId);
    }
}
