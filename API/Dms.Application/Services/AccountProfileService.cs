using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Dms.Application.Services
{
    public class AccountProfileService : IAccountProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Khởi tạo dịch vụ đọc hồ sơ tài khoản và các hồ sơ nghiệp vụ liên kết.</summary>
        /// <param name="unitOfWork">Kho dữ liệu tài khoản, đơn vị, trọng tài, thư ký và người điều hành môn.</param>
        public AccountProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Lấy thông tin tài khoản và toàn bộ hồ sơ nghiệp vụ đang liên kết với tài khoản đó.</summary>
        /// <param name="userId">ID tài khoản đăng nhập cần xem.</param>
        /// <param name="roles">Danh sách vai trò đã được Identity xác thực cho tài khoản.</param>
        /// <returns>Thông tin hồ sơ; trả về null nếu tài khoản không tồn tại.</returns>
        public async Task<AccountProfileDto?> GetProfileAsync(int userId, IReadOnlyCollection<string> roles)
        {
            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId);
            if (user == null) return null;

            var result = new AccountProfileDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                Roles = roles.OrderBy(role => role).ToList()
            };

            if (user.DonViId.HasValue)
            {
                var donVi = await _unitOfWork.DonVis.GetByIdAsync(user.DonViId.Value);
                if (donVi != null && donVi.IsDeleted != true)
                {
                    var khoi = donVi.KhoiId.HasValue ? await _unitOfWork.Khois.GetByIdAsync(donVi.KhoiId.Value) : null;
                    var donViCha = donVi.DonViChaId.HasValue ? await _unitOfWork.DonVis.GetByIdAsync(donVi.DonViChaId.Value) : null;
                    result.LinkedProfiles.Add(new AccountLinkedProfileDto
                    {
                        LoaiHoSo = "Đơn vị / Đoàn thể thao",
                        Ma = donVi.Ma,
                        HoTen = donVi.Ten,
                        Email = donVi.Email,
                        SoDienThoai = donVi.SoDienThoai,
                        DiaChi = donVi.DiaChi,
                        NguoiDaiDien = donVi.NguoiDaiDien,
                        NhomDonVi = khoi != null && khoi.IsDeleted != true ? khoi.Ten : null,
                        DonViCha = donViCha != null && donViCha.IsDeleted != true ? donViCha.Ten : null,
                        MoTa = donVi.MoTa,
                        ChucDanh = donVi.LoaiDonVi,
                        TrangThai = donVi.TrangThai ? "Đang hoạt động" : "Tạm ngưng"
                    });
                }
            }

            if (user.TrongTaiId.HasValue)
            {
                var referee = await _unitOfWork.TrongTais.GetByIdAsync(user.TrongTaiId.Value);
                if (referee != null && referee.IsDeleted != true)
                {
                    result.LinkedProfiles.Add(new AccountLinkedProfileDto
                    {
                        LoaiHoSo = "Trọng tài",
                        Ma = referee.Ma,
                        HoTen = referee.HoTen,
                        GioiTinh = referee.GioiTinh,
                        Email = referee.Email,
                        SoDienThoai = referee.SoDienThoai,
                        ChucDanh = referee.CapBac,
                        TrangThai = referee.TrangThai ? "Đang hoạt động" : "Tạm ngưng"
                    });
                }
            }

            if (user.ThuKyId.HasValue)
            {
                var secretary = await _unitOfWork.ThuKys.GetByIdAsync(user.ThuKyId.Value);
                if (secretary != null && secretary.IsDeleted != true)
                {
                    result.LinkedProfiles.Add(new AccountLinkedProfileDto
                    {
                        LoaiHoSo = "Thư ký",
                        Ma = secretary.Ma,
                        HoTen = secretary.HoTen,
                        GioiTinh = secretary.GioiTinh,
                        Email = secretary.Email,
                        SoDienThoai = secretary.SoDienThoai,
                        ChucDanh = secretary.ChucVu,
                        DonViCongTac = secretary.DonViCongTac,
                        TrangThai = secretary.TrangThai ? "Đang hoạt động" : "Tạm ngưng"
                    });
                }
            }

            if (user.NguoiDieuHanhMonId.HasValue)
            {
                var coordinator = await _unitOfWork.NguoiDieuHanhMons.GetByIdAsync(user.NguoiDieuHanhMonId.Value);
                if (coordinator != null && coordinator.IsDeleted != true)
                {
                    result.LinkedProfiles.Add(new AccountLinkedProfileDto
                    {
                        LoaiHoSo = "Người điều hành môn",
                        Ma = coordinator.Ma,
                        HoTen = coordinator.HoTen,
                        GioiTinh = coordinator.GioiTinh,
                        Email = coordinator.Email,
                        SoDienThoai = coordinator.SoDienThoai,
                        ChucDanh = coordinator.ChucVu,
                        DonViCongTac = coordinator.DonViCongTac,
                        TrangThai = coordinator.TrangThai ? "Đang hoạt động" : "Tạm ngưng"
                    });
                }
            }

            return result;
        }

        /// <summary>Cập nhật số điện thoại đăng nhập của tài khoản và hủy trạng thái xác nhận số cũ khi giá trị thay đổi.</summary>
        /// <param name="userId">ID tài khoản đang yêu cầu cập nhật.</param>
        /// <param name="phoneNumber">Số điện thoại mới của tài khoản.</param>
        /// <returns>Kết quả thành công/thất bại cùng thông báo phù hợp để hiển thị cho người dùng.</returns>
        public async Task<(bool success, string message)> UpdatePhoneNumberAsync(int userId, string? phoneNumber)
        {
            var normalizedPhoneNumber = string.IsNullOrWhiteSpace(phoneNumber)
                ? null
                : phoneNumber.Trim();

            if (normalizedPhoneNumber == null)
            {
                return (false, "Vui lòng nhập số điện thoại.");
            }

            if (normalizedPhoneNumber.Length > 32)
            {
                return (false, "Số điện thoại không được dài quá 32 ký tự.");
            }

            if (!new PhoneAttribute().IsValid(normalizedPhoneNumber))
            {
                return (false, "Số điện thoại không đúng định dạng.");
            }

            var user = await _unitOfWork.ApplicationUsers.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "Không tìm thấy tài khoản.");
            }

            if (string.Equals(user.PhoneNumber, normalizedPhoneNumber, StringComparison.Ordinal))
            {
                return (true, "Số điện thoại tài khoản không có thay đổi.");
            }

            user.PhoneNumber = normalizedPhoneNumber;
            user.PhoneNumberConfirmed = false;
            _unitOfWork.ApplicationUsers.Update(user);
            await _unitOfWork.CompleteAsync();

            return (true, "Cập nhật số điện thoại tài khoản thành công. Số điện thoại mới đang ở trạng thái chưa xác nhận.");
        }
    }
}
