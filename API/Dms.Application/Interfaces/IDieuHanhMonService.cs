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
        Task<(bool success, string message)> UpdateMatchStatusAsync(int tranDauId, string status);
        Task<List<CoordinatorResultItemDto>> GetResultsAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments);
        Task<(bool success, string message)> ApproveResultAsync(int tranDauId);
        Task<List<CoordinatorRefereeItemDto>> GetMonRefereesAsync(int giaiDauId, int danhMucId, int? monTheThaoId, List<CoordinatorAssignmentDto> assignments);
    }
}
