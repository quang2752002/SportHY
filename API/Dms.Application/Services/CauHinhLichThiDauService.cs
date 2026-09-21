using AutoMapper;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class CauHinhLichThiDauService : ICauHinhLichThiDauService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CauHinhLichThiDauService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy thông tin cấu hình tham số xếp lịch và thể lực VĐV theo ID môn thể thao.
        /// </summary>
        /// <param name="monTheThaoId">Mã định danh môn thể thao</param>
        /// <returns>DTO cấu hình lịch thi đấu hoặc null nếu chưa có</returns>
        public async Task<CauHinhLichThiDauDto?> GetByMonTheThaoAsync(int monTheThaoId)
        {
            var items = await _unitOfWork.CauHinhLichThiDaus.FindAsync(
                c => c.MonTheThaoId == monTheThaoId && c.IsDeleted != true
            );
            var entity = items.FirstOrDefault();
            if (entity == null) return null;

            var dto = _mapper.Map<CauHinhLichThiDauDto>(entity);
            dto.TenMonTheThao = entity.MonTheThao?.Ten;
            return dto;
        }

        /// <summary>
        /// Thêm mới hoặc cập nhật cấu hình xếp lịch (số trận tối đa/ngày, thời gian nghỉ, khung giờ, ca thi đấu) cho môn thể thao.
        /// </summary>
        /// <param name="dto">Dữ liệu cấu hình cần lưu</param>
        /// <param name="by">Người thực hiện thao tác</param>
        /// <returns>DTO cấu hình sau khi cập nhật thành công</returns>
        public async Task<CauHinhLichThiDauDto> UpsertAsync(CreateUpdateCauHinhLichThiDauDto dto, string? by = null)
        {
            var items = await _unitOfWork.CauHinhLichThiDaus.FindAsync(
                c => c.MonTheThaoId == dto.MonTheThaoId && c.IsDeleted != true
            );
            var entity = items.FirstOrDefault();

            if (entity == null)
            {
                // Create new
                entity = new CauHinhLichThiDau
                {
                    MonTheThaoId = dto.MonTheThaoId,
                    MoiVongMotNgay = dto.MoiVongMotNgay,
                    KhoangCachGiuaCacVongGio = dto.KhoangCachGiuaCacVongGio,
                    UuTienChungKetNgayCuoi = dto.UuTienChungKetNgayCuoi,
                    SoTranToiDaMoiDoiMoiNgay = dto.SoTranToiDaMoiDoiMoiNgay,
                    NghiToiThieuGiua2TranPhut = dto.NghiToiThieuGiua2TranPhut,
                    ChiaCaThiDau = dto.ChiaCaThiDau,
                    CaSangBatDau = dto.CaSangBatDau,
                    CaSangKetThuc = dto.CaSangKetThuc,
                    CaChieuBatDau = dto.CaChieuBatDau,
                    CaChieuKetThuc = dto.CaChieuKetThuc,
                    CaToBatDau = dto.CaToBatDau,
                    CaToKetThuc = dto.CaToKetThuc,
                    ThoiGianDemDonSanPhut = dto.ThoiGianDemDonSanPhut,
                    SoTranToiDaMoiTrongTaiMoiNgay = dto.SoTranToiDaMoiTrongTaiMoiNgay,
                    NghiToiThieuTrongTaiPhut = dto.NghiToiThieuTrongTaiPhut,
                    ThoiGianDemDiChuyenPhut = dto.ThoiGianDemDiChuyenPhut,
                    ThoiLuongTranMacDinhPhut = dto.ThoiLuongTranMacDinhPhut,
                    SoHiepDauMacDinh = dto.SoHiepDauMacDinh,
                    ThoiGianMoiHiepPhut = dto.ThoiGianMoiHiepPhut,
                    GhiChu = dto.GhiChu,
                    Created = DateTime.UtcNow,
                    CreatedBy = by
                };
                await _unitOfWork.CauHinhLichThiDaus.AddAsync(entity);
            }
            else
            {
                // Update
                entity.MoiVongMotNgay = dto.MoiVongMotNgay;
                entity.KhoangCachGiuaCacVongGio = dto.KhoangCachGiuaCacVongGio;
                entity.UuTienChungKetNgayCuoi = dto.UuTienChungKetNgayCuoi;
                entity.SoTranToiDaMoiDoiMoiNgay = dto.SoTranToiDaMoiDoiMoiNgay;
                entity.NghiToiThieuGiua2TranPhut = dto.NghiToiThieuGiua2TranPhut;
                entity.ChiaCaThiDau = dto.ChiaCaThiDau;
                entity.CaSangBatDau = dto.CaSangBatDau;
                entity.CaSangKetThuc = dto.CaSangKetThuc;
                entity.CaChieuBatDau = dto.CaChieuBatDau;
                entity.CaChieuKetThuc = dto.CaChieuKetThuc;
                entity.CaToBatDau = dto.CaToBatDau;
                entity.CaToKetThuc = dto.CaToKetThuc;
                entity.ThoiGianDemDonSanPhut = dto.ThoiGianDemDonSanPhut;
                entity.SoTranToiDaMoiTrongTaiMoiNgay = dto.SoTranToiDaMoiTrongTaiMoiNgay;
                entity.NghiToiThieuTrongTaiPhut = dto.NghiToiThieuTrongTaiPhut;
                entity.ThoiGianDemDiChuyenPhut = dto.ThoiGianDemDiChuyenPhut;
                entity.ThoiLuongTranMacDinhPhut = dto.ThoiLuongTranMacDinhPhut;
                entity.SoHiepDauMacDinh = dto.SoHiepDauMacDinh;
                entity.ThoiGianMoiHiepPhut = dto.ThoiGianMoiHiepPhut;
                entity.GhiChu = dto.GhiChu;
                entity.LastModified = DateTime.UtcNow;
                entity.LastModifiedBy = by;

                _unitOfWork.CauHinhLichThiDaus.Update(entity);
            }

            await _unitOfWork.CompleteAsync();

            var mon = await _unitOfWork.MonTheThaos.GetByIdAsync(dto.MonTheThaoId);
            var result = _mapper.Map<CauHinhLichThiDauDto>(entity);
            result.TenMonTheThao = mon?.Ten;
            return result;
        }

        /// <summary>
        /// Xóa mềm cấu hình xếp lịch của môn thể thao để chuyển về thiết lập mặc định của hệ thống.
        /// </summary>
        /// <param name="monTheThaoId">Mã định danh môn thể thao cần reset cấu hình</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> DeleteAsync(int monTheThaoId)
        {
            var items = await _unitOfWork.CauHinhLichThiDaus.FindAsync(
                c => c.MonTheThaoId == monTheThaoId && c.IsDeleted != true
            );
            var entity = items.FirstOrDefault();
            if (entity == null) return false;

            entity.IsDeleted = true;
            _unitOfWork.CauHinhLichThiDaus.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
