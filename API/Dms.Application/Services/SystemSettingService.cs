using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Enums;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Triển khai dịch vụ quản lý cấu hình hệ thống (System Settings) lưu trong cơ sở dữ liệu
    /// </summary>
    public class SystemSettingService : ISystemSettingService
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Khởi tạo instance của SystemSettingService
        /// </summary>
        /// <param name="unitOfWork">Đơn vị quản lý repository và lưu trữ dữ liệu</param>
        public SystemSettingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy toàn bộ các thiết lập hệ thống dưới dạng Dictionary khóa - giá trị.
        /// </summary>
        /// <returns>Từ điển chứa tất cả các cặp Key - Value cấu hình đang hoạt động.</returns>
        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            var pagedResult = await _unitOfWork.SystemSettings.GetPagedAsync(
                1, 1000,
                predicate: s => s.IsDeleted != true
            );

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in pagedResult.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.Key))
                {
                    dict[item.Key] = item.Value ?? string.Empty;
                }
            }

            return dict;
        }

        /// <summary>
        /// Lấy giá trị chuỗi của một khóa cấu hình cụ thể theo Enum Key.
        /// </summary>
        /// <param name="key">Khóa cấu hình thuộc enum SystemSettingKey.</param>
        /// <param name="defaultValue">Giá trị mặc định trả về nếu chưa có trong cơ sở dữ liệu.</param>
        /// <returns>Giá trị của khóa cấu hình hoặc defaultValue nếu chưa tồn tại.</returns>
        public async Task<string> GetSettingValueAsync(SystemSettingKey key, string defaultValue = "")
        {
            string keyStr = key.ToString();
            var paged = await _unitOfWork.SystemSettings.GetPagedAsync(
                1, 1,
                predicate: s => s.Key == keyStr && s.IsDeleted != true
            );

            var setting = paged.Items.FirstOrDefault();
            return setting != null && !string.IsNullOrEmpty(setting.Value) ? setting.Value : defaultValue;
        }

        /// <summary>
        /// Lấy toàn bộ các thông số cấu hình hiển thị Footer bao gồm màu sắc và thông tin liên hệ.
        /// </summary>
        /// <returns>DTO FooterSettingDto đã được điền đầy đủ dữ liệu từ DB hoặc giá trị mặc định chuẩn.</returns>
        public async Task<FooterSettingDto> GetFooterSettingsAsync()
        {
            var allSettings = await GetAllSettingsAsync();

            var result = new FooterSettingDto();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_BgColor.ToString(), out var bg) && !string.IsNullOrWhiteSpace(bg))
                result.BgColor = bg.Trim();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_TextColor.ToString(), out var text) && !string.IsNullOrWhiteSpace(text))
                result.TextColor = text.Trim();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_OrganizationName.ToString(), out var org) && !string.IsNullOrWhiteSpace(org))
                result.OrganizationName = org.Trim();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_Hotline.ToString(), out var hot) && !string.IsNullOrWhiteSpace(hot))
                result.Hotline = hot.Trim();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_Email.ToString(), out var em) && !string.IsNullOrWhiteSpace(em))
                result.Email = em.Trim();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_SupportHours.ToString(), out var hours) && !string.IsNullOrWhiteSpace(hours))
                result.SupportHours = hours.Trim();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_Description.ToString(), out var desc) && !string.IsNullOrWhiteSpace(desc))
                result.Description = desc.Trim();

            if (allSettings.TryGetValue(SystemSettingKey.Footer_Copyright.ToString(), out var copy) && !string.IsNullOrWhiteSpace(copy))
                result.Copyright = copy.Trim();

            return result;
        }

        /// <summary>
        /// Lưu hoặc cập nhật các thông số cấu hình Footer vào bảng SystemSetting theo các Enum Key tương ứng.
        /// </summary>
        /// <param name="request">Dữ liệu yêu cầu cập nhật Footer từ giao diện Admin.</param>
        /// <param name="updatedBy">Tên người thực hiện cập nhật.</param>
        /// <returns>True nếu toàn bộ thông số được lưu thành công, ngược lại False.</returns>
        public async Task<bool> SaveFooterSettingsAsync(UpdateFooterSettingRequest request, string? updatedBy = null)
        {
            if (request == null) return false;

            var settingsMap = new Dictionary<SystemSettingKey, (string Value, string Description)>
            {
                { SystemSettingKey.Footer_BgColor, (request.BgColor ?? "#0f172a", "Màu nền chân trang Footer") },
                { SystemSettingKey.Footer_TextColor, (request.TextColor ?? "#94a3b8", "Màu chữ chân trang Footer") },
                { SystemSettingKey.Footer_OrganizationName, (request.OrganizationName ?? string.Empty, "Tên đơn vị / Sở quản lý tại Footer") },
                { SystemSettingKey.Footer_Hotline, (request.Hotline ?? string.Empty, "Hotline hỗ trợ tại Footer") },
                { SystemSettingKey.Footer_Email, (request.Email ?? string.Empty, "Email liên hệ tại Footer") },
                { SystemSettingKey.Footer_SupportHours, (request.SupportHours ?? string.Empty, "Khung giờ hỗ trợ kỹ thuật tại Footer") },
                { SystemSettingKey.Footer_Description, (request.Description ?? string.Empty, "Mô tả giới thiệu ngắn tại Footer") },
                { SystemSettingKey.Footer_Copyright, (request.Copyright ?? string.Empty, "Thông tin bản quyền tại Footer") }
            };

            foreach (var kvp in settingsMap)
            {
                await UpsertSettingInternalAsync(kvp.Key, kvp.Value.Value, kvp.Value.Description, updatedBy);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Cập nhật một thiết lập hệ thống đơn lẻ theo Enum Key.
        /// </summary>
        /// <param name="key">Khóa cấu hình thuộc enum SystemSettingKey.</param>
        /// <param name="value">Giá trị mới cần cập nhật.</param>
        /// <param name="description">Mô tả mục đích cấu hình (nếu có).</param>
        /// <param name="updatedBy">Tên người thực hiện cập nhật.</param>
        /// <returns>True nếu cập nhật thành công, ngược lại False.</returns>
        public async Task<bool> UpdateSettingAsync(SystemSettingKey key, string value, string? description = null, string? updatedBy = null)
        {
            await UpsertSettingInternalAsync(key, value, description, updatedBy);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Phương thức nội bộ tìm hoặc tạo mới bản ghi SystemSetting tương ứng với key
        /// </summary>
        private async Task UpsertSettingInternalAsync(SystemSettingKey key, string value, string? description, string? updatedBy)
        {
            string keyStr = key.ToString();
            var paged = await _unitOfWork.SystemSettings.GetPagedAsync(
                1, 1,
                predicate: s => s.Key == keyStr && s.IsDeleted != true
            );

            var existing = paged.Items.FirstOrDefault();
            if (existing != null)
            {
                existing.Value = value;
                if (!string.IsNullOrEmpty(description))
                {
                    existing.Description = description;
                }
                existing.LastModified = DateTime.UtcNow;
                existing.LastModifiedBy = updatedBy;
                _unitOfWork.SystemSettings.Update(existing);
            }
            else
            {
                var newSetting = new SystemSetting
                {
                    Key = keyStr,
                    Value = value,
                    Description = description,
                    Created = DateTime.UtcNow,
                    CreatedBy = updatedBy,
                    IsDeleted = false
                };
                await _unitOfWork.SystemSettings.AddAsync(newSetting);
            }
        }
    }
}
