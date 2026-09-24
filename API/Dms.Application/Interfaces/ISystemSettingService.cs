using Dms.Application.DTOs;
using Dms.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ quản lý các cấu hình hệ thống (System Settings)
    /// </summary>
    public interface ISystemSettingService
    {
        /// <summary>
        /// Lấy toàn bộ các thiết lập hệ thống dưới dạng Dictionary khóa - giá trị.
        /// </summary>
        /// <returns>Từ điển chứa tất cả các cặp Key - Value cấu hình.</returns>
        Task<Dictionary<string, string>> GetAllSettingsAsync();

        /// <summary>
        /// Lấy giá trị chuỗi của một khóa cấu hình cụ thể theo Enum Key.
        /// </summary>
        /// <param name="key">Khóa cấu hình thuộc enum SystemSettingKey.</param>
        /// <param name="defaultValue">Giá trị mặc định trả về nếu chưa có trong cơ sở dữ liệu.</param>
        /// <returns>Giá trị của khóa cấu hình.</returns>
        Task<string> GetSettingValueAsync(SystemSettingKey key, string defaultValue = "");

        /// <summary>
        /// Lấy toàn bộ các thông số cấu hình hiển thị Footer.
        /// </summary>
        /// <returns>DTO FooterSettingDto chứa màu sắc và thông tin liên hệ chân trang.</returns>
        Task<FooterSettingDto> GetFooterSettingsAsync();

        /// <summary>
        /// Lưu hoặc cập nhật các thông số cấu hình Footer vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="request">Dữ liệu yêu cầu cập nhật Footer từ giao diện Admin.</param>
        /// <param name="updatedBy">Tên người thực hiện cập nhật.</param>
        /// <returns>True nếu lưu thành công, ngược lại False.</returns>
        Task<bool> SaveFooterSettingsAsync(UpdateFooterSettingRequest request, string? updatedBy = null);

        /// <summary>
        /// Cập nhật một thiết lập hệ thống đơn lẻ theo Enum Key.
        /// </summary>
        /// <param name="key">Khóa cấu hình thuộc enum SystemSettingKey.</param>
        /// <param name="value">Giá trị mới cần cập nhật.</param>
        /// <param name="description">Mô tả mục đích cấu hình (nếu có).</param>
        /// <param name="updatedBy">Tên người thực hiện cập nhật.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại False.</returns>
        Task<bool> UpdateSettingAsync(SystemSettingKey key, string value, string? description = null, string? updatedBy = null);
    }
}
