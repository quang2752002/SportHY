using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IMonTheThaoService
    {
        Task<PagedResult<MonTheThaoDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, int? danhMucId = null, bool? trangThai = null, string? gioiTinh = null);
        Task<IEnumerable<MonTheThaoDto>> GetAllAsync(int? danhMucId = null, string? gioiTinh = null);
        Task<MonTheThaoDto?> GetByIdAsync(int id);
        Task<MonTheThaoDto> CreateAsync(CreateUpdateMonTheThaoDto dto, string? createdBy = null);
        Task<MonTheThaoDto?> UpdateAsync(int id, CreateUpdateMonTheThaoDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
