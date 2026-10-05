using Dms.Application.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface ITaiLieuCongKhaiService
    {
        /// <summary>Lấy danh sách tài liệu chưa xóa mềm để quản lý, sắp xếp theo thời gian tải lên mới nhất.</summary>
        /// <returns>Danh sách tài liệu gồm cả tài liệu đang công khai và đang ẩn.</returns>
        Task<IReadOnlyList<TaiLieuCongKhaiDto>> GetAllForManagerAsync();

        /// <summary>Lấy các tài liệu đang công khai để hiển thị trên cổng thông tin chung.</summary>
        /// <returns>Danh sách tài liệu đã bật công khai và chưa bị xóa mềm.</returns>
        Task<IReadOnlyList<TaiLieuCongKhaiDto>> GetPublicAsync();

        /// <summary>Xác thực, lưu tệp vào kho riêng và tạo thông tin tài liệu trong cơ sở dữ liệu.</summary>
        /// <param name="tieuDe">Tiêu đề hiển thị và dùng làm tên khi người dùng tải tệp xuống.</param>
        /// <param name="loaiTaiLieu">Nhóm tài liệu dùng để lọc, ví dụ biểu mẫu hoặc kế hoạch.</param>
        /// <param name="moTa">Mô tả tùy chọn cho tài liệu.</param>
        /// <param name="content">Luồng nội dung tệp được tải lên.</param>
        /// <param name="originalFileName">Tên tệp gốc do người dùng chọn.</param>
        /// <param name="fileLength">Dung lượng tệp theo byte.</param>
        /// <param name="storageDirectory">Thư mục wwwroot/TaiLieuCongKhai, được chặn truy cập tĩnh để đi qua kiểm tra quyền xem.</param>
        /// <param name="createdBy">Tài khoản quản lý thực hiện tải tài liệu lên.</param>
        /// <returns>Thông tin tài liệu vừa được lưu.</returns>
        Task<TaiLieuCongKhaiDto> UploadAsync(
            string tieuDe,
            string loaiTaiLieu,
            string? moTa,
            Stream content,
            string originalFileName,
            long fileLength,
            string storageDirectory,
            string? createdBy);

        /// <summary>Bật hoặc tắt trạng thái công khai của một tài liệu còn hoạt động.</summary>
        /// <param name="id">Mã tài liệu cần thay đổi.</param>
        /// <param name="congKhai">True để hiển thị ngoài trang chủ, false để chỉ lưu trong màn quản lý.</param>
        /// <param name="updatedBy">Tài khoản quản lý thực hiện thay đổi.</param>
        /// <returns>True nếu cập nhật được tài liệu; false nếu không tồn tại hoặc đã xóa mềm.</returns>
        Task<bool> SetPublicAsync(int id, bool congKhai, string? updatedBy);

        /// <summary>Xóa mềm thông tin một tài liệu khỏi danh sách quản lý và cổng công khai.</summary>
        /// <param name="id">Mã tài liệu cần xóa mềm.</param>
        /// <param name="deletedBy">Tài khoản quản lý thực hiện thao tác.</param>
        /// <returns>True nếu xóa mềm thành công; false nếu tài liệu không còn tồn tại.</returns>
        Task<bool> SoftDeleteAsync(int id, string? deletedBy);

        /// <summary>Lấy đường dẫn và thông tin tệp của tài liệu cho tài khoản quản lý.</summary>
        /// <param name="id">Mã tài liệu cần xem hoặc tải.</param>
        /// <param name="storageDirectory">Thư mục hiện tại lưu tài liệu dưới wwwroot nhưng bị chặn truy cập tĩnh.</param>
        /// <param name="legacyStorageDirectory">Thư mục cũ dùng để đọc các tài liệu đã lưu trước khi chuyển sang wwwroot.</param>
        /// <returns>Thông tin tệp nếu tệp tồn tại và chưa xóa mềm; nếu không thì trả về null.</returns>
        Task<TaiLieuCongKhaiTepDto?> GetFileForManagerAsync(int id, string storageDirectory, string? legacyStorageDirectory = null);

        /// <summary>Lấy tệp của tài liệu đang công khai để người dùng ngoài hệ thống xem hoặc tải.</summary>
        /// <param name="id">Mã tài liệu cần truy cập.</param>
        /// <param name="storageDirectory">Thư mục hiện tại lưu tài liệu dưới wwwroot nhưng bị chặn truy cập tĩnh.</param>
        /// <param name="legacyStorageDirectory">Thư mục cũ dùng để đọc các tài liệu đã lưu trước khi chuyển sang wwwroot.</param>
        /// <returns>Thông tin tệp nếu tài liệu đang công khai; nếu không thì trả về null.</returns>
        Task<TaiLieuCongKhaiTepDto?> GetPublicFileAsync(int id, string storageDirectory, string? legacyStorageDirectory = null);

        /// <summary>Chuyển nội dung tài liệu Word DOCX thành HTML an toàn để xem trực tiếp trong trình duyệt.</summary>
        /// <param name="id">Mã tài liệu cần xem trước.</param>
        /// <param name="storageDirectory">Thư mục hiện tại lưu tài liệu dưới wwwroot nhưng bị chặn truy cập tĩnh.</param>
        /// <param name="requirePublic">True nếu chỉ cho xem tài liệu đang công khai; false nếu dành cho màn quản lý.</param>
        /// <param name="legacyStorageDirectory">Thư mục cũ dùng để đọc các tài liệu đã lưu trước khi chuyển sang wwwroot.</param>
        /// <returns>Nội dung HTML đã mã hóa an toàn nếu là DOCX hợp lệ; nếu tệp không tồn tại hoặc không phải DOCX thì trả về null.</returns>
        Task<string?> GetDocxPreviewHtmlAsync(int id, string storageDirectory, bool requirePublic, string? legacyStorageDirectory = null);
    }
}
