using Dms.Application.Interfaces;
using Dms.Application.DTOs;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;

namespace Dms.Application.Services
{
    public class NguoiDieuHanhMonService : INguoiDieuHanhMonService
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Khởi tạo dịch vụ hồ sơ người điều hành môn với Unit of Work của ứng dụng.</summary>
        /// <param name="unitOfWork">Kho truy cập tài khoản và hồ sơ người điều hành môn.</param>
        public NguoiDieuHanhMonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Lấy hồ sơ điều hành môn đang hoạt động và liên kết với các tài khoản đã được xác nhận thuộc role SportCoordinator.</summary>
        /// <param name="applicationUserIds">Danh sách ID tài khoản đã được xác nhận thuộc role SportCoordinator.</param>
        /// <returns>Các hồ sơ có thể lựa chọn để phân công trong giải.</returns>
        public async Task<List<SportCoordinatorUserOptionDto>> GetProfilesForAccountsAsync(IReadOnlyCollection<int> applicationUserIds)
        {
            if (applicationUserIds.Count == 0) return new List<SportCoordinatorUserOptionDto>();

            var users = (await _unitOfWork.ApplicationUsers.FindAsync(user =>
                applicationUserIds.Contains(user.Id) && user.NguoiDieuHanhMonId.HasValue)).ToList();
            var profileIds = users.Select(user => user.NguoiDieuHanhMonId!.Value).Distinct().ToList();
            var profiles = (await _unitOfWork.NguoiDieuHanhMons.FindAsync(profile =>
                profileIds.Contains(profile.Id) && profile.IsDeleted != true && profile.TrangThai)).ToDictionary(profile => profile.Id);

            return users
                .Where(user => user.NguoiDieuHanhMonId.HasValue && profiles.ContainsKey(user.NguoiDieuHanhMonId.Value))
                .Select(user =>
                {
                    var profile = profiles[user.NguoiDieuHanhMonId!.Value];
                    return new SportCoordinatorUserOptionDto
                    {
                        Id = profile.Id,
                        ApplicationUserId = user.Id,
                        Ma = profile.Ma,
                        FullName = profile.HoTen,
                        UserName = user.UserName,
                        Email = profile.Email ?? user.Email
                    };
                })
                .OrderBy(profile => profile.FullName)
                .ToList();
        }

        /// <summary>Đảm bảo tài khoản điều hành môn có hồ sơ riêng và đồng bộ thông tin cá nhân từ tài khoản đăng nhập.</summary>
        /// <param name="applicationUserId">ID tài khoản đăng nhập có vai trò SportCoordinator.</param>
        /// <returns>Kết quả tạo/cập nhật hồ sơ cùng thông báo.</returns>
        public async Task<(bool success, string message)> EnsureProfileForUserAsync(int applicationUserId)
        {
            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(applicationUserId);
            if (user == null) return (false, "Không tìm thấy tài khoản người điều hành môn.");

            NguoiDieuHanhMon? profile = user.NguoiDieuHanhMonId.HasValue
                ? await _unitOfWork.NguoiDieuHanhMons.GetByIdAsync(user.NguoiDieuHanhMonId.Value)
                : null;

            if (profile == null)
            {
                profile = new NguoiDieuHanhMon
                {
                    Ma = $"NDH-{user.Id:D6}",
                    HoTen = string.IsNullOrWhiteSpace(user.FullName) ? user.UserName ?? $"Người điều hành {user.Id}" : user.FullName.Trim(),
                    Email = user.Email,
                    SoDienThoai = user.PhoneNumber,
                    TrangThai = true,
                    IsDeleted = false,
                    CreatedBy = user.UserName ?? "System"
                };
                await _unitOfWork.NguoiDieuHanhMons.AddAsync(profile);
                await _unitOfWork.CompleteAsync();

                user.NguoiDieuHanhMonId = profile.Id;
                _unitOfWork.ApplicationUsers.Update(user);
            }
            else
            {
                profile.HoTen = string.IsNullOrWhiteSpace(user.FullName) ? user.UserName ?? profile.HoTen : user.FullName.Trim();
                profile.Email = user.Email;
                profile.SoDienThoai = user.PhoneNumber;
                profile.TrangThai = true;
                profile.IsDeleted = false;
                profile.LastModifiedBy = user.UserName ?? "System";
                profile.LastModified = DateTime.UtcNow;
                _unitOfWork.NguoiDieuHanhMons.Update(profile);
            }

            await _unitOfWork.CompleteAsync();
            return (true, "Hồ sơ người điều hành môn đã được đồng bộ.");
        }
    }
}
