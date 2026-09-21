using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface ICumSanService
    {
        Task<PagedResult<CumSanDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);
        Task<IEnumerable<CumSanDto>> GetAllAsync();
        Task<CumSanDto?> GetByIdAsync(int id);
        Task<CumSanDto> CreateAsync(CreateUpdateCumSanDto dto, string? createdBy = null);
        Task<CumSanDto?> UpdateAsync(int id, CreateUpdateCumSanDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }

    public interface ISanDauService
    {
        Task<PagedResult<SanDauDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, int? cumSanId = null, int? monTheThaoId = null, bool? trangThai = null);
        Task<IEnumerable<SanDauDto>> GetAllAsync(int? cumSanId = null, int? monTheThaoId = null);
        Task<SanDauDto?> GetByIdAsync(int id);
        Task<SanDauDto> CreateAsync(CreateUpdateSanDauDto dto, string? createdBy = null);
        Task<SanDauDto?> UpdateAsync(int id, CreateUpdateSanDauDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
