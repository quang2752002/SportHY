using Dms.Application.DTOs;
using Dms.Domain.Common;
using Dms.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IGiaiDauService
    {
        Task<PagedResult<GiaiDauDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, TrangThaiGiaiDau? trangThai = null, PhamViGiaiDau? phamVi = null);
        Task<IEnumerable<GiaiDauDto>> GetAllAsync();
        Task<GiaiDauDto?> GetByIdAsync(int id);
        Task<GiaiDauDto?> GetBySlugAsync(string slug);
        Task<GiaiDauDto> CreateAsync(CreateUpdateGiaiDauDto dto, string? createdBy = null);
        Task<GiaiDauDto?> UpdateAsync(int id, CreateUpdateGiaiDauDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
