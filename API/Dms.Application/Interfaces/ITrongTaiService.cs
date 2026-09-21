using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface ITrongTaiService
    {
        Task<PagedResult<TrongTaiDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);
        Task<IEnumerable<TrongTaiDto>> GetAllAsync();
        Task<TrongTaiDto?> GetByIdAsync(int id);
        Task<TrongTaiDto> CreateAsync(CreateUpdateTrongTaiDto dto, string? createdBy = null);
        Task<TrongTaiDto?> UpdateAsync(int id, CreateUpdateTrongTaiDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
