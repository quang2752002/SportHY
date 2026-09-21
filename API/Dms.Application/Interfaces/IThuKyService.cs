using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IThuKyService
    {
        Task<PagedResult<ThuKyDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null);
        Task<IEnumerable<ThuKyDto>> GetAllAsync();
        Task<ThuKyDto?> GetByIdAsync(int id);
        Task<ThuKyDto> CreateAsync(CreateUpdateThuKyDto dto, string? createdBy = null);
        Task<ThuKyDto?> UpdateAsync(int id, CreateUpdateThuKyDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
