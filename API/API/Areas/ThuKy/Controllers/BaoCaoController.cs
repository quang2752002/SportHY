using Dms.Application.Interfaces;
using Dms.Application.DTOs;
using Dms.Domain.Entities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (User.IsInRole(Dms.Application.Common.AppRoles.Secretary) && !effectiveGiaiDauId.HasValue)
            {
                return Json(new { success = true, data = new BangTongSapHuyChuongDto() });
            }

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
            if (User.IsInRole(Dms.Application.Common.AppRoles.Secretary) && !effectiveGiaiDauId.HasValue)
            {
                return Json(new { success = true, data = new ThuKyBaoCaoTongHopDto() });
            }

            var data = await _thuKyService.GetBaoCaoTongHopAsync(effectiveGiaiDauId);
            return Json(new { success = true, data });
        }

        /// <summary>
        /// Tạo và tải xuống file Excel .xlsx có định dạng trình bày cho báo cáo được chọn.
        /// </summary>
        /// <param name="loaiBaoCao">Loại báo cáo: HuyChuong, TienDo hoặc KetQua.</param>
        /// <param name="giaiDauId">ID giải đấu cần xuất dữ liệu, nếu không truyền sẽ lấy giải đang được chọn.</param>
        /// <returns>File Excel .xlsx hoặc thông báo NotFound nếu thư ký chưa được phân công giải.</returns>
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string loaiBaoCao, int? giaiDauId)
        {
            var effectiveGiaiDauId = await GetSelectedGiaiDauIdAsync(giaiDauId);
            if (User.IsInRole(Dms.Application.Common.AppRoles.Secretary) && !effectiveGiaiDauId.HasValue)
            {
                return NotFound("Thư ký chưa được phân công vào giải đấu nào.");
            }

            var exportedAt = DateTime.Now;
            var reportType = loaiBaoCao?.Trim();

            if (string.Equals(reportType, "HuyChuong", StringComparison.OrdinalIgnoreCase))
            {
                var data = await _thuKyService.GetBangTongSapHuyChuongAsync(effectiveGiaiDauId);
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Tổng sắp huy chương");
                var headers = new[]
                {
                    "Xếp hạng", "Mã đơn vị", "Tên đơn vị / đoàn", "Huy chương vàng",
                    "Huy chương bạc", "Huy chương đồng", "Tổng huy chương", "Tổng điểm"
                };
                var headerRow = 4;
                var firstDataRow = 5;
                var lastDataRow = firstDataRow + data.BangXepHang.Count - 1;

                ConfigureWorksheet(worksheet, $"BẢNG TỔNG SẮP HUY CHƯƠNG - {data.TenGiaiDau}",
                    $"Ngày xuất: {exportedAt:dd/MM/yyyy HH:mm} | Số đơn vị: {data.BangXepHang.Count}", headers.Length);
                WriteHeaders(worksheet, headerRow, headers);

                for (var index = 0; index < data.BangXepHang.Count; index++)
                {
                    var row = data.BangXepHang[index];
                    var excelRow = firstDataRow + index;
                    worksheet.Cell(excelRow, 1).Value = row.XepHang;
                    worksheet.Cell(excelRow, 2).Value = row.MaDonVi ?? string.Empty;
                    worksheet.Cell(excelRow, 3).Value = row.TenDonVi;
                    worksheet.Cell(excelRow, 4).Value = row.SoHuyChuongVang;
                    worksheet.Cell(excelRow, 5).Value = row.SoHuyChuongBac;
                    worksheet.Cell(excelRow, 6).Value = row.SoHuyChuongDong;
                    worksheet.Cell(excelRow, 7).Value = row.TongSoHuyChuong;
                    worksheet.Cell(excelRow, 8).Value = row.TongDiem;
                    ApplyMedalColors(worksheet, excelRow);
                }

                var totalRow = Math.Max(firstDataRow, lastDataRow + 1);
                worksheet.Range(totalRow, 1, totalRow, 3).Merge();
                worksheet.Cell(totalRow, 1).Value = "TỔNG CỘNG";
                worksheet.Cell(totalRow, 4).Value = data.TongSoHuyChuongVang;
                worksheet.Cell(totalRow, 5).Value = data.TongSoHuyChuongBac;
                worksheet.Cell(totalRow, 6).Value = data.TongSoHuyChuongDong;
                worksheet.Cell(totalRow, 7).Value = data.TongSoHuyChuongDaTrao;
                worksheet.Cell(totalRow, 8).Value = data.BangXepHang.Sum(x => x.TongDiem);
                FinalizeWorksheet(worksheet, headerRow, lastDataRow, headers.Length,
                    new[] { 11d, 16d, 32d, 18d, 18d, 18d, 18d, 14d });
                StyleTotalRow(worksheet, totalRow, headers.Length);

                return CreateExcelFile(workbook, $"BangTongSapHuyChuong_{exportedAt:yyyyMMdd_HHmm}.xlsx");
            }

            if (string.Equals(reportType, "TienDo", StringComparison.OrdinalIgnoreCase))
            {
                var data = await _thuKyService.GetBaoCaoTongHopAsync(effectiveGiaiDauId);
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Tiến độ các môn");
                var headers = new[]
                {
                    "STT", "Mã môn", "Tên môn thể thao", "Số nội dung", "Tổng số trận",
                    "Đã đấu", "Đang đấu", "Chưa đấu", "Tỷ lệ hoàn thành", "Huy chương đã trao", "Trạng thái"
                };
                var headerRow = 4;
                var firstDataRow = 5;
                var lastDataRow = firstDataRow + data.TienDoMonList.Count - 1;

                ConfigureWorksheet(worksheet, $"BÁO CÁO TIẾN ĐỘ CÁC MÔN THỂ THAO - {data.TenGiaiDau}",
                    $"Ngày xuất: {exportedAt:dd/MM/yyyy HH:mm} | Tỷ lệ hoàn thành toàn giải: {data.TiLeHoanThanh:0.0}%", headers.Length);
                WriteHeaders(worksheet, headerRow, headers);

                for (var index = 0; index < data.TienDoMonList.Count; index++)
                {
                    var row = data.TienDoMonList[index];
                    var excelRow = firstDataRow + index;
                    worksheet.Cell(excelRow, 1).Value = index + 1;
                    worksheet.Cell(excelRow, 2).Value = row.MaMon ?? string.Empty;
                    worksheet.Cell(excelRow, 3).Value = row.TenMon;
                    worksheet.Cell(excelRow, 4).Value = row.TongSoNoiDung;
                    worksheet.Cell(excelRow, 5).Value = row.TongSoTran;
                    worksheet.Cell(excelRow, 6).Value = row.SoTranDaHoanThanh;
                    worksheet.Cell(excelRow, 7).Value = row.SoTranDangDienRa;
                    worksheet.Cell(excelRow, 8).Value = row.SoTranChuaDau;
                    worksheet.Cell(excelRow, 9).Value = row.TiLeHoanThanh / 100d;
                    worksheet.Cell(excelRow, 9).Style.NumberFormat.Format = "0.0%";
                    worksheet.Cell(excelRow, 10).Value = row.SoHuyChuongDaTrao;
                    worksheet.Cell(excelRow, 11).Value = row.TrangThai;
                    StyleStatusCell(worksheet.Cell(excelRow, 11), row.TrangThai);
                }

                var totalRow = Math.Max(firstDataRow, lastDataRow + 1);
                worksheet.Range(totalRow, 1, totalRow, 3).Merge();
                worksheet.Cell(totalRow, 1).Value = "TỔNG / TOÀN GIẢI";
                worksheet.Cell(totalRow, 4).Value = data.TongSoNoiDung;
                worksheet.Cell(totalRow, 5).Value = data.TongSoTranDau;
                worksheet.Cell(totalRow, 6).Value = data.SoTranDaDau;
                worksheet.Cell(totalRow, 7).Value = data.SoTranDangDau;
                worksheet.Cell(totalRow, 8).Value = data.SoTranChuaDau;
                worksheet.Cell(totalRow, 9).Value = data.TiLeHoanThanh / 100d;
                worksheet.Cell(totalRow, 9).Style.NumberFormat.Format = "0.0%";
                worksheet.Cell(totalRow, 10).Value = data.TongHuyChuongDaTrao;
                worksheet.Cell(totalRow, 11).Value = "Tổng hợp";
                FinalizeWorksheet(worksheet, headerRow, lastDataRow, headers.Length,
                    new[] { 8d, 14d, 28d, 15d, 15d, 13d, 13d, 13d, 18d, 20d, 18d });
                StyleTotalRow(worksheet, totalRow, headers.Length);

                return CreateExcelFile(workbook, $"BaoCaoTienDo_{exportedAt:yyyyMMdd_HHmm}.xlsx");
            }

            var results = await _thuKyService.GetDanhSachKetQuaAsync(effectiveGiaiDauId);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Kết quả toàn giải");
                var headers = new[]
                {
                    "Số trận", "Tên trận", "Môn thể thao", "Vòng đấu", "Bảng đấu", "Sân đấu",
                    "Đội 1", "Tỉ số 1", "Tỉ số 2", "Đội 2", "Đội thắng", "Trạng thái trận",
                    "Xác nhận thư ký", "Người duyệt"
                };
                var headerRow = 4;
                var firstDataRow = 5;
                var lastDataRow = firstDataRow + results.Count - 1;

                ConfigureWorksheet(worksheet, "DANH SÁCH KẾT QUẢ TRẬN ĐẤU TOÀN GIẢI",
                    $"Ngày xuất: {exportedAt:dd/MM/yyyy HH:mm} | Số trận: {results.Count}", headers.Length);
                WriteHeaders(worksheet, headerRow, headers);

                for (var index = 0; index < results.Count; index++)
                {
                    var row = results[index];
                    var excelRow = firstDataRow + index;
                    worksheet.Cell(excelRow, 1).Value = row.SoTran;
                    worksheet.Cell(excelRow, 2).Value = row.TenTran ?? string.Empty;
                    worksheet.Cell(excelRow, 3).Value = row.TenMonTheThao ?? string.Empty;
                    worksheet.Cell(excelRow, 4).Value = row.TenVongDau ?? string.Empty;
                    worksheet.Cell(excelRow, 5).Value = row.TenBangDau ?? string.Empty;
                    worksheet.Cell(excelRow, 6).Value = row.TenSanDau ?? string.Empty;
                    worksheet.Cell(excelRow, 7).Value = row.TenDoi1 ?? string.Empty;
                    worksheet.Cell(excelRow, 8).Value = row.TySoDoi1;
                    worksheet.Cell(excelRow, 9).Value = row.TySoDoi2;
                    worksheet.Cell(excelRow, 10).Value = row.TenDoi2 ?? string.Empty;
                    worksheet.Cell(excelRow, 11).Value = row.DoiThang ?? string.Empty;
                    worksheet.Cell(excelRow, 12).Value = row.TrangThaiTranDau;
                    worksheet.Cell(excelRow, 13).Value = row.TrangThaiXacNhan;
                    worksheet.Cell(excelRow, 14).Value = row.NguoiXacNhan ?? string.Empty;
                    StyleStatusCell(worksheet.Cell(excelRow, 12), row.TrangThaiTranDau);
                    StyleStatusCell(worksheet.Cell(excelRow, 13), row.TrangThaiXacNhan);
                }

                var totalRow = Math.Max(firstDataRow, lastDataRow + 1);
                worksheet.Range(totalRow, 1, totalRow, 7).Merge();
                worksheet.Cell(totalRow, 1).Value = "TỔNG SỐ TRẬN";
                worksheet.Cell(totalRow, 8).Value = results.Count;
                FinalizeWorksheet(worksheet, headerRow, lastDataRow, headers.Length,
                    new[] { 11d, 28d, 24d, 18d, 16d, 18d, 25d, 11d, 11d, 25d, 25d, 18d, 20d, 22d });
                StyleTotalRow(worksheet, totalRow, headers.Length);

                return CreateExcelFile(workbook, $"DanhSachKetQua_{exportedAt:yyyyMMdd_HHmm}.xlsx");
            }
        }

        private static void ConfigureWorksheet(IXLWorksheet worksheet, string title, string subtitle, int columnCount)
        {
            worksheet.ShowGridLines = false;
            worksheet.TabColor = XLColor.FromHtml("#1F4E78");

            worksheet.Range(1, 1, 1, columnCount).Merge();
            worksheet.Cell(1, 1).Value = title;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78");
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(1).Height = 30;

            worksheet.Range(2, 1, 2, columnCount).Merge();
            worksheet.Cell(2, 1).Value = subtitle;
            worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#506176");
            worksheet.Cell(2, 1).Style.Font.Italic = true;
            worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(2).Height = 21;
        }

        private static void WriteHeaders(IXLWorksheet worksheet, int rowNumber, IReadOnlyList<string> headers)
        {
            for (var column = 0; column < headers.Count; column++)
            {
                var cell = worksheet.Cell(rowNumber, column + 1);
                cell.Value = headers[column];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2F75B5");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
            }

            worksheet.Row(rowNumber).Height = 34;
        }

        private static void FinalizeWorksheet(IXLWorksheet worksheet, int headerRow, int lastRow, int columnCount, IReadOnlyList<double> widths)
        {
            var tableRange = worksheet.Range(headerRow, 1, Math.Max(headerRow, lastRow), columnCount);
            tableRange.Style.Font.FontName = "Arial";
            tableRange.Style.Font.FontSize = 10;
            tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#D9E2F3");
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#9FBAD0");
            tableRange.SetAutoFilter();

            for (var row = headerRow + 1; row <= lastRow; row++)
            {
                if ((row - headerRow) % 2 == 0)
                {
                    worksheet.Range(row, 1, row, columnCount).Style.Fill.BackgroundColor = XLColor.FromHtml("#F6F9FC");
                }

                worksheet.Row(row).Height = 22;
            }

            for (var column = 0; column < widths.Count; column++)
            {
                worksheet.Column(column + 1).Width = widths[column];
            }

            worksheet.SheetView.FreezeRows(headerRow);
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.PageSetup.FitToPages(1, 0);
            worksheet.PageSetup.Margins.Left = 0.25;
            worksheet.PageSetup.Margins.Right = 0.25;
            worksheet.PageSetup.Margins.Top = 0.5;
            worksheet.PageSetup.Margins.Bottom = 0.5;
        }

        private static void StyleTotalRow(IXLWorksheet worksheet, int rowNumber, int columnCount)
        {
            var range = worksheet.Range(rowNumber, 1, rowNumber, columnCount);
            range.Style.Font.Bold = true;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9EAF7");
            range.Style.Border.TopBorder = XLBorderStyleValues.Medium;
            range.Style.Border.TopBorderColor = XLColor.FromHtml("#2F75B5");
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(rowNumber).Height = 24;
        }

        private static void ApplyMedalColors(IXLWorksheet worksheet, int rowNumber)
        {
            worksheet.Cell(rowNumber, 4).Style.Font.FontColor = XLColor.FromHtml("#BF9000");
            worksheet.Cell(rowNumber, 5).Style.Font.FontColor = XLColor.FromHtml("#7F7F7F");
            worksheet.Cell(rowNumber, 6).Style.Font.FontColor = XLColor.FromHtml("#C0504D");
        }

        private static void StyleStatusCell(IXLCell cell, string? status)
        {
            var normalized = (status ?? string.Empty).ToLowerInvariant();
            cell.Style.Font.Bold = true;
            if (normalized.Contains("hoanthanh") || normalized.Contains("daxacnhan") || normalized.Contains("hople") || normalized.Contains("hop le"))
            {
                cell.Style.Font.FontColor = XLColor.FromHtml("#198754");
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E9F7EF");
            }
            else if (normalized.Contains("dang") || normalized.Contains("cho") || normalized.Contains("kiemtra"))
            {
                cell.Style.Font.FontColor = XLColor.FromHtml("#B26A00");
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF4D6");
            }
            else if (normalized.Contains("can") || normalized.Contains("loi"))
            {
                cell.Style.Font.FontColor = XLColor.FromHtml("#B02A37");
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FDEBEC");
            }
        }

        private FileContentResult CreateExcelFile(XLWorkbook workbook, string fileName)
        {
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
