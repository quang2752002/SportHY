using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface ILoaiHuyChuongService
    {
        Task<PagedResult<LoaiHuyChuongDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null);
        Task<IEnumerable<LoaiHuyChuongDto>> GetAllAsync();
        Task<LoaiHuyChuongDto?> GetByIdAsync(int id);
        Task<LoaiHuyChuongDto> CreateAsync(CreateUpdateLoaiHuyChuongDto dto, string? createdBy = null);
        Task<LoaiHuyChuongDto?> UpdateAsync(int id, CreateUpdateLoaiHuyChuongDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
