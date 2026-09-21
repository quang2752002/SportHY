using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IDanhMucMonTheThaoService
    {
        Task<PagedResult<DanhMucMonTheThaoDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);
        Task<IEnumerable<DanhMucMonTheThaoDto>> GetAllAsync();
        Task<DanhMucMonTheThaoDto?> GetByIdAsync(int id);
        Task<DanhMucMonTheThaoDto> CreateAsync(CreateUpdateDanhMucMonTheThaoDto dto, string? createdBy = null);
        Task<DanhMucMonTheThaoDto?> UpdateAsync(int id, CreateUpdateDanhMucMonTheThaoDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
