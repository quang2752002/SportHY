using Dms.Application.DTOs;

namespace Dms.Application.Interfaces
{
    public interface IAccountProfileService
    {
        /// <summary>Lấy thông tin tài khoản và toàn bộ hồ sơ nghiệp vụ đang liên kết với tài khoản đó.</summary>
        /// <param name="userId">ID tài khoản đăng nhập cần xem.</param>
        /// <param name="roles">Danh sách vai trò đã được Identity xác thực cho tài khoản.</param>
        /// <returns>Thông tin hồ sơ; trả về null nếu tài khoản không tồn tại.</returns>
        Task<AccountProfileDto?> GetProfileAsync(int userId, IReadOnlyCollection<string> roles);

        /// <summary>Cập nhật số điện thoại đăng nhập của tài khoản và hủy trạng thái xác nhận số cũ khi giá trị thay đổi.</summary>
        /// <param name="userId">ID tài khoản đang yêu cầu cập nhật.</param>
        /// <param name="phoneNumber">Số điện thoại mới của tài khoản.</param>
        /// <returns>Kết quả thành công/thất bại cùng thông báo phù hợp để hiển thị cho người dùng.</returns>
        Task<(bool success, string message)> UpdatePhoneNumberAsync(int userId, string? phoneNumber);
    }
}
