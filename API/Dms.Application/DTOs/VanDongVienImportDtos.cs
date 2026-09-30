using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    /// <summary>Dữ liệu tạm của một dòng vận động viên sau khi đọc từ tệp Excel.</summary>
    public class VanDongVienImportRowDto
    {
        /// <summary>Số dòng trong tệp Excel hoặc số thứ tự tạm của dòng được thêm trên màn hình.</summary>
        public int RowNumber { get; set; }

        /// <summary>Họ và tên vận động viên.</summary>
        public string HoTen { get; set; } = string.Empty;

        /// <summary>Giới tính Nam hoặc Nữ.</summary>
        public string GioiTinh { get; set; } = string.Empty;

        /// <summary>Ngày sinh dạng dd/MM/yyyy.</summary>
        public string NgaySinh { get; set; } = string.Empty;

        /// <summary>Số CCCD hoặc định danh.</summary>
        public string SoCCCD { get; set; } = string.Empty;

        /// <summary>Số điện thoại.</summary>
        public string SoDienThoai { get; set; } = string.Empty;

        /// <summary>Địa chỉ hoặc quê quán.</summary>
        public string DiaChi { get; set; } = string.Empty;

        /// <summary>Địa chỉ email.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Trạng thái hoạt động của vận động viên.</summary>
        public bool TrangThai { get; set; } = true;

        /// <summary>Giá trị trạng thái dạng chữ để kiểm tra lại khi người dùng xác nhận lưu.</summary>
        public string TrangThaiText { get; set; } = "Sẵn sàng";

        /// <summary>URL ảnh đại diện, có thể để trống.</summary>
        public string? HinhAnh { get; set; }

        /// <summary>Dữ liệu ảnh dạng Data URL chỉ tồn tại trong danh sách tạm cho đến khi người dùng bấm lưu.</summary>
        public string? HinhAnhData { get; set; }

        /// <summary>Tên tệp gốc của ảnh được chọn trong danh sách tạm.</summary>
        public string? HinhAnhFileName { get; set; }

        /// <summary>Các lỗi đang gắn với dòng tạm để người dùng chỉnh sửa.</summary>
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>Dữ liệu xem trước danh sách vận động viên trước khi lưu.</summary>
    public class VanDongVienImportPreviewDto
    {
        /// <summary>Các dòng vận động viên được đọc từ file Excel.</summary>
        public List<VanDongVienImportRowDto> Rows { get; set; } = new();

        /// <summary>Số dòng hiện đang hợp lệ.</summary>
        public int ValidCount { get; set; }

        /// <summary>Số dòng còn lỗi cần chỉnh sửa.</summary>
        public int ErrorCount { get; set; }
    }

    /// <summary>Kết quả lưu danh sách vận động viên sau bước xem trước.</summary>
    public class VanDongVienImportResultDto
    {
        /// <summary>Số hồ sơ vận động viên đã tạo thành công.</summary>
        public int ImportedCount { get; set; }

        /// <summary>Số dòng bị từ chối khi kiểm tra trước khi lưu.</summary>
        public int SkippedCount { get; set; }

        /// <summary>Danh sách lỗi theo từng dòng cần chỉnh sửa.</summary>
        public List<VanDongVienImportErrorDto> Errors { get; set; } = new();
    }

    /// <summary>Mô tả lỗi dữ liệu của một dòng nhập vận động viên.</summary>
    public class VanDongVienImportErrorDto
    {
        /// <summary>Số dòng Excel phát sinh lỗi.</summary>
        public int RowNumber { get; set; }

        /// <summary>Họ tên đọc được từ dòng lỗi, nếu có.</summary>
        public string? HoTen { get; set; }

        /// <summary>Nội dung lỗi cần người dùng điều chỉnh.</summary>
        public string Error { get; set; } = string.Empty;
    }
}
