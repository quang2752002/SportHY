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
        /// <summary>
        /// Lấy các điều lệ đang được công bố của một giải và từng môn thi đấu trong giải.
        /// </summary>
        /// <param name="giaiDauId">Mã giải đấu cần xem điều lệ.</param>
        /// <param name="giaiDauMonTheThaoId">Mã môn thuộc giải cần lọc; bỏ trống để lấy tất cả các môn.</param>
        /// <returns>Thông tin điều lệ công khai, hoặc null nếu giải không tồn tại/chưa được công bố.</returns>
        Task<GiaiDauDieuLeCongKhaiDto?> GetPublicRegulationsAsync(int giaiDauId, int? giaiDauMonTheThaoId = null);
        Task<GiaiDauDto?> GetBySlugAsync(string slug);
        Task<GiaiDauDto> CreateAsync(CreateUpdateGiaiDauDto dto, string? createdBy = null);
        Task<GiaiDauDto?> UpdateAsync(int id, CreateUpdateGiaiDauDto dto, string? updatedBy = null);
        Task<bool> DeleteAsync(int id);
    }
}
