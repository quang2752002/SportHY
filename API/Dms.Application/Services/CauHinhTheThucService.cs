using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    /// <summary>
    /// Triển khai dịch vụ quản lý cấu hình thể thức thi đấu và luật tính điểm cho các môn thể thao.
    /// </summary>
    public class CauHinhTheThucService : ICauHinhTheThucService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CauHinhTheThucService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy thông tin cấu hình thể thức thi đấu áp dụng cho môn thể thao hoặc giải đấu cụ thể.
        /// Ưu tiên cấu hình riêng theo giải đấu nếu có, nếu không lấy cấu hình mặc định của môn.
        /// </summary>
        /// <param name="monTheThaoId">Mã định danh môn thể thao</param>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn trong giải đấu (tùy chọn)</param>
        /// <returns>Đối tượng DTO cấu hình thể thức thi đấu tương ứng</returns>
        public async Task<CauHinhTheThucDto?> GetEffectiveConfigAsync(int monTheThaoId, int? giaiDauMonTheThaoId = null)
        {
            // 1. Kiểm tra cấu hình riêng của môn trong giải
            if (giaiDauMonTheThaoId.HasValue && giaiDauMonTheThaoId.Value > 0)
            {
                var specificConfigs = await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(
                    c => c.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value && c.IsDeleted != true
                );
                var specific = specificConfigs.FirstOrDefault();
                if (specific != null)
                {
                    return MapToDto(specific);
                }
            }

            // 2. Lấy cấu hình mặc định của môn thể thao
            var defaultConfigs = await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(
                c => c.MonTheThaoId == monTheThaoId && c.GiaiDauMonTheThaoId == null && c.IsDeleted != true
            );
            var def = defaultConfigs.FirstOrDefault();
            if (def != null)
            {
                return MapToDto(def);
            }

            // 3. Fallback: Nếu chưa có trong DB, trả về cấu hình chuẩn tạo sẵn
            var mon = await _unitOfWork.MonTheThaos.GetByIdAsync(monTheThaoId);
            return CreateFallbackDto(monTheThaoId, mon?.Ten, mon?.Ma);
        }

        /// <summary>
        /// Lấy cấu hình thể thức áp dụng trực tiếp cho một trận đấu cụ thể dựa trên ID trận đấu.
        /// </summary>
        /// <param name="tranDauId">Mã định danh trận đấu</param>
        /// <returns>Đối tượng DTO cấu hình thể thức của môn trong trận đấu đó</returns>
        public async Task<CauHinhTheThucDto?> GetConfigByTranDauIdAsync(int tranDauId)
        {
            var match = await _unitOfWork.TranDaus.GetByIdAsync(tranDauId);
            if (match == null) return null;

            var gdm = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(match.GiaiDauMonTheThaoId);
            if (gdm == null) return null;

            return await GetEffectiveConfigAsync(gdm.MonTheThaoId, gdm.Id);
        }

        /// <summary>
        /// Lấy toàn bộ danh sách cấu hình thể thức thi đấu trong hệ thống.
        /// </summary>
        /// <returns>Danh sách các cấu hình thể thức</returns>
        public async Task<IEnumerable<CauHinhTheThucDto>> GetAllAsync()
        {
            var paged = await _unitOfWork.CauHinhTheThucThiDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1000,
                predicate: c => c.IsDeleted != true,
                orderBy: q => q.OrderBy(c => c.MonTheThaoId),
                c => c.MonTheThao,
                c => c.GiaiDauMonTheThao!
            );

            return paged.Items.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Thêm mới hoặc cập nhật cấu hình thể thức cho môn thể thao / giải đấu.
        /// </summary>
        /// <param name="dto">Dữ liệu cấu hình cần lưu</param>
        /// <param name="username">Tên người thực hiện thao tác</param>
        /// <returns>Cấu hình đã lưu sau khi cập nhật</returns>
        public async Task<CauHinhTheThucDto> UpsertConfigAsync(CreateUpdateCauHinhTheThucDto dto, string? username = null)
        {
            var configs = await _unitOfWork.CauHinhTheThucThiDaus.FindAsync(c =>
                c.MonTheThaoId == dto.MonTheThaoId &&
                c.GiaiDauMonTheThaoId == dto.GiaiDauMonTheThaoId &&
                c.IsDeleted != true
            );
            var entity = configs.FirstOrDefault();

            if (entity == null)
            {
                entity = new CauHinhTheThucThiDau
                {
                    MonTheThaoId = dto.MonTheThaoId,
                    GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                    Created = DateTime.UtcNow,
                    CreatedBy = username,
                    IsDeleted = false
                };
                CopyProperties(dto, entity);
                await _unitOfWork.CauHinhTheThucThiDaus.AddAsync(entity);
            }
            else
            {
                CopyProperties(dto, entity);
                entity.LastModified = DateTime.UtcNow;
                entity.LastModifiedBy = username;
                _unitOfWork.CauHinhTheThucThiDaus.Update(entity);
            }

            await _unitOfWork.CompleteAsync();
            return (await GetEffectiveConfigAsync(entity.MonTheThaoId, entity.GiaiDauMonTheThaoId))!;
        }

        private static void CopyProperties(CreateUpdateCauHinhTheThucDto source, CauHinhTheThucThiDau target)
        {
            target.LoaiTheThuc = source.LoaiTheThuc;
            target.SoHiepToiDa = source.SoHiepToiDa;
            target.SoHiepThangDeThangTran = source.SoHiepThangDeThangTran;
            target.DiemMoiHiep = source.DiemMoiHiep;
            target.DiemHiepQuyetDinh = source.DiemHiepQuyetDinh;
            target.CachBietDiemToiThieu = source.CachBietDiemToiThieu;
            target.DiemToiDaMoiHiep = source.DiemToiDaMoiHiep;
            target.ThoiGianHiepChinhPhut = source.ThoiGianHiepChinhPhut;

            target.ChoPhepHoaVongBang = source.ChoPhepHoaVongBang;
            target.ChoPhepHoaKnockout = source.ChoPhepHoaKnockout;
            target.CoHiepPhu = source.CoHiepPhu;
            target.SoHiepPhu = source.SoHiepPhu;
            target.ThoiGianHiepPhuPhut = source.ThoiGianHiepPhuPhut;
            target.CoPenalty = source.CoPenalty;
            target.SoLuotPenaltyMoiDoi = source.SoLuotPenaltyMoiDoi;
            target.CoTieBreak = source.CoTieBreak;

            target.DiemThang = source.DiemThang;
            target.DiemHoa = source.DiemHoa;
            target.DiemThua = source.DiemThua;
            target.DiemThuaBocCuoc = source.DiemThuaBocCuoc;
            target.CachTinhDiemTheoSet = source.CachTinhDiemTheoSet;
            target.TieuChiXepHangJson = string.IsNullOrWhiteSpace(source.TieuChiXepHangJson)
                ? "[\"Diem\",\"HieuSo\",\"DiemGhiDuoc\",\"DoiDau\",\"SoTranThang\"]"
                : source.TieuChiXepHangJson;

            target.LoaiDoThanhTich = source.LoaiDoThanhTich;
            target.DonViThanhTich = source.DonViThanhTich;
            target.TieuChiXepHangThanhTich = source.TieuChiXepHangThanhTich;
            target.SoVdvMoiLuotThi = source.SoVdvMoiLuotThi > 0 ? source.SoVdvMoiLuotThi : 8;
            target.QuyCachTienVaoChungKet = source.QuyCachTienVaoChungKet;
            target.SoVdvVaoChungKet = source.SoVdvVaoChungKet;
            target.KyLucHienTai = source.KyLucHienTai;
            target.KyLucHienTaiText = source.KyLucHienTaiText;
        }

        private static CauHinhTheThucDto MapToDto(CauHinhTheThucThiDau entity)
        {
            return new CauHinhTheThucDto
            {
                Id = entity.Id,
                MonTheThaoId = entity.MonTheThaoId,
                TenMonTheThao = entity.MonTheThao?.Ten,
                GiaiDauMonTheThaoId = entity.GiaiDauMonTheThaoId,
                TenGiaiDau = entity.GiaiDauMonTheThao?.GiaiDau?.Ten,
                LoaiTheThuc = entity.LoaiTheThuc,
                SoHiepToiDa = entity.SoHiepToiDa,
                SoHiepThangDeThangTran = entity.SoHiepThangDeThangTran,
                DiemMoiHiep = entity.DiemMoiHiep,
                DiemHiepQuyetDinh = entity.DiemHiepQuyetDinh,
                CachBietDiemToiThieu = entity.CachBietDiemToiThieu,
                DiemToiDaMoiHiep = entity.DiemToiDaMoiHiep,
                ThoiGianHiepChinhPhut = entity.ThoiGianHiepChinhPhut,
                ChoPhepHoaVongBang = entity.ChoPhepHoaVongBang,
                ChoPhepHoaKnockout = entity.ChoPhepHoaKnockout,
                CoHiepPhu = entity.CoHiepPhu,
                SoHiepPhu = entity.SoHiepPhu,
                ThoiGianHiepPhuPhut = entity.ThoiGianHiepPhuPhut,
                CoPenalty = entity.CoPenalty,
                SoLuotPenaltyMoiDoi = entity.SoLuotPenaltyMoiDoi,
                CoTieBreak = entity.CoTieBreak,
                DiemThang = entity.DiemThang,
                DiemHoa = entity.DiemHoa,
                DiemThua = entity.DiemThua,
                DiemThuaBocCuoc = entity.DiemThuaBocCuoc,
                CachTinhDiemTheoSet = entity.CachTinhDiemTheoSet,
                TieuChiXepHangJson = entity.TieuChiXepHangJson,
                LoaiDoThanhTich = entity.LoaiDoThanhTich,
                DonViThanhTich = entity.DonViThanhTich,
                TieuChiXepHangThanhTich = entity.TieuChiXepHangThanhTich,
                SoVdvMoiLuotThi = entity.SoVdvMoiLuotThi,
                QuyCachTienVaoChungKet = entity.QuyCachTienVaoChungKet,
                SoVdvVaoChungKet = entity.SoVdvVaoChungKet,
                KyLucHienTai = entity.KyLucHienTai,
                KyLucHienTaiText = entity.KyLucHienTaiText
            };
        }

        private static CauHinhTheThucDto CreateFallbackDto(int monId, string? tenMon, string? maMon)
        {
            var isBongDa = (maMon ?? "").Contains("BONG_DA", StringComparison.OrdinalIgnoreCase);
            var isChuyen = (maMon ?? "").Contains("BONG_CHUYEN", StringComparison.OrdinalIgnoreCase);
            var isBoiChay = (maMon ?? "").Contains("BOI", StringComparison.OrdinalIgnoreCase) || (maMon ?? "").Contains("CHAY", StringComparison.OrdinalIgnoreCase);

            if (isBongDa)
            {
                return new CauHinhTheThucDto
                {
                    MonTheThaoId = monId,
                    TenMonTheThao = tenMon,
                    LoaiTheThuc = "ThoiGianHiep",
                    SoHiepToiDa = 2,
                    ThoiGianHiepChinhPhut = 25,
                    ChoPhepHoaVongBang = true,
                    ChoPhepHoaKnockout = false,
                    CoPenalty = true,
                    DiemThang = 3,
                    DiemHoa = 1,
                    DiemThua = 0
                };
            }

            if (isChuyen)
            {
                return new CauHinhTheThucDto
                {
                    MonTheThaoId = monId,
                    TenMonTheThao = tenMon,
                    LoaiTheThuc = "SetDiem",
                    SoHiepToiDa = 5,
                    SoHiepThangDeThangTran = 3,
                    DiemMoiHiep = 25,
                    DiemHiepQuyetDinh = 15,
                    CachTinhDiemTheoSet = true,
                    ChoPhepHoaVongBang = false,
                    ChoPhepHoaKnockout = false
                };
            }

            if (isBoiChay)
            {
                return new CauHinhTheThucDto
                {
                    MonTheThaoId = monId,
                    TenMonTheThao = tenMon,
                    LoaiTheThuc = "TinhDiemXepHang",
                    LoaiDoThanhTich = "ThoiGian",
                    DonViThanhTich = "giay",
                    TieuChiXepHangThanhTich = "CangNhoCangTot",
                    SoVdvMoiLuotThi = 8,
                    QuyCachTienVaoChungKet = "TopNToanVong",
                    SoVdvVaoChungKet = 8
                };
            }

            // Mặc định Cầu lông Best of 3
            return new CauHinhTheThucDto
            {
                MonTheThaoId = monId,
                TenMonTheThao = tenMon,
                LoaiTheThuc = "SetDiem",
                SoHiepToiDa = 3,
                SoHiepThangDeThangTran = 2,
                DiemMoiHiep = 21,
                DiemHiepQuyetDinh = 21,
                CachBietDiemToiThieu = 2,
                DiemToiDaMoiHiep = 30,
                ChoPhepHoaVongBang = false,
                ChoPhepHoaKnockout = false,
                DiemThang = 2,
                DiemHoa = 0,
                DiemThua = 0
            };
        }
    }
}
