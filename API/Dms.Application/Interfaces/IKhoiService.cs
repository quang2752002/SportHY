using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IKhoiService
    {
        Task<PagedResult<KhoiDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);
        Task<IEnumerable<KhoiDto>> GetAllAsync();
        Task<KhoiDto?> GetByIdAsync(int id);
        Task<KhoiDto> CreateAsync(CreateUpdateKhoiDto dto, string? createdBy = null);
        Task<KhoiDto?> UpdateAsync(int id, CreateUpdateKhoiDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
