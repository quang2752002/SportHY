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
    /// Service quản lý ghi nhận, cập nhật, hiển thị và thu hồi huy chương trao cho các đơn vị/VĐV
    /// </summary>
    public class HuyChuongService : IHuyChuongService
    {
        private readonly IUnitOfWork _unitOfWork;

        public HuyChuongService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách huy chương đã trao, có thể lọc theo giải đấu
        /// </summary>
        /// <param name="giaiDauId">Mã giải đấu (tùy chọn)</param>
        /// <returns>Danh sách chi tiết các huy chương đã trao</returns>
        public async Task<IEnumerable<HuyChuongDto>> GetAllAsync(int? giaiDauId = null)
        {
            var huyChuongs = (await _unitOfWork.HuyChuongs.FindAsync(h =>
                h.IsDeleted != true &&
                (!giaiDauId.HasValue || giaiDauId.Value == 0 || h.GiaiDauId == giaiDauId.Value)
            )).ToList();

            if (!huyChuongs.Any()) return Enumerable.Empty<HuyChuongDto>();

            var gIds = huyChuongs.Select(h => h.GiaiDauId).Distinct().ToList();
            var gdmIds = huyChuongs.Select(h => h.GiaiDauMonTheThaoId).Distinct().ToList();
            var dkIds = huyChuongs.Select(h => h.DangKyThiDauId).Distinct().ToList();
            var lhcIds = huyChuongs.Select(h => h.LoaiHuyChuongId).Distinct().ToList();

            var giaiDaus = (await _unitOfWork.GiaiDaus.FindAsync(g => gIds.Contains(g.Id))).ToDictionary(g => g.Id);
            var gdms = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(g => gdmIds.Contains(g.Id))).ToDictionary(g => g.Id);
            var monIds = gdms.Values.Select(m => m.MonTheThaoId).Distinct().ToList();
            var mons = (await _unitOfWork.MonTheThaos.FindAsync(m => monIds.Contains(m.Id))).ToDictionary(m => m.Id);

            var dks = (await _unitOfWork.DangKyThiDaus.FindAsync(d => dkIds.Contains(d.Id))).ToDictionary(d => d.Id);
            var lhcs = (await _unitOfWork.LoaiHuyChuongs.FindAsync(l => lhcIds.Contains(l.Id))).ToDictionary(l => l.Id);

            var dois = (await _unitOfWork.Dois.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var donVis = (await _unitOfWork.DonVis.FindAsync(d => d.IsDeleted != true)).ToDictionary(d => d.Id);
            var chiTiets = (await _unitOfWork.ChiTietDangKyThiDaus.FindAsync(c => dkIds.Contains(c.DangKyThiDauId) && c.IsDeleted != true)).ToList();
            var vdvIds = chiTiets.Select(c => c.VanDongVienId).Distinct().ToList();
            var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id))).ToDictionary(v => v.Id);

            var result = new List<HuyChuongDto>();
            foreach (var hc in huyChuongs)
            {
                giaiDaus.TryGetValue(hc.GiaiDauId, out var gd);
                gdms.TryGetValue(hc.GiaiDauMonTheThaoId, out var gdm);
                MonTheThao? mon = null;
                if (gdm != null) mons.TryGetValue(gdm.MonTheThaoId, out mon);

                dks.TryGetValue(hc.DangKyThiDauId, out var dk);
                lhcs.TryGetValue(hc.LoaiHuyChuongId, out var lhc);

                string? tenDonVi = null;
                string? tenVdv = null;
                string? tenDoi = null;

                if (dk != null)
                {
                    if (dk.DoiId.HasValue && dois.TryGetValue(dk.DoiId.Value, out var doi))
                    {
                        tenDoi = doi.Ten;
                        if (doi.DonViId.HasValue && donVis.TryGetValue(doi.DonViId.Value, out var dvDoi))
                        {
                            tenDonVi = dvDoi.Ten;
                        }
                    }

                    var ctList = chiTiets.Where(c => c.DangKyThiDauId == dk.Id).ToList();
                    if (ctList.Any())
                    {
                        var vdvNames = ctList
                            .Select(c => vdvs.TryGetValue(c.VanDongVienId, out var v) ? v.HoTen : "")
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                        tenVdv = string.Join(", ", vdvNames);

                        if (string.IsNullOrEmpty(tenDonVi))
                        {
                            var firstVdvId = ctList.First().VanDongVienId;
                            if (vdvs.TryGetValue(firstVdvId, out var fVdv) && fVdv.DonViId.HasValue && donVis.TryGetValue(fVdv.DonViId.Value, out var dvVdv))
                            {
                                tenDonVi = dvVdv.Ten;
                            }
                        }
                    }
                }

                result.Add(new HuyChuongDto
                {
                    Id = hc.Id,
                    GiaiDauId = hc.GiaiDauId,
                    TenGiaiDau = gd?.Ten,
                    GiaiDauMonTheThaoId = hc.GiaiDauMonTheThaoId,
                    TenMonTheThao = mon?.Ten,
                    DangKyThiDauId = hc.DangKyThiDauId,
                    TenDangKy = dk?.TenDangKy,
                    TenDonVi = tenDonVi,
                    TenVanDongVien = tenVdv,
                    TenDoi = tenDoi,
                    LoaiHuyChuongId = hc.LoaiHuyChuongId,
                    TenLoaiHuyChuong = lhc?.Ten,
                    XepHang = hc.XepHang,
                    NgayTrao = hc.NgayTrao,
                    GhiChu = hc.GhiChu,
                    Created = hc.Created,
                    LastModified = hc.LastModified
                });
            }

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết một huy chương theo định danh Id
        /// </summary>
        /// <param name="id">Mã huy chương</param>
        /// <returns>Thông tin DTO của huy chương hoặc null nếu không tồn tại</returns>
        public async Task<HuyChuongDto?> GetByIdAsync(int id)
        {
            var hc = await _unitOfWork.HuyChuongs.GetByIdAsync(id);
            if (hc == null || hc.IsDeleted == true) return null;

            var list = await GetAllAsync(hc.GiaiDauId);
            return list.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Ghi nhận trao mới một huy chương cho giải đấu
        /// </summary>
        /// <param name="dto">Dữ liệu tạo huy chương</param>
        /// <param name="createdBy">Tài khoản người thực hiện</param>
        /// <returns>Thông tin huy chương đã được tạo</returns>
        public async Task<HuyChuongDto> CreateAsync(CreateUpdateHuyChuongDto dto, string? createdBy = null)
        {
            var entity = new HuyChuong
            {
                GiaiDauId = dto.GiaiDauId,
                GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                DangKyThiDauId = dto.DangKyThiDauId,
                LoaiHuyChuongId = dto.LoaiHuyChuongId,
                XepHang = dto.XepHang,
                NgayTrao = dto.NgayTrao ?? DateTime.Now,
                GhiChu = dto.GhiChu,
                CreatedBy = createdBy,
                Created = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.HuyChuongs.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return (await GetByIdAsync(entity.Id))!;
        }

        /// <summary>
        /// Cập nhật thông tin huy chương đã trao
        /// </summary>
        /// <param name="id">Mã huy chương cần sửa</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <param name="updatedBy">Tài khoản người cập nhật</param>
        /// <returns>Thông tin huy chương sau cập nhật hoặc null nếu không tìm thấy</returns>
        public async Task<HuyChuongDto?> UpdateAsync(int id, CreateUpdateHuyChuongDto dto, string? updatedBy = null)
        {
            var entity = await _unitOfWork.HuyChuongs.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            entity.GiaiDauId = dto.GiaiDauId;
            entity.GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId;
            entity.DangKyThiDauId = dto.DangKyThiDauId;
            entity.LoaiHuyChuongId = dto.LoaiHuyChuongId;
            entity.XepHang = dto.XepHang;
            entity.NgayTrao = dto.NgayTrao;
            entity.GhiChu = dto.GhiChu;
            entity.LastModifiedBy = updatedBy;
            entity.LastModified = DateTime.UtcNow;

            _unitOfWork.HuyChuongs.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(id);
        }

        /// <summary>
        /// Thu hồi hoặc xóa mềm huy chương khỏi hệ thống (IsDeleted = true)
        /// </summary>
        /// <param name="id">Mã định danh huy chương cần thu hồi</param>
        /// <returns>True nếu thu hồi thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.HuyChuongs.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;

            _unitOfWork.HuyChuongs.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
