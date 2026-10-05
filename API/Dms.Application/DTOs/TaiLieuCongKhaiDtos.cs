using System;

namespace Dms.Application.DTOs
{
    public class TaiLieuCongKhaiDto
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string LoaiTaiLieu { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string TenTepGoc { get; set; } = string.Empty;
        public string DuoiTep { get; set; } = string.Empty;
        public long KichThuocTep { get; set; }
        public bool CongKhai { get; set; }
        public DateTime? Created { get; set; }
    }

    public class TaiLieuCongKhaiTepDto
    {
        public string DuongDanTuyetDoi { get; set; } = string.Empty;
        public string TenTaiXuong { get; set; } = string.Empty;
        public string LoaiNoiDung { get; set; } = "application/octet-stream";
    }
}
