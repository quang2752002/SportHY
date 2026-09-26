using Dms.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    // ==================== DIEU LE GIAI DAU DTO ====================
    public class DieuLeGiaiDauDto
    {
        public int Id { get; set; }
        public int GiaiDauId { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? TepDinhKem { get; set; }
        public int ThuTu { get; set; } = 0;
        public bool TrangThai { get; set; } = true;
    }

    public class CreateUpdateDieuLeGiaiDauDto
    {
        public int? Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? TepDinhKem { get; set; }
        public int ThuTu { get; set; } = 0;
        public bool TrangThai { get; set; } = true;
    }

    // ==================== GIAI DAU DTOs ====================
    public class GiaiDauDto
    {
        public int Id { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? HinhAnh { get; set; }
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public DateTime? HanDangKy { get; set; }
        public string? DiaDiem { get; set; }

        // Giá trị Enum
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public PhamViGiaiDau PhamVi { get; set; } = PhamViGiaiDau.TatCa;
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public TrangThaiGiaiDau TrangThai { get; set; } = TrangThaiGiaiDau.Nhap;

        // Chuỗi tiếng Việt hiển thị ra ngoài
        public string PhamViText => PhamVi.GetDescription();
        public string TrangThaiText => TrangThai.GetDescription();

        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }

        public int? TruongBanTrongTaiId { get; set; }
        public string? TenTruongBanTrongTai { get; set; }

        // Danh sách ID các khối áp dụng (nếu PhamVi == PhamViGiaiDau.TheoKhoi)
        public List<int> KhoiIds { get; set; } = new();

        // Danh sách ID môn thi đấu tổ chức
        public List<int> MonTheThaoIds { get; set; } = new();

        // Danh sách chi tiết các môn thi đấu tổ chức
        public List<GiaiDauMonTheThaoDto> MonTheThaos { get; set; } = new();

        // Danh sách điều lệ giải đấu
        public List<DieuLeGiaiDauDto> DieuLeGiaiDaus { get; set; } = new();
    }

    public class GiaiDauMonTheThaoDto
    {
        public int Id { get; set; }
        public int MonTheThaoId { get; set; }
        public int? DanhMucId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool LaMonDongDoi { get; set; }
        public string? TenDanhMuc { get; set; }
        public string? HinhThucThiDau { get; set; }
        public string? GioiTinh { get; set; }
    }

    public class CreateUpdateGiaiDauDto
    {
        public string Ma { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? HinhAnh { get; set; }
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public DateTime? HanDangKy { get; set; }
        public string? DiaDiem { get; set; }
        public PhamViGiaiDau PhamVi { get; set; } = PhamViGiaiDau.TatCa;
        public TrangThaiGiaiDau TrangThai { get; set; } = TrangThaiGiaiDau.Nhap;
        public int? TruongBanTrongTaiId { get; set; }

        // Danh sách ID các khối áp dụng (nếu chọn TheoKhoi)
        public List<int>? KhoiIds { get; set; }

        // Danh sách ID các môn thi đấu tổ chức
        public List<int>? MonTheThaoIds { get; set; }

        // Danh sách điều lệ giải đấu
        public List<CreateUpdateDieuLeGiaiDauDto>? DieuLes { get; set; }
    }
}
