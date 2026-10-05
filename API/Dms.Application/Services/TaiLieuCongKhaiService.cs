using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dms.Application.Services
{
    public class TaiLieuCongKhaiService : ITaiLieuCongKhaiService
    {
        private const long MaxFileSize = 30 * 1024 * 1024;
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".odt", ".ods", ".odp", ".rtf", ".txt", ".csv",
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private readonly IUnitOfWork _unitOfWork;

        /// <summary>Khởi tạo dịch vụ quản lý tài liệu công khai.</summary>
        /// <param name="unitOfWork">Đơn vị truy cập kho tài liệu trong cơ sở dữ liệu.</param>
        public TaiLieuCongKhaiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>Lấy danh sách tài liệu chưa xóa mềm để quản lý, sắp xếp theo thời gian tải lên mới nhất.</summary>
        /// <returns>Danh sách tài liệu gồm cả tài liệu đang công khai và đang ẩn.</returns>
        public async Task<IReadOnlyList<TaiLieuCongKhaiDto>> GetAllForManagerAsync()
        {
            var documents = await _unitOfWork.TaiLieuCongKhais.FindAsync(item => item.IsDeleted != true);
            return documents.OrderByDescending(item => item.Created).Select(Map).ToList();
        }

        /// <summary>Lấy các tài liệu đang công khai để hiển thị trên cổng thông tin chung.</summary>
        /// <returns>Danh sách tài liệu đã bật công khai và chưa bị xóa mềm.</returns>
        public async Task<IReadOnlyList<TaiLieuCongKhaiDto>> GetPublicAsync()
        {
            var documents = await _unitOfWork.TaiLieuCongKhais.FindAsync(item =>
                item.IsDeleted != true && item.CongKhai);
            return documents.OrderByDescending(item => item.Created).Select(Map).ToList();
        }

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
        public async Task<TaiLieuCongKhaiDto> UploadAsync(
            string tieuDe,
            string loaiTaiLieu,
            string? moTa,
            Stream content,
            string originalFileName,
            long fileLength,
            string storageDirectory,
            string? createdBy)
        {
            var title = tieuDe?.Trim() ?? string.Empty;
            var category = loaiTaiLieu?.Trim() ?? string.Empty;
            var description = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();
            var safeOriginalName = Path.GetFileName(originalFileName ?? string.Empty);
            var extension = Path.GetExtension(safeOriginalName).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(title) || title.Length > 200)
                throw new ArgumentException("Tiêu đề tài liệu là bắt buộc và không được vượt quá 200 ký tự.");
            if (string.IsNullOrWhiteSpace(category) || category.Length > 100)
                throw new ArgumentException("Vui lòng chọn loại tài liệu hợp lệ.");
            if (description?.Length > 1000)
                throw new ArgumentException("Mô tả tài liệu không được vượt quá 1.000 ký tự.");
            if (content == null || !content.CanRead || fileLength <= 0)
                throw new ArgumentException("Vui lòng chọn tệp tài liệu cần tải lên.");
            if (fileLength > MaxFileSize)
                throw new ArgumentException("Dung lượng tệp không được vượt quá 30 MB.");
            if (string.IsNullOrWhiteSpace(safeOriginalName) || safeOriginalName.Length > 255 || !AllowedExtensions.Contains(extension))
                throw new ArgumentException("Chỉ chấp nhận PDF, Word, Excel, PowerPoint, OpenDocument, văn bản, CSV hoặc ảnh JPG/PNG/WEBP.");

            Directory.CreateDirectory(storageDirectory);
            var storedName = $"{Guid.NewGuid():N}{extension}";
            var storedPath = Path.Combine(storageDirectory, storedName);
            try
            {
                await using (var output = new FileStream(storedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await content.CopyToAsync(output);
                }

                var entity = new TaiLieuCongKhai
                {
                    TieuDe = title,
                    LoaiTaiLieu = category,
                    MoTa = description,
                    TenTepGoc = safeOriginalName,
                    TenTepLuu = storedName,
                    LoaiNoiDung = GetContentType(extension),
                    KichThuocTep = fileLength,
                    CongKhai = true,
                    CreatedBy = createdBy
                };

                await _unitOfWork.TaiLieuCongKhais.AddAsync(entity);
                await _unitOfWork.CompleteAsync();
                return Map(entity);
            }
            catch
            {
                if (File.Exists(storedPath)) File.Delete(storedPath);
                throw;
            }
        }

        /// <summary>Bật hoặc tắt trạng thái công khai của một tài liệu còn hoạt động.</summary>
        /// <param name="id">Mã tài liệu cần thay đổi.</param>
        /// <param name="congKhai">True để hiển thị ngoài trang chủ, false để chỉ lưu trong màn quản lý.</param>
        /// <param name="updatedBy">Tài khoản quản lý thực hiện thay đổi.</param>
        /// <returns>True nếu cập nhật được tài liệu; false nếu không tồn tại hoặc đã xóa mềm.</returns>
        public async Task<bool> SetPublicAsync(int id, bool congKhai, string? updatedBy)
        {
            var document = (await _unitOfWork.TaiLieuCongKhais.FindAsync(item =>
                item.Id == id && item.IsDeleted != true)).FirstOrDefault();
            if (document == null) return false;

            document.CongKhai = congKhai;
            document.LastModified = DateTime.UtcNow;
            document.LastModifiedBy = updatedBy;
            _unitOfWork.TaiLieuCongKhais.Update(document);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>Xóa mềm thông tin một tài liệu khỏi danh sách quản lý và cổng công khai.</summary>
        /// <param name="id">Mã tài liệu cần xóa mềm.</param>
        /// <param name="deletedBy">Tài khoản quản lý thực hiện thao tác.</param>
        /// <returns>True nếu xóa mềm thành công; false nếu tài liệu không còn tồn tại.</returns>
        public async Task<bool> SoftDeleteAsync(int id, string? deletedBy)
        {
            var document = (await _unitOfWork.TaiLieuCongKhais.FindAsync(item =>
                item.Id == id && item.IsDeleted != true)).FirstOrDefault();
            if (document == null) return false;

            document.IsDeleted = true;
            document.LastModified = DateTime.UtcNow;
            document.LastModifiedBy = deletedBy;
            _unitOfWork.TaiLieuCongKhais.Update(document);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>Lấy đường dẫn và thông tin tệp của tài liệu cho tài khoản quản lý.</summary>
        /// <param name="id">Mã tài liệu cần xem hoặc tải.</param>
        /// <param name="storageDirectory">Thư mục hiện tại lưu tài liệu dưới wwwroot nhưng bị chặn truy cập tĩnh.</param>
        /// <param name="legacyStorageDirectory">Thư mục cũ dùng để đọc các tài liệu đã lưu trước khi chuyển sang wwwroot.</param>
        /// <returns>Thông tin tệp nếu tệp tồn tại và chưa xóa mềm; nếu không thì trả về null.</returns>
        public async Task<TaiLieuCongKhaiTepDto?> GetFileForManagerAsync(int id, string storageDirectory, string? legacyStorageDirectory = null)
        {
            return await GetFileAsync(id, storageDirectory, requirePublic: false, legacyStorageDirectory: legacyStorageDirectory);
        }

        /// <summary>Lấy tệp của tài liệu đang công khai để người dùng ngoài hệ thống xem hoặc tải.</summary>
        /// <param name="id">Mã tài liệu cần truy cập.</param>
        /// <param name="storageDirectory">Thư mục hiện tại lưu tài liệu dưới wwwroot nhưng bị chặn truy cập tĩnh.</param>
        /// <param name="legacyStorageDirectory">Thư mục cũ dùng để đọc các tài liệu đã lưu trước khi chuyển sang wwwroot.</param>
        /// <returns>Thông tin tệp nếu tài liệu đang công khai; nếu không thì trả về null.</returns>
        public async Task<TaiLieuCongKhaiTepDto?> GetPublicFileAsync(int id, string storageDirectory, string? legacyStorageDirectory = null)
        {
            return await GetFileAsync(id, storageDirectory, requirePublic: true, legacyStorageDirectory: legacyStorageDirectory);
        }

        /// <summary>Đọc nội dung DOCX và chuyển các đoạn văn, định dạng cơ bản cùng bảng thành HTML an toàn để xem trên web.</summary>
        /// <param name="id">Mã tài liệu cần xem trước.</param>
        /// <param name="storageDirectory">Thư mục hiện tại lưu tài liệu dưới wwwroot nhưng bị chặn truy cập tĩnh.</param>
        /// <param name="requirePublic">True nếu chỉ cho xem tài liệu đang công khai; false nếu dành cho màn quản lý.</param>
        /// <param name="legacyStorageDirectory">Thư mục cũ dùng để đọc các tài liệu đã lưu trước khi chuyển sang wwwroot.</param>
        /// <returns>Trang HTML xem trước nếu tài liệu là DOCX; null nếu tài liệu không tồn tại hoặc thuộc định dạng khác.</returns>
        public async Task<string?> GetDocxPreviewHtmlAsync(int id, string storageDirectory, bool requirePublic, string? legacyStorageDirectory = null)
        {
            var file = await GetFileAsync(id, storageDirectory, requirePublic, legacyStorageDirectory);
            if (file == null || !string.Equals(Path.GetExtension(file.TenTaiXuong), ".docx", StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                using var archive = ZipFile.OpenRead(file.DuongDanTuyetDoi);
                var documentEntry = archive.GetEntry("word/document.xml");
                if (documentEntry == null) return BuildDocxPreviewPage(file.TenTaiXuong, "Tệp Word không có nội dung để hiển thị.");

                using var stream = documentEntry.Open();
                var document = XDocument.Load(stream);
                XNamespace word = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                var body = document.Root?.Element(word + "body");
                if (body == null) return BuildDocxPreviewPage(file.TenTaiXuong, "Không tìm thấy nội dung trong tài liệu Word.");

                var content = new StringBuilder();
                foreach (var element in body.Elements())
                {
                    if (element.Name == word + "p")
                        content.Append(RenderDocxParagraph(element, word));
                    else if (element.Name == word + "tbl")
                        content.Append(RenderDocxTable(element, word));
                }

                return BuildDocxPreviewPage(file.TenTaiXuong,
                    content.Length == 0 ? "Tài liệu này chưa có nội dung văn bản để hiển thị." : content.ToString());
            }
            catch (Exception exception) when (exception is InvalidDataException || exception is System.Xml.XmlException || exception is IOException)
            {
                return BuildDocxPreviewPage(file.TenTaiXuong, "Không thể đọc nội dung tệp Word này. Vui lòng tải tệp xuống để mở bằng ứng dụng phù hợp.");
            }
        }

        /// <summary>Tìm metadata hợp lệ và dựng đường dẫn tệp trong thư mục lưu trữ được cấu hình.</summary>
        /// <param name="id">Mã tài liệu cần truy cập.</param>
        /// <param name="storageDirectory">Thư mục hiện tại trong wwwroot dùng để lưu tài liệu.</param>
        /// <param name="requirePublic">True nếu chỉ cho phép tài liệu đang công khai.</param>
        /// <param name="legacyStorageDirectory">Thư mục cũ cần kiểm tra nếu tệp không còn trong thư mục hiện tại.</param>
        /// <returns>Thông tin tệp khi bản ghi và tệp tồn tại; nếu không thì trả về null.</returns>
        private async Task<TaiLieuCongKhaiTepDto?> GetFileAsync(int id, string storageDirectory, bool requirePublic, string? legacyStorageDirectory = null)
        {
            var document = (await _unitOfWork.TaiLieuCongKhais.FindAsync(item =>
                item.Id == id && item.IsDeleted != true && (!requirePublic || item.CongKhai))).FirstOrDefault();
            if (document == null) return null;

            var extension = Path.GetExtension(document.TenTepLuu);
            var safeTitle = SanitizeFileName(document.TieuDe);
            var storedFileName = Path.GetFileName(document.TenTepLuu);
            var storedPath = Path.Combine(storageDirectory, storedFileName);
            if (!File.Exists(storedPath) && !string.IsNullOrWhiteSpace(legacyStorageDirectory))
                storedPath = Path.Combine(legacyStorageDirectory, storedFileName);

            var file = new TaiLieuCongKhaiTepDto
            {
                DuongDanTuyetDoi = storedPath,
                TenTaiXuong = safeTitle.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                    ? safeTitle
                    : $"{safeTitle}{extension}",
                LoaiNoiDung = document.LoaiNoiDung
            };
            return File.Exists(file.DuongDanTuyetDoi) ? file : null;
        }

        /// <summary>Chuyển entity tài liệu thành dữ liệu an toàn để hiển thị trong giao diện.</summary>
        /// <param name="document">Bản ghi tài liệu đã được lọc trạng thái xóa mềm.</param>
        /// <returns>DTO tài liệu không chứa vị trí vật lý trên máy chủ.</returns>
        private static TaiLieuCongKhaiDto Map(TaiLieuCongKhai document)
        {
            return new TaiLieuCongKhaiDto
            {
                Id = document.Id,
                TieuDe = document.TieuDe,
                LoaiTaiLieu = document.LoaiTaiLieu,
                MoTa = document.MoTa,
                TenTepGoc = document.TenTepGoc,
                DuoiTep = Path.GetExtension(document.TenTepGoc).ToUpperInvariant(),
                KichThuocTep = document.KichThuocTep,
                CongKhai = document.CongKhai,
                Created = document.Created
            };
        }

        /// <summary>Ánh xạ phần mở rộng tệp đã cho phép sang MIME type do máy chủ kiểm soát.</summary>
        /// <param name="extension">Phần mở rộng viết thường của tệp.</param>
        /// <returns>MIME type an toàn dùng khi trả tệp về trình duyệt.</returns>
        private static string GetContentType(string extension)
        {
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".odt" => "application/vnd.oasis.opendocument.text",
                ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
                ".odp" => "application/vnd.oasis.opendocument.presentation",
                ".rtf" => "application/rtf",
                ".txt" => "text/plain; charset=utf-8",
                ".csv" => "text/csv; charset=utf-8",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private static string BuildDocxPreviewPage(string fileName, string content)
        {
            var title = WebUtility.HtmlEncode(Path.GetFileNameWithoutExtension(fileName));
            return "<!doctype html><html lang=\"vi\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><title>" + title +
                "</title><style>body{margin:0;background:#eef2f7;color:#1f2937;font:16px/1.65 Arial,sans-serif}.page{max-width:900px;margin:32px auto;padding:64px 72px;background:#fff;box-shadow:0 4px 24px #0f172a18;min-height:80vh;box-sizing:border-box}.title{margin:0 0 32px;padding-bottom:16px;border-bottom:1px solid #dbe3ee;font-size:14px;color:#64748b}.content p{margin:0 0 12px;white-space:pre-wrap}.content h1,.content h2,.content h3{line-height:1.3;margin:24px 0 12px}.content table{width:100%;border-collapse:collapse;margin:16px 0}.content td{border:1px solid #94a3b8;padding:8px;vertical-align:top}.content td p{margin:0 0 6px}@media(max-width:600px){.page{margin:0;padding:28px 20px;min-height:100vh;box-shadow:none}}</style></head><body><main class=\"page\"><header class=\"title\">Xem tài liệu: <strong>" + title +
                "</strong></header><article class=\"content\">" + content + "</article></main></body></html>";
        }

        private static string RenderDocxParagraph(XElement paragraph, XNamespace word)
        {
            var styleName = (string?)paragraph.Element(word + "pPr")?.Element(word + "pStyle")?.Attribute(word + "val") ?? string.Empty;
            var headingLevel = styleName.StartsWith("Heading", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(styleName.Substring("Heading".Length), out var level) ? Math.Clamp(level, 1, 6) : 0;
            var tag = headingLevel > 0 ? $"h{headingLevel}" : "p";
            var html = new StringBuilder().Append('<').Append(tag).Append('>');

            foreach (var run in paragraph.Descendants(word + "r"))
            {
                var properties = run.Element(word + "rPr");
                var bold = properties?.Element(word + "b") != null;
                var italic = properties?.Element(word + "i") != null;
                var underline = properties?.Element(word + "u") != null;
                var text = new StringBuilder();
                foreach (var part in run.Elements())
                {
                    if (part.Name == word + "t") text.Append(WebUtility.HtmlEncode(part.Value));
                    else if (part.Name == word + "tab") text.Append("&emsp;");
                    else if (part.Name == word + "br" || part.Name == word + "cr") text.Append("<br>");
                }

                var rendered = text.ToString();
                if (bold) rendered = "<strong>" + rendered + "</strong>";
                if (italic) rendered = "<em>" + rendered + "</em>";
                if (underline) rendered = "<u>" + rendered + "</u>";
                html.Append(rendered);
            }

            return html.Append("</").Append(tag).Append('>').ToString();
        }

        private static string RenderDocxTable(XElement table, XNamespace word)
        {
            var html = new StringBuilder("<table><tbody>");
            foreach (var row in table.Elements(word + "tr"))
            {
                html.Append("<tr>");
                foreach (var cell in row.Elements(word + "tc"))
                {
                    html.Append("<td>");
                    foreach (var paragraph in cell.Elements(word + "p"))
                        html.Append(RenderDocxParagraph(paragraph, word));
                    html.Append("</td>");
                }
                html.Append("</tr>");
            }
            return html.Append("</tbody></table>").ToString();
        }

        /// <summary>Làm sạch tiêu đề để tạo tên tệp tải xuống, loại bỏ ký tự phân tách đường dẫn.</summary>
        /// <param name="value">Tiêu đề tài liệu do quản lý nhập.</param>
        /// <returns>Tên tệp hợp lệ, không chứa ký tự đường dẫn.</returns>
        private static string SanitizeFileName(string value)
        {
            var safe = string.Concat(value.Select(character =>
                Path.GetInvalidFileNameChars().Contains(character) || character is '/' or '\\' ? '-' : character));
            return string.IsNullOrWhiteSpace(safe) ? "Tai-lieu" : safe.Trim();
        }
    }
}
