using Dms.Application.DTOs;
using Dms.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IDieuHanhMonService
    {
        Task<TrongTai?> GetRefereeByUserIdOrNameAsync(int? trongTaiId, string? userName, string? email);
        Task<List<CoordinatorAssignmentDto>> GetAssignedDisciplinesAsync(int? refereeId, bool isAdminOrManager);
        Task<CoordinatorDashboardDto> GetDashboardAsync(int giaiDauId, int danhMucId, List<CoordinatorAssignmentDto> assignments);
        Task<List<CoordinatorEventDto>> GetEventsAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments);
        Task<List<CoordinatorScheduleMatchDto>> GetScheduleAsync(int giaiDauId, int danhMucId, int? monTheThaoId, string? status, DateTime? date, List<CoordinatorAssignmentDto> assignments);
        /// <summary>Lấy các sân còn hoạt động phù hợp với môn của giải, sau khi kiểm tra phạm vi phân công.</summary>
        /// <param name="giaiDauMonTheThaoId">ID môn thi đấu thuộc giải cần lập lịch.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <returns>Danh sách sân phù hợp; danh sách rỗng nếu môn không thuộc phạm vi được giao.</returns>
        Task<List<CoordinatorScheduleCourtDto>> GetAvailableCourtsAsync(int giaiDauMonTheThaoId, List<int> allowedGiaiDauMonTheThaoIds);
        /// <summary>Lập lịch hàng loạt cho các trận chưa có lịch thuộc một môn được phân công, giữ nguyên lịch hiện có.</summary>
        /// <param name="request">Ngày, khung giờ, thời lượng trận, thời gian nghỉ và danh sách sân sử dụng.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tên tài khoản thực hiện thao tác để lưu vết.</param>
        /// <returns>Kết quả lập lịch, số trận được xếp mới, số lịch được giữ nguyên và cảnh báo xung đột.</returns>
        Task<CoordinatorSchedulePlanResultDto> PlanScheduleAsync(CoordinatorAutoScheduleRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username);
        /// <summary>Cập nhật thời gian dự kiến hoặc sân của một trận sau khi xác nhận quyền trên môn và kiểm tra xung đột.</summary>
        /// <param name="request">Trận cần sửa cùng thời gian và sân mới.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tên tài khoản thực hiện thao tác để lưu vết.</param>
        /// <returns>Kết quả cập nhật kèm thông báo nếu trận không thuộc phạm vi hoặc bị trùng lịch.</returns>
        Task<(bool success, string message)> UpdateMatchScheduleAsync(CoordinatorUpdateScheduleRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username);
        /// <summary>Cập nhật trạng thái thi đấu cho trận thuộc môn người gọi được phân công.</summary>
        /// <param name="tranDauId">ID trận đấu.</param>
        /// <param name="status">Trạng thái hợp lệ: ChuaDau, DangDau hoặc KetThuc.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tên tài khoản thực hiện thao tác để lưu vết.</param>
        /// <returns>Kết quả cập nhật và thông báo tương ứng.</returns>
        Task<(bool success, string message)> UpdateMatchStatusAsync(int tranDauId, string status, List<int> allowedGiaiDauMonTheThaoIds, string username);
        Task<List<CoordinatorResultItemDto>> GetResultsAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments);
        /// <summary>Duyệt kết quả hoặc yêu cầu trọng tài chỉnh sửa kết quả của một trận thuộc môn được phân công.</summary>
        /// <param name="request">Trận đấu, hành động duyệt/trả sửa và nội dung ghi chú.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tên tài khoản thực hiện thao tác để lưu vết.</param>
        /// <returns>Kết quả nghiệp vụ và thông báo cho giao diện.</returns>
        Task<(bool success, string message)> ReviewMatchResultAsync(CoordinatorReviewResultRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username);
        Task<List<CoordinatorRefereeItemDto>> GetMonRefereesAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments);
        /// <summary>Lấy các sự cố trong phạm vi những môn được giao, có thể lọc theo một môn và trạng thái.</summary>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="giaiDauMonTheThaoId">ID môn muốn lọc; null để lấy toàn bộ phạm vi được giao.</param>
        /// <param name="status">Trạng thái sự cố cần lọc; null hoặc rỗng để lấy mọi trạng thái.</param>
        /// <returns>Danh sách sự cố mới nhất phù hợp với phạm vi và bộ lọc.</returns>
        Task<List<CoordinatorIssueDto>> GetIssuesAsync(List<int> allowedGiaiDauMonTheThaoIds, int? giaiDauMonTheThaoId, string? status);
        /// <summary>Tạo sự cố gắn với môn được giao và tùy chọn với một trận cụ thể.</summary>
        /// <param name="request">Tiêu đề, mô tả, mức độ và liên kết môn/trận.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tài khoản báo cáo để lưu vào CreatedBy.</param>
        /// <returns>Kết quả tạo mới cùng thông báo nghiệp vụ.</returns>
        Task<(bool success, string message)> CreateIssueAsync(CreateCoordinatorIssueRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username);
        /// <summary>Cập nhật trạng thái, người phụ trách và ghi chú xử lý của sự cố thuộc môn được giao.</summary>
        /// <param name="request">ID sự cố và thông tin xử lý mới.</param>
        /// <param name="allowedGiaiDauMonTheThaoIds">Các môn trong giải mà người gọi được quyền điều hành.</param>
        /// <param name="username">Tài khoản cập nhật để lưu LastModifiedBy.</param>
        /// <returns>Kết quả cập nhật và thông báo tương ứng.</returns>
        Task<(bool success, string message)> UpdateIssueAsync(UpdateCoordinatorIssueRequestDto request, List<int> allowedGiaiDauMonTheThaoIds, string username);
    }
}
