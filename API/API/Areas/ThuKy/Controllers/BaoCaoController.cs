using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    /// <summary>
    /// Controller tổng hợp báo cáo và xuất dữ liệu Excel cho Thư ký giải
    /// </summary>
    public class BaoCaoController : BaseThuKyController
    {
        public BaoCaoController(
            IThuKyGiaiService thuKyService,
            IGiaiDauService giaiDauService,
            UserManager<ApplicationUser> userManager)
            : base(thuKyService, giaiDauService, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId)
        {
            var selectedGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.Tournaments = await GetAllTournamentsAsync();
            return View();
        }

        /// <summary>
        /// AJAX endpoint lấy bảng tổng sắp huy chương toàn đoàn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBaoCaoHuyChuong(int? giaiDauId)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var data = await _thuKyService.GetBangTongSapHuyChuongAsync(effectiveGiaiDauId);
            return Json(new { success = true, data });
        }

        /// <summary>
        /// AJAX endpoint lấy báo cáo tiến độ và số liệu tổng hợp toàn giải
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBaoCaoTongHop(int? giaiDauId)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var data = await _thuKyService.GetBaoCaoTongHopAsync(effectiveGiaiDauId);
            return Json(new { success = true, data });
        }

        /// <summary>
        /// Tải xuống file Excel/CSV chuẩn UTF-8 chứa số liệu báo cáo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string loaiBaoCao, int? giaiDauId)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            var sb = new StringBuilder();

            if (loaiBaoCao == "HuyChuong")
            {
                var data = await _thuKyService.GetBangTongSapHuyChuongAsync(effectiveGiaiDauId);
                sb.AppendLine($"\"BẢNG TỔNG SẮP HUY CHƯƠNG - {data.TenGiaiDau}\"");
                sb.AppendLine($"\"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}\"");
                sb.AppendLine();
                sb.AppendLine("\"Xếp Hạng\",\"Mã Đơn Vị\",\"Tên Đơn Vị / Đoàn\",\"Huy Chương Vàng\",\"Huy Chương Bạc\",\"Huy Chương Đồng\",\"Tổng Huy Chương\",\"Tổng Điểm\"");

                foreach (var r in data.BangXepHang)
                {
                    sb.AppendLine($"\"{r.XepHang}\",\"{r.MaDonVi}\",\"{r.TenDonVi}\",\"{r.SoHuyChuongVang}\",\"{r.SoHuyChuongBac}\",\"{r.SoHuyChuongDong}\",\"{r.TongSoHuyChuong}\",\"{r.TongDiem}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"BangTongSapHuyChuong_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            }
            else if (loaiBaoCao == "TienDo")
            {
                var data = await _thuKyService.GetBaoCaoTongHopAsync(effectiveGiaiDauId);
                sb.AppendLine($"\"BÁO CÁO TIẾN ĐỘ CÁC MÔN THI ĐẤU - {data.TenGiaiDau}\"");
                sb.AppendLine($"\"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}\"");
                sb.AppendLine();
                sb.AppendLine("\"Mã Môn\",\"Tên Môn Thể Thao\",\"Số Nội Dung\",\"Tổng Số Trận\",\"Đã Đấu\",\"Đang Đấu\",\"Chưa Đấu\",\"Tỷ Lệ Hoàn Thành\",\"Huy Chương Đã Trao\",\"Trạng Thái\"");

                foreach (var m in data.TienDoMonList)
                {
                    sb.AppendLine($"\"{m.MaMon}\",\"{m.TenMon}\",\"{m.TongSoNoiDung}\",\"{m.TongSoTran}\",\"{m.SoTranDaHoanThanh}\",\"{m.SoTranDangDienRa}\",\"{m.SoTranChuaDau}\",\"{m.TiLeHoanThanh}%\",\"{m.SoHuyChuongDaTrao}\",\"{m.TrangThai}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"BaoCaoTienDo_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            }
            else
            {
                // Báo cáo Kết quả toàn bộ trận đấu
                var list = await _thuKyService.GetDanhSachKetQuaAsync(effectiveGiaiDauId);
                sb.AppendLine($"\"DANH SÁCH KẾT QUẢ TRẬN ĐẤU TOÀN GIẢI\"");
                sb.AppendLine($"\"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}\"");
                sb.AppendLine();
                sb.AppendLine("\"Số Trận\",\"Tên Trận\",\"Môn Thể Thao\",\"Vòng Đấu\",\"Bảng Đấu\",\"Sân Đấu\",\"Đội 1\",\"Tỉ Số 1\",\"Tỉ Số 2\",\"Đội 2\",\"Đội Thắng\",\"Trạng Thái Trận\",\"Xác Nhận Thư Ký\",\"Người Duyệt\"");

                foreach (var k in list)
                {
                    sb.AppendLine($"\"{k.SoTran}\",\"{k.TenTran}\",\"{k.TenMonTheThao}\",\"{k.TenVongDau}\",\"{k.TenBangDau}\",\"{k.TenSanDau}\",\"{k.TenDoi1}\",\"{k.TySoDoi1}\",\"{k.TySoDoi2}\",\"{k.TenDoi2}\",\"{k.DoiThang}\",\"{k.TrangThaiTranDau}\",\"{k.TrangThaiXacNhan}\",\"{k.NguoiXacNhan}\"");
                }

                var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
                return File(bytes, "text/csv; charset=utf-8", $"DanhSachKetQua_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            }
        }
    }
}
