using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IVanDongVienAvatarStorageService
    {
        /// <summary>Kiểm tra và lưu tệp ảnh đại diện VĐV vào vùng tệp công khai của ứng dụng.</summary>
        /// <param name="imageStream">Luồng nội dung tệp được tải lên.</param>
        /// <param name="fileName">Tên tệp gốc dùng để xác định phần mở rộng.</param>
        /// <param name="contentLength">Kích thước tệp do máy chủ nhận được.</param>
        /// <param name="cancellationToken">Token hủy tác vụ khi yêu cầu kết thúc.</param>
        /// <returns>Kết quả lưu, URL tương đối nếu thành công và thông báo tiếng Việt.</returns>
        Task<(bool success, string? url, string message)> SaveAsync(
            Stream imageStream,
            string fileName,
            long contentLength,
            CancellationToken cancellationToken = default);
    }
}
