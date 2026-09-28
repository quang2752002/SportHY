using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dms.Infrastructure.Services
{
    public class VanDongVienAvatarStorageService : IVanDongVienAvatarStorageService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private readonly IWebHostEnvironment _environment;

        /// <summary>Khởi tạo dịch vụ lưu ảnh đại diện VĐV.</summary>
        /// <param name="environment">Thông tin thư mục gốc của ứng dụng web.</param>
        public VanDongVienAvatarStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>Kiểm tra phần mở rộng, dung lượng và chữ ký tệp trước khi lưu ảnh VĐV.</summary>
        /// <param name="imageStream">Luồng nội dung tệp được tải lên.</param>
        /// <param name="fileName">Tên tệp gốc dùng để xác định phần mở rộng.</param>
        /// <param name="contentLength">Kích thước tệp do máy chủ nhận được.</param>
        /// <param name="cancellationToken">Token hủy tác vụ khi yêu cầu kết thúc.</param>
        /// <returns>Kết quả lưu, URL tương đối nếu thành công và thông báo tiếng Việt.</returns>
        public async Task<(bool success, string? url, string message)> SaveAsync(
            Stream imageStream,
            string fileName,
            long contentLength,
            CancellationToken cancellationToken = default)
        {
            if (imageStream == null || !imageStream.CanRead || contentLength <= 0)
            {
                return (false, null, "Vui lòng chọn tệp ảnh đại diện.");
            }

            if (contentLength > MaxFileSizeBytes)
            {
                return (false, null, "Ảnh đại diện không được vượt quá 5 MB.");
            }

            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
            {
                return (false, null, "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
            }

            byte[] imageBytes;
            using (var buffer = new MemoryStream())
            {
                await imageStream.CopyToAsync(buffer, cancellationToken);
                if (buffer.Length == 0)
                {
                    return (false, null, "Tệp ảnh rỗng hoặc không hợp lệ.");
                }

                if (buffer.Length > MaxFileSizeBytes)
                {
                    return (false, null, "Ảnh đại diện không được vượt quá 5 MB.");
                }

                imageBytes = buffer.ToArray();
            }

            if (!HasExpectedSignature(extension, imageBytes))
            {
                return (false, null, "Nội dung tệp không đúng định dạng ảnh đã chọn.");
            }

            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;
            var targetDirectory = Path.Combine(webRoot, "vdv");
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var targetPath = Path.Combine(targetDirectory, storedFileName);

            try
            {
                Directory.CreateDirectory(targetDirectory);
                await File.WriteAllBytesAsync(targetPath, imageBytes, cancellationToken);
            }
            catch (IOException)
            {
                return (false, null, "Không thể lưu ảnh đại diện. Vui lòng thử lại.");
            }
            catch (UnauthorizedAccessException)
            {
                return (false, null, "Ứng dụng không có quyền lưu ảnh đại diện.");
            }

            return (true, $"/vdv/{storedFileName}", "Tải ảnh đại diện lên thành công.");
        }

        /// <summary>Đối chiếu chữ ký nhị phân trong nội dung tệp với phần mở rộng ảnh khai báo.</summary>
        /// <param name="extension">Phần mở rộng ảnh đã chuẩn hóa.</param>
        /// <param name="content">Nội dung ảnh cần kiểm tra.</param>
        /// <returns>True nếu nội dung có chữ ký phù hợp với định dạng.</returns>
        private static bool HasExpectedSignature(string extension, byte[] content)
        {
            if (extension is ".jpg" or ".jpeg")
            {
                return content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF;
            }

            if (extension == ".png")
            {
                return content.Length >= 8 && content[0] == 0x89 && content[1] == 0x50 && content[2] == 0x4E && content[3] == 0x47 &&
                       content[4] == 0x0D && content[5] == 0x0A && content[6] == 0x1A && content[7] == 0x0A;
            }

            return extension == ".webp" && content.Length >= 12 &&
                   Encoding.ASCII.GetString(content, 0, 4) == "RIFF" &&
                   Encoding.ASCII.GetString(content, 8, 4) == "WEBP";
        }
    }
}
