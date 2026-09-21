using Dms.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuDto>> GetAllAsync();
        Task<IEnumerable<MenuDto>> GetTreeAsync();
        Task<MenuDto?> GetByIdAsync(int id);
        Task<MenuDto> CreateAsync(MenuDto dto);
        Task<MenuDto?> UpdateAsync(int id, MenuDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
