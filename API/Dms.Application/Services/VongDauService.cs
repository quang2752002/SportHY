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
    public class VongDauService : IVongDauService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VongDauService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<VongDauDto>> GetAllAsync(int? giaiDauMonTheThaoId = null)
        {
            var paged = await _unitOfWork.VongDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 500,
                predicate: v => v.IsDeleted != true &&
                                (!giaiDauMonTheThaoId.HasValue || v.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value),
                orderBy: q => q.OrderBy(v => v.ThuTu).ThenBy(v => v.Id),
                v => v.GiaiDauMonTheThao,
                v => v.TranDaus
            );

            return paged.Items.Select(v => new VongDauDto
            {
                Id = v.Id,
                GiaiDauMonTheThaoId = v.GiaiDauMonTheThaoId,
                TenMonTheThao = v.GiaiDauMonTheThao?.MonTheThao?.Ten,
                Ma = $"VONG_{v.Id}",
                Ten = v.Ten,
                ThuTu = v.ThuTu,
                LoaiVongDau = v.LoaiVong,
                SoTran = v.TranDaus.Count(t => t.IsDeleted != true),
                Created = v.Created,
                LastModified = v.LastModified
            }).ToList();
        }

        public async Task<VongDauDto?> GetByIdAsync(int id)
        {
            var paged = await _unitOfWork.VongDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: v => v.Id == id && v.IsDeleted != true,
                orderBy: null,
                v => v.GiaiDauMonTheThao,
                v => v.GiaiDauMonTheThao.MonTheThao,
                v => v.TranDaus
            );

            var v = paged.Items.FirstOrDefault();
            if (v == null) return null;

            return new VongDauDto
            {
                Id = v.Id,
                GiaiDauMonTheThaoId = v.GiaiDauMonTheThaoId,
                TenMonTheThao = v.GiaiDauMonTheThao?.MonTheThao?.Ten,
                Ma = $"VONG_{v.Id}",
                Ten = v.Ten,
                ThuTu = v.ThuTu,
                LoaiVongDau = v.LoaiVong,
                SoTran = v.TranDaus.Count(t => t.IsDeleted != true),
                Created = v.Created,
                LastModified = v.LastModified
            };
        }

        public async Task<VongDauDto> CreateAsync(CreateUpdateVongDauDto dto, string? createdBy = null)
        {
            var entity = new VongDau
            {
                GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                Ten = dto.Ten.Trim(),
                ThuTu = dto.ThuTu,
                LoaiVong = string.IsNullOrWhiteSpace(dto.LoaiVongDau) ? "VongBang" : dto.LoaiVongDau,
                MoTa = dto.MoTa,
                Created = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsDeleted = false
            };

            await _unitOfWork.VongDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<VongDauDto?> UpdateAsync(int id, CreateUpdateVongDauDto dto, string? updatedBy = null)
        {
            var paged = await _unitOfWork.VongDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: v => v.Id == id && v.IsDeleted != true
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Ten)) entity.Ten = dto.Ten.Trim();
            entity.ThuTu = dto.ThuTu;
            if (!string.IsNullOrWhiteSpace(dto.LoaiVongDau)) entity.LoaiVong = dto.LoaiVongDau;
            if (dto.MoTa != null) entity.MoTa = dto.MoTa;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.VongDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var paged = await _unitOfWork.VongDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: v => v.Id == id && v.IsDeleted != true
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.VongDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
