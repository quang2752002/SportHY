using AutoMapper;
using ClosedXML.Excel;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Common;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class VanDongVienService : IVanDongVienService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IVanDongVienAvatarStorageService _avatarStorageService;

        public VanDongVienService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IVanDongVienAvatarStorageService avatarStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _avatarStorageService = avatarStorageService;
        }

        public async Task<PagedResult<VanDongVienDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? donViId = null,
            bool? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.VanDongViens.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: v => v.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || v.HoTen.Contains(keyword) || v.Ma.Contains(keyword) || (v.SoCCCD != null && v.SoCCCD.Contains(keyword)) || (v.SoDienThoai != null && v.SoDienThoai.Contains(keyword))) &&
                                (!donViId.HasValue || v.DonViId == donViId.Value) &&
                                (!trangThai.HasValue || v.TrangThai == trangThai.Value),
                orderBy: q => q.OrderBy(v => v.Ma),
                includes: new System.Linq.Expressions.Expression<Func<VanDongVien, object>>[]
                {
                    v => v.DonVi!
                }
            );

            var dtos = _mapper.Map<IEnumerable<VanDongVienDto>>(pagedEntities.Items);
            return new PagedResult<VanDongVienDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<VanDongVienDto>> GetAllAsync(int? donViId = null)
        {
            var items = await _unitOfWork.VanDongViens.FindAsync(
                v => v.IsDeleted != true && v.TrangThai &&
                     (!donViId.HasValue || v.DonViId == donViId.Value)
            );
            return _mapper.Map<IEnumerable<VanDongVienDto>>(items.OrderBy(v => v.HoTen));
        }

        public async Task<VanDongVienDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.VanDongViens.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            return _mapper.Map<VanDongVienDto>(entity);
        }

        public async Task<VanDongVienDto> CreateAsync(CreateUpdateVanDongVienDto dto, string? createdBy = null)
        {
            var entity = _mapper.Map<VanDongVien>(dto);
            entity.Created = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsDeleted = false;

            await _unitOfWork.VanDongViens.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VanDongVienDto>(entity);
        }

        public async Task<VanDongVienDto?> UpdateAsync(int id, CreateUpdateVanDongVienDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.VanDongViens.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return null;

            _mapper.Map(dto, entity);
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.VanDongViens.Update(entity);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<VanDongVienDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.VanDongViens.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true)
                return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.VanDongViens.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>Đọc và kiểm tra tệp Excel để tạo danh sách tạm cho màn hình xem trước, chưa ghi dữ liệu vào cơ sở dữ liệu.</summary>
        /// <param name="fileStream">Luồng dữ liệu của tệp Excel .xlsx.</param>
        /// <param name="fileName">Tên tệp Excel được người dùng tải lên.</param>
        /// <param name="donViId">Mã đơn vị sở hữu danh sách vận động viên.</param>
        /// <returns>Dữ liệu tạm gồm các dòng vận động viên và lỗi để người dùng chỉnh sửa trước khi lưu.</returns>
        public async Task<VanDongVienImportPreviewDto> PreviewFromExcelAsync(Stream fileStream, string fileName, int donViId)
        {
            if (fileStream == null || !fileStream.CanRead)
                throw new InvalidDataException("Không thể đọc tệp Excel được tải lên.");

            if (!string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Chỉ hỗ trợ tệp Excel định dạng .xlsx.");

            if (fileStream.CanSeek && fileStream.Length > 10 * 1024 * 1024)
                throw new InvalidDataException("Tệp Excel không được vượt quá 10 MB.");

            var donVi = await _unitOfWork.DonVis.GetByIdAsync(donViId);
            if (donVi == null || donVi.IsDeleted == true)
                throw new InvalidDataException("Không xác định được đơn vị quản lý vận động viên.");

            using var workbook = new XLWorkbook(fileStream);
            IXLWorksheet? worksheet = null;
            var headerRow = -1;
            var lastRow = 0;
            var lastColumn = 0;

            foreach (var candidate in workbook.Worksheets)
            {
                var lastRowUsed = candidate.LastRowUsed();
                var lastColumnUsed = candidate.LastColumnUsed();
                if (lastRowUsed == null || lastColumnUsed == null)
                    continue;

                var candidateLastRow = lastRowUsed.RowNumber();
                var candidateLastColumn = lastColumnUsed.ColumnNumber();
                var candidateHeaderRow = FindImportHeaderRow(candidate, Math.Min(candidateLastRow, 15), candidateLastColumn);
                if (candidateHeaderRow <= 0)
                    continue;

                worksheet = candidate;
                headerRow = candidateHeaderRow;
                lastRow = candidateLastRow;
                lastColumn = candidateLastColumn;
                break;
            }

            if (worksheet == null || headerRow <= 0)
                throw new InvalidDataException("Không tìm thấy dòng tiêu đề hợp lệ. Vui lòng tải và sử dụng file Excel mẫu.");

            var columns = ResolveImportColumns(worksheet, headerRow, lastColumn);
            var missingColumns = new[] { "HoTen", "GioiTinh", "NgaySinh", "SoCCCD", "SoDienThoai", "Email", "DiaChi" }
                .Where(name => !columns.ContainsKey(name))
                .ToList();
            if (missingColumns.Count > 0)
                throw new InvalidDataException($"Tệp Excel đang thiếu cột bắt buộc: {string.Join(", ", missingColumns)}.");

            const int maxImportRows = 1000;
            var dataRowCount = lastRow - headerRow;
            if (dataRowCount > maxImportRows)
                throw new InvalidDataException($"Mỗi lần chỉ được nhập tối đa {maxImportRows} vận động viên.");

            var existingCccd = await GetExistingCccdAsync(donViId);
            var preview = new VanDongVienImportPreviewDto();

            for (var rowNumber = headerRow + 1; rowNumber <= lastRow; rowNumber++)
            {
                if (IsEmptyImportRow(worksheet, rowNumber, columns))
                    continue;

                var row = ReadImportRow(worksheet, rowNumber, columns);
                row.Errors = row.Errors
                    .Concat(ValidateImportRow(row, existingCccd))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                preview.Rows.Add(row);

                if (row.Errors.Count == 0)
                {
                    preview.ValidCount++;
                    existingCccd.Add(NormalizeDigits(row.SoCCCD));
                }
                else
                {
                    preview.ErrorCount++;
                }
            }

            return preview;
        }

        /// <summary>Kiểm tra và lưu toàn bộ danh sách vận động viên đã chỉnh sửa từ màn hình xem trước.</summary>
        /// <param name="rows">Các dòng vận động viên người dùng xác nhận lưu.</param>
        /// <param name="donViId">Mã đơn vị sở hữu danh sách vận động viên.</param>
        /// <param name="createdBy">Tài khoản thực hiện thao tác lưu.</param>
        /// <returns>Kết quả lưu; nếu còn lỗi thì không lưu bất kỳ dòng nào.</returns>
        public async Task<VanDongVienImportResultDto> SaveImportedAsync(IEnumerable<VanDongVienImportRowDto> rows, int donViId, string? createdBy = null)
        {
            var donVi = await _unitOfWork.DonVis.GetByIdAsync(donViId);
            if (donVi == null || donVi.IsDeleted == true)
                throw new InvalidDataException("Không xác định được đơn vị quản lý vận động viên.");

            var importRows = rows?.ToList() ?? new List<VanDongVienImportRowDto>();
            var result = new VanDongVienImportResultDto();
            if (importRows.Count == 0)
            {
                result.Errors.Add(new VanDongVienImportErrorDto
                {
                    RowNumber = 0,
                    Error = "Danh sách tạm chưa có vận động viên nào."
                });
                result.SkippedCount = 1;
                return result;
            }

            if (importRows.Count > 1000)
                throw new InvalidDataException("Mỗi lần chỉ được lưu tối đa 1000 vận động viên.");

            var existingCccd = await GetExistingCccdAsync(donViId);
            var entitiesToCreate = new List<VanDongVien>();
            var validImportRows = new List<VanDongVienImportRowDto>();
            for (var index = 0; index < importRows.Count; index++)
            {
                var row = importRows[index];
                var rowErrors = ValidateImportRow(row, existingCccd);
                if (rowErrors.Count > 0)
                {
                    result.Errors.Add(new VanDongVienImportErrorDto
                    {
                        RowNumber = row.RowNumber > 0 ? row.RowNumber : index + 1,
                        HoTen = string.IsNullOrWhiteSpace(row.HoTen) ? null : row.HoTen,
                        Error = string.Join(" ", rowErrors)
                    });
                    continue;
                }

                existingCccd.Add(NormalizeDigits(row.SoCCCD));
                validImportRows.Add(row);
                entitiesToCreate.Add(BuildImportedEntity(row, donVi.Id, donVi.Ma, createdBy));
            }

            result.SkippedCount = result.Errors.Count;
            if (result.Errors.Count > 0)
                return result;

            for (var index = 0; index < entitiesToCreate.Count; index++)
            {
                var avatarResult = await SaveImportedAvatarAsync(validImportRows[index]);
                if (!avatarResult.success)
                {
                    result.Errors.Add(new VanDongVienImportErrorDto
                    {
                        RowNumber = validImportRows[index].RowNumber > 0 ? validImportRows[index].RowNumber : index + 1,
                        HoTen = validImportRows[index].HoTen,
                        Error = avatarResult.message
                    });
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(avatarResult.url))
                    entitiesToCreate[index].HinhAnh = avatarResult.url;
            }

            result.SkippedCount = result.Errors.Count;
            if (result.Errors.Count > 0)
                return result;

            result.ImportedCount = entitiesToCreate.Count;
            foreach (var entity in entitiesToCreate)
                await _unitOfWork.VanDongViens.AddAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        /// <summary>Tạo file Excel mẫu cho chức năng nhập danh sách vận động viên.</summary>
        /// <returns>Nội dung file Excel .xlsx đã được định dạng sẵn.</returns>
        public Task<byte[]> GenerateImportTemplateAsync()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Danh sách VĐV");
            var headers = new[]
            {
                "Họ và tên VĐV *", "Giới tính *", "Ngày sinh *", "Số CCCD / Định danh *",
                "Số điện thoại *", "Email *", "Địa chỉ / Quê quán *", "Trạng thái", "Ảnh đại diện (URL)"
            };

            worksheet.ShowGridLines = false;
            worksheet.TabColor = XLColor.FromHtml("#059669");
            worksheet.Range(1, 1, 1, headers.Length).Merge();
            worksheet.Cell(1, 1).Value = "MẪU NHẬP DANH SÁCH VẬN ĐỘNG VIÊN";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#059669");
            worksheet.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(1).Height = 30;

            worksheet.Range(2, 1, 2, headers.Length).Merge();
            worksheet.Cell(2, 1).Value = "Mã VĐV được hệ thống tự tạo. Các cột có dấu * là bắt buộc. Ảnh đại diện có thể bổ sung sau trên màn hình chỉnh sửa.";
            worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#506176");
            worksheet.Cell(2, 1).Style.Font.Italic = true;
            worksheet.Cell(2, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(2).Height = 24;

            for (var column = 0; column < headers.Length; column++)
            {
                var cell = worksheet.Cell(4, column + 1);
                cell.Value = headers[column];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#047857");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.WrapText = true;
            }

            worksheet.Row(4).Height = 36;
            var inputRange = worksheet.Range(5, 1, 104, headers.Length);
            inputRange.Style.Font.FontName = "Arial";
            inputRange.Style.Font.FontSize = 10;
            inputRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            inputRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            inputRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#D9E2F3");
            inputRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            inputRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#9FBAD0");
            worksheet.Range(5, 3, 104, 3).Style.NumberFormat.Format = "dd/mm/yyyy";
            worksheet.Range(5, 4, 104, 5).Style.NumberFormat.Format = "@";
            worksheet.Range(4, 1, 104, headers.Length).SetAutoFilter();

            var widths = new[] { 28d, 14d, 15d, 23d, 18d, 28d, 34d, 16d, 34d };
            for (var column = 0; column < widths.Length; column++)
                worksheet.Column(column + 1).Width = widths[column];

            worksheet.SheetView.FreezeRows(4);
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.PageSetup.FitToPages(1, 0);

            var instructionSheet = workbook.Worksheets.Add("Hướng dẫn");
            instructionSheet.ShowGridLines = false;
            instructionSheet.TabColor = XLColor.FromHtml("#6B7280");
            instructionSheet.Range("A1:D1").Merge();
            instructionSheet.Cell(1, 1).Value = "HƯỚNG DẪN NHẬP DANH SÁCH VẬN ĐỘNG VIÊN";
            instructionSheet.Cell(1, 1).Style.Font.Bold = true;
            instructionSheet.Cell(1, 1).Style.Font.FontSize = 15;
            instructionSheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
            instructionSheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#374151");
            instructionSheet.Cell(2, 1).Value = "1. Nhập dữ liệu tại sheet 'Danh sách VĐV', mỗi dòng là một vận động viên.";
            instructionSheet.Cell(3, 1).Value = "2. Ngày sinh nhập theo định dạng dd/MM/yyyy, ví dụ 15/07/2003.";
            instructionSheet.Cell(4, 1).Value = "3. Giới tính chỉ dùng Nam hoặc Nữ; trạng thái dùng Sẵn sàng hoặc Tạm ngừng.";
            instructionSheet.Cell(5, 1).Value = "4. CCCD và số điện thoại nên nhập dạng văn bản để giữ số 0 ở đầu.";
            instructionSheet.Cell(6, 1).Value = "5. Mã VĐV được hệ thống tự tạo. Ảnh đại diện là URL tùy chọn và có thể bổ sung sau.";
            instructionSheet.Range("A2:D6").Style.Font.FontName = "Arial";
            instructionSheet.Range("A2:D6").Style.Font.FontSize = 11;
            instructionSheet.Range("A2:D6").Style.Alignment.WrapText = true;
            instructionSheet.Column(1).Width = 100;
            instructionSheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        /// <summary>Đọc một dòng Excel thành dữ liệu tạm để hiển thị trên màn hình xem trước.</summary>
        /// <param name="worksheet">Worksheet chứa dữ liệu nhập.</param>
        /// <param name="rowNumber">Số dòng Excel cần đọc.</param>
        /// <param name="columns">Ánh xạ tên trường và vị trí cột.</param>
        /// <returns>Dữ liệu vận động viên tạm của dòng được đọc.</returns>
        private static VanDongVienImportRowDto ReadImportRow(IXLWorksheet worksheet, int rowNumber, Dictionary<string, int> columns)
        {
            var genderRaw = GetCellText(worksheet, rowNumber, columns["GioiTinh"]);
            var birthCell = worksheet.Cell(rowNumber, columns["NgaySinh"]);
            var birthDate = TryParseExcelDate(birthCell);
            var status = true;
            var statusRaw = columns.TryGetValue("TrangThai", out var statusValueColumn)
                ? GetCellText(worksheet, rowNumber, statusValueColumn)
                : "Sẵn sàng";
            var statusErrors = new List<string>();

            if (!TryParseStatus(statusRaw, out status))
            {
                statusErrors.Add("Trạng thái chỉ được là Sẵn sàng hoặc Tạm ngừng.");
            }

            var row = new VanDongVienImportRowDto
            {
                RowNumber = rowNumber,
                HoTen = GetCellText(worksheet, rowNumber, columns["HoTen"]),
                GioiTinh = TryNormalizeGender(genderRaw, out var normalizedGender) ? normalizedGender : genderRaw,
                NgaySinh = birthDate?.ToString("dd/MM/yyyy") ?? GetCellText(worksheet, rowNumber, columns["NgaySinh"]),
                SoCCCD = NormalizeDigits(GetCellText(worksheet, rowNumber, columns["SoCCCD"])),
                SoDienThoai = NormalizePhone(GetCellText(worksheet, rowNumber, columns["SoDienThoai"])),
                Email = GetCellText(worksheet, rowNumber, columns["Email"]),
                DiaChi = GetCellText(worksheet, rowNumber, columns["DiaChi"]),
                TrangThai = status,
                TrangThaiText = string.IsNullOrWhiteSpace(statusRaw) ? "Sẵn sàng" : statusRaw,
                HinhAnh = columns.TryGetValue("HinhAnh", out var imageColumn)
                    ? GetCellText(worksheet, rowNumber, imageColumn)
                    : null,
                Errors = statusErrors
            };

            return row;
        }

        /// <summary>Kiểm tra và chuẩn hóa dữ liệu của một dòng tạm trước khi hiển thị hoặc lưu.</summary>
        /// <param name="row">Dòng vận động viên cần kiểm tra.</param>
        /// <param name="existingCccd">Tập CCCD đã tồn tại hoặc đã được chấp nhận trước đó.</param>
        /// <returns>Danh sách lỗi của dòng; danh sách rỗng nghĩa là dòng hợp lệ.</returns>
        private static List<string> ValidateImportRow(VanDongVienImportRowDto row, ISet<string> existingCccd)
        {
            row.HoTen = (row.HoTen ?? string.Empty).Trim();
            row.Email = (row.Email ?? string.Empty).Trim();
            row.DiaChi = (row.DiaChi ?? string.Empty).Trim();
            row.HinhAnh = string.IsNullOrWhiteSpace(row.HinhAnh) ? null : row.HinhAnh.Trim();
            row.SoCCCD = NormalizeDigits(row.SoCCCD);
            row.SoDienThoai = NormalizePhone(row.SoDienThoai);

            var errors = new List<string>();
            if (row.HoTen.Length < 2 || row.HoTen.Length > 200)
                errors.Add("Họ và tên phải có từ 2 đến 200 ký tự.");

            if (!TryNormalizeGender(row.GioiTinh, out var normalizedGender))
                errors.Add("Giới tính chỉ được là Nam hoặc Nữ.");
            else
                row.GioiTinh = normalizedGender;

            if (!TryParseImportDate(row.NgaySinh, out var birthDate))
                errors.Add("Ngày sinh bắt buộc và phải đúng định dạng dd/MM/yyyy, không ở tương lai.");
            else
                row.NgaySinh = birthDate!.Value.ToString("dd/MM/yyyy");

            if (row.SoCCCD.Length != 12 || !row.SoCCCD.All(char.IsDigit))
                errors.Add("CCCD/định danh phải gồm đúng 12 chữ số.");
            else if (existingCccd.Contains(row.SoCCCD))
                errors.Add("CCCD/định danh đã tồn tại trong đơn vị hoặc bị trùng trong danh sách.");

            if (!IsValidPhone(row.SoDienThoai))
                errors.Add("Số điện thoại phải có dạng 10 số bắt đầu bằng 0 hoặc +84 và 9 chữ số.");

            if (row.Email.Length > 200 || !new EmailAddressAttribute().IsValid(row.Email))
                errors.Add("Email không đúng định dạng hoặc vượt quá 200 ký tự.");

            if (string.IsNullOrWhiteSpace(row.DiaChi) || row.DiaChi.Length > 500)
                errors.Add("Địa chỉ/quê quán là bắt buộc và không vượt quá 500 ký tự.");

            if ((row.HinhAnh?.Length ?? 0) > 1000)
                errors.Add("Đường dẫn ảnh không được vượt quá 1000 ký tự.");

            if (!TryParseStatus(row.TrangThaiText, out var normalizedStatus))
                errors.Add("Trạng thái chỉ được là Sẵn sàng hoặc Tạm ngừng.");
            else
            {
                row.TrangThai = normalizedStatus;
                row.TrangThaiText = normalizedStatus ? "Sẵn sàng" : "Tạm ngừng";
            }

            return errors;
        }

        /// <summary>Lấy tập CCCD chưa bị xóa mềm của các vận động viên thuộc một đơn vị.</summary>
        /// <param name="donViId">Mã đơn vị cần kiểm tra trùng CCCD.</param>
        /// <returns>Tập CCCD đã chuẩn hóa.</returns>
        private async Task<HashSet<string>> GetExistingCccdAsync(int donViId)
        {
            var existingCccd = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existingEntities = await _unitOfWork.VanDongViens.FindAsync(v =>
                v.IsDeleted != true &&
                v.DonViId == donViId &&
                v.SoCCCD != null);

            foreach (var existing in existingEntities)
            {
                var normalizedCccd = NormalizeDigits(existing.SoCCCD);
                if (!string.IsNullOrWhiteSpace(normalizedCccd))
                    existingCccd.Add(normalizedCccd);
            }

            return existingCccd;
        }

        /// <summary>Tạo entity vận động viên từ một dòng đã được kiểm tra hợp lệ.</summary>
        /// <param name="row">Dòng dữ liệu tạm đã xác nhận.</param>
        /// <param name="donViId">Mã đơn vị sở hữu hồ sơ.</param>
        /// <param name="unitCode">Mã đơn vị dùng để tạo mã VĐV.</param>
        /// <param name="createdBy">Tài khoản tạo hồ sơ.</param>
        /// <returns>Entity vận động viên sẵn sàng thêm vào cơ sở dữ liệu.</returns>
        private static VanDongVien BuildImportedEntity(VanDongVienImportRowDto row, int donViId, string? unitCode, string? createdBy)
        {
            TryParseImportDate(row.NgaySinh, out var birthDate);
            return new VanDongVien
            {
                Ma = GenerateAthleteCode(unitCode),
                HoTen = row.HoTen,
                DonViId = donViId,
                NgaySinh = birthDate,
                GioiTinh = row.GioiTinh,
                SoDienThoai = row.SoDienThoai,
                Email = row.Email,
                SoCCCD = row.SoCCCD,
                DiaChi = row.DiaChi,
                HinhAnh = row.HinhAnh,
                TrangThai = row.TrangThai,
                Created = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsDeleted = false
            };
        }

        /// <summary>Giải mã và lưu ảnh được chọn trong danh sách tạm, chỉ thực hiện sau khi các trường dữ liệu đã hợp lệ.</summary>
        /// <param name="row">Dòng tạm chứa Data URL và tên tệp ảnh.</param>
        /// <returns>Kết quả lưu ảnh gồm URL tương đối hoặc thông báo lỗi.</returns>
        private async Task<(bool success, string? url, string message)> SaveImportedAvatarAsync(VanDongVienImportRowDto row)
        {
            if (string.IsNullOrWhiteSpace(row.HinhAnhData))
                return (true, null, string.Empty);

            var dataUrl = row.HinhAnhData.Trim();
            var separatorIndex = dataUrl.IndexOf(',');
            var dataHeader = separatorIndex > 0 ? dataUrl[..separatorIndex] : string.Empty;
            if (separatorIndex <= 0 || separatorIndex == dataUrl.Length - 1 ||
                !(dataHeader.Equals("data:image/jpeg;base64", StringComparison.OrdinalIgnoreCase) ||
                  dataHeader.Equals("data:image/png;base64", StringComparison.OrdinalIgnoreCase) ||
                  dataHeader.Equals("data:image/webp;base64", StringComparison.OrdinalIgnoreCase)))
            {
                return (false, null, "Ảnh đại diện tạm không đúng định dạng dữ liệu.");
            }

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(dataUrl[(separatorIndex + 1)..]);
            }
            catch (FormatException)
            {
                return (false, null, "Ảnh đại diện tạm không thể đọc được.");
            }

            var fileName = string.IsNullOrWhiteSpace(row.HinhAnhFileName)
                ? "avatar.png"
                : Path.GetFileName(row.HinhAnhFileName);
            await using var imageStream = new MemoryStream(imageBytes, writable: false);
            return await _avatarStorageService.SaveAsync(imageStream, fileName, imageBytes.Length, CancellationToken.None);
        }

        /// <summary>Tìm dòng tiêu đề của bảng nhập trong vài dòng đầu của worksheet.</summary>
        /// <param name="worksheet">Worksheet chứa dữ liệu nhập.</param>
        /// <param name="maxHeaderRow">Số dòng tối đa được quét để tìm tiêu đề.</param>
        /// <param name="lastColumn">Cột cuối cùng đang có dữ liệu.</param>
        /// <returns>Số dòng tiêu đề hoặc -1 nếu không tìm thấy.</returns>
        private static int FindImportHeaderRow(IXLWorksheet worksheet, int maxHeaderRow, int lastColumn)
        {
            for (var row = 1; row <= maxHeaderRow; row++)
            {
                var normalizedHeaders = Enumerable.Range(1, lastColumn)
                    .Select(column => NormalizeHeader(worksheet.Cell(row, column).GetString()))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var hasNameHeader = normalizedHeaders.Contains("hovatenvdv") ||
                                    normalizedHeaders.Contains("hovaten") ||
                                    normalizedHeaders.Contains("hoten");
                if (hasNameHeader && normalizedHeaders.Contains("gioitinh"))
                    return row;
            }

            return -1;
        }

        /// <summary>Ánh xạ các tên cột trong file Excel về tên trường chuẩn của hồ sơ vận động viên.</summary>
        /// <param name="worksheet">Worksheet chứa dòng tiêu đề.</param>
        /// <param name="headerRow">Số dòng tiêu đề.</param>
        /// <param name="lastColumn">Cột cuối cùng đang có dữ liệu.</param>
        /// <returns>Từ điển tên trường chuẩn và vị trí cột tương ứng.</returns>
        private static Dictionary<string, int> ResolveImportColumns(IXLWorksheet worksheet, int headerRow, int lastColumn)
        {
            var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var column = 1; column <= lastColumn; column++)
            {
                var normalized = NormalizeHeader(worksheet.Cell(headerRow, column).GetString());
                if (!string.IsNullOrWhiteSpace(normalized) && !headerMap.ContainsKey(normalized))
                    headerMap[normalized] = column;
            }

            var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            AddColumnIfFound(columns, "HoTen", headerMap, "hovatenvdv", "hovatendv", "hovaten", "hoten");
            AddColumnIfFound(columns, "GioiTinh", headerMap, "gioitinh", "gender");
            AddColumnIfFound(columns, "NgaySinh", headerMap, "ngaysinh", "dob");
            AddColumnIfFound(columns, "SoCCCD", headerMap, "socccddinhdanh", "socccd", "cccd", "dinhdanh");
            AddColumnIfFound(columns, "SoDienThoai", headerMap, "sodienthoai", "sdt", "dienthoai");
            AddColumnIfFound(columns, "Email", headerMap, "email");
            AddColumnIfFound(columns, "DiaChi", headerMap, "diachiquequan", "diachi", "quequan");
            AddColumnIfFound(columns, "TrangThai", headerMap, "trangthai", "status");
            AddColumnIfFound(columns, "HinhAnh", headerMap, "anhdaidien", "hinhanh", "urlanh", "linkanh");
            return columns;
        }

        /// <summary>Thêm vị trí cột vào ánh xạ nếu tìm thấy một trong các tên cột thay thế.</summary>
        /// <param name="columns">Ánh xạ cột chuẩn cần cập nhật.</param>
        /// <param name="key">Tên trường chuẩn.</param>
        /// <param name="headerMap">Ánh xạ tên tiêu đề đã chuẩn hóa.</param>
        /// <param name="aliases">Các tên tiêu đề được chấp nhận.</param>
        private static void AddColumnIfFound(Dictionary<string, int> columns, string key, Dictionary<string, int> headerMap, params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                if (headerMap.TryGetValue(alias, out var column))
                {
                    columns[key] = column;
                    return;
                }
            }
        }

        /// <summary>Chuẩn hóa tiêu đề cột bằng cách bỏ dấu, khoảng trắng và ký tự phân cách.</summary>
        /// <param name="value">Tiêu đề gốc.</param>
        /// <returns>Tiêu đề dạng chữ thường, chỉ còn chữ và số.</returns>
        private static string NormalizeHeader(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim().Replace("Đ", "D").Replace("đ", "d").Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
                    builder.Append(character);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        /// <summary>Lấy chuỗi hiển thị của một ô Excel và loại bỏ khoảng trắng dư thừa.</summary>
        /// <param name="worksheet">Worksheet chứa dữ liệu.</param>
        /// <param name="rowNumber">Số dòng cần đọc.</param>
        /// <param name="columnNumber">Số cột cần đọc.</param>
        /// <returns>Giá trị chuỗi đã được chuẩn hóa hoặc chuỗi rỗng khi ô không có dữ liệu.</returns>
        private static string GetCellText(IXLWorksheet worksheet, int rowNumber, int columnNumber)
        {
            return worksheet.Cell(rowNumber, columnNumber).GetString().Trim();
        }

        /// <summary>Kiểm tra dòng dữ liệu có hoàn toàn trống ở các cột được hỗ trợ hay không.</summary>
        /// <param name="worksheet">Worksheet chứa dữ liệu.</param>
        /// <param name="rowNumber">Số dòng cần kiểm tra.</param>
        /// <param name="columns">Ánh xạ các cột dữ liệu.</param>
        /// <returns>True nếu dòng trống và có thể bỏ qua.</returns>
        private static bool IsEmptyImportRow(IXLWorksheet worksheet, int rowNumber, Dictionary<string, int> columns)
        {
            return columns.Values
                .Distinct()
                .All(column => worksheet.Cell(rowNumber, column).IsEmpty() || string.IsNullOrWhiteSpace(GetCellText(worksheet, rowNumber, column)));
        }

        /// <summary>Đọc ngày sinh từ ô Excel dạng ngày, số serial hoặc chuỗi ngày theo định dạng Việt Nam.</summary>
        /// <param name="cell">Ô Excel chứa ngày sinh.</param>
        /// <returns>Ngày sinh hợp lệ hoặc null nếu ô trống, sai định dạng hoặc là ngày tương lai.</returns>
        private static DateTime? TryParseExcelDate(IXLCell cell)
        {
            DateTime? date = null;
            if (cell.DataType == XLDataType.DateTime)
            {
                date = cell.GetDateTime().Date;
            }
            else if (cell.DataType == XLDataType.Number && double.TryParse(cell.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var serialDate))
            {
                try
                {
                    date = DateTime.FromOADate(serialDate).Date;
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
            else
            {
                var text = cell.GetString().Trim();
                var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy" };
                if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
                    date = exactDate.Date;
                else if (DateTime.TryParse(text, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var parsedDate))
                    date = parsedDate.Date;
            }

            return date.HasValue && date.Value <= DateTime.Today ? date : null;
        }

        /// <summary>Đọc ngày sinh dạng chuỗi từ dữ liệu tạm và kiểm tra ngày không nằm trong tương lai.</summary>
        /// <param name="value">Ngày sinh dạng dd/MM/yyyy hoặc định dạng tương đương.</param>
        /// <param name="date">Ngày sinh đã chuyển thành DateTime.</param>
        /// <returns>True nếu chuỗi ngày hợp lệ.</returns>
        private static bool TryParseImportDate(string? value, out DateTime? date)
        {
            date = null;
            var text = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d-M-yyyy" };
            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
                date = exactDate.Date;
            else if (DateTime.TryParse(text, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var parsedDate))
                date = parsedDate.Date;

            return date.HasValue && date.Value <= DateTime.Today;
        }

        /// <summary>Chuẩn hóa giới tính từ các giá trị phổ biến trong file Excel.</summary>
        /// <param name="value">Giá trị giới tính cần chuẩn hóa.</param>
        /// <param name="normalizedValue">Giới tính Nam hoặc Nữ sau khi chuẩn hóa.</param>
        /// <returns>True nếu giá trị hợp lệ.</returns>
        private static bool TryNormalizeGender(string? value, out string normalizedValue)
        {
            var normalized = NormalizeHeader(value);
            if (normalized is "nam" or "male" or "m" or "1")
            {
                normalizedValue = "Nam";
                return true;
            }

            if (normalized is "nu" or "female" or "f" or "0")
            {
                normalizedValue = "Nữ";
                return true;
            }

            normalizedValue = string.Empty;
            return false;
        }

        /// <summary>Chuẩn hóa số CCCD hoặc số điện thoại bị Excel loại bỏ số 0 ở đầu.</summary>
        /// <param name="value">Chuỗi số đọc được từ ô Excel.</param>
        /// <returns>Chuỗi chỉ gồm các chữ số, có bổ sung số 0 đầu khi phù hợp.</returns>
        private static string NormalizeDigits(string? value)
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length == 11)
                return "0" + digits;
            return digits;
        }

        /// <summary>Chuẩn hóa số điện thoại dạng số thuần hoặc dạng +84.</summary>
        /// <param name="value">Số điện thoại đọc được từ Excel.</param>
        /// <returns>Số điện thoại sau khi loại bỏ khoảng trắng và bổ sung số 0 đầu khi cần.</returns>
        private static string NormalizePhone(string? value)
        {
            var phone = (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
            if (phone.Length == 9 && phone.All(char.IsDigit))
                return "0" + phone;
            return phone;
        }

        /// <summary>Kiểm tra số điện thoại theo định dạng được sử dụng trong hồ sơ VĐV.</summary>
        /// <param name="value">Số điện thoại cần kiểm tra.</param>
        /// <returns>True nếu số điện thoại hợp lệ.</returns>
        private static bool IsValidPhone(string value)
        {
            return Regex.IsMatch(value, @"^(?:0\d{9}|\+84\d{9})$");
        }

        /// <summary>Đọc trạng thái hoạt động từ các giá trị tiếng Việt hoặc boolean thường gặp.</summary>
        /// <param name="value">Giá trị trạng thái trong Excel.</param>
        /// <param name="status">Trạng thái đã chuẩn hóa.</param>
        /// <returns>True nếu ô trống hoặc giá trị hợp lệ; false nếu giá trị không được hỗ trợ.</returns>
        private static bool TryParseStatus(string? value, out bool status)
        {
            var normalized = NormalizeHeader(value);
            if (string.IsNullOrWhiteSpace(normalized) || normalized is "sang" or "sansang" or "true" or "1" or "active" or "danghoatdong")
            {
                status = true;
                return true;
            }

            if (normalized is "tamngung" or "false" or "0" or "inactive")
            {
                status = false;
                return true;
            }

            status = true;
            return false;
        }

        /// <summary>Tạo mã VĐV duy nhất theo mã đơn vị để dùng cho hồ sơ nhập mới.</summary>
        /// <param name="unitCode">Mã đơn vị quản lý.</param>
        /// <returns>Mã VĐV mới.</returns>
        private static string GenerateAthleteCode(string? unitCode)
        {
            var normalizedUnitCode = string.IsNullOrWhiteSpace(unitCode) ? "DV" : unitCode.Trim();
            if (normalizedUnitCode.Length > 13)
                normalizedUnitCode = normalizedUnitCode.Substring(0, 13);

            return $"VDV-{normalizedUnitCode}-{Guid.NewGuid():N}";
        }
    }
}
