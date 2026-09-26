using Dms.Application.DTOs;

namespace Dms.Application.Interfaces
{
    public interface INguoiDieuHanhMonService
    {
        /// <summary>Lấy hồ sơ điều hành môn đang hoạt động và liên kết với các tài khoản có quyền điều hành.</summary>
        /// <param name="applicationUserIds">Danh sách ID tài khoản đã được xác nhận thuộc role SportCoordinator.</param>
        /// <returns>Các hồ sơ có thể lựa chọn để phân công trong giải.</returns>
        Task<List<SportCoordinatorUserOptionDto>> GetProfilesForAccountsAsync(IReadOnlyCollection<int> applicationUserIds);

        /// <summary>Đảm bảo tài khoản điều hành môn có hồ sơ riêng và đồng bộ họ tên, email, số điện thoại từ tài khoản.</summary>
        /// <param name="applicationUserId">ID tài khoản đăng nhập có vai trò SportCoordinator.</param>
        /// <returns>Kết quả tạo/cập nhật hồ sơ cùng thông báo.</returns>
        Task<(bool success, string message)> EnsureProfileForUserAsync(int applicationUserId);
    }
}
