using Dms.Application.DTOs;
using Dms.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dms.Application.Interfaces
{
    public interface ICumSanService
    {
        /// <summary>Lấy danh sách cụm sân có phân trang, tìm kiếm và giới hạn theo đơn vị sở hữu nếu được cung cấp.</summary>
        /// <param name="pageIndex">Số trang bắt đầu từ 1.</param>
        /// <param name="pageSize">Số lượng bản ghi trên một trang.</param>
        /// <param name="keyword">Từ khóa tìm theo mã, tên hoặc địa chỉ cụm sân.</param>
        /// <param name="trangThai">Trạng thái hoạt động cần lọc.</param>
        /// <param name="donViId">ID đơn vị sở hữu; để trống khi đọc toàn hệ thống.</param>
        /// <returns>Kết quả cụm sân đã phân trang, không bao gồm bản ghi xóa mềm.</returns>
        Task<PagedResult<CumSanDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, bool? trangThai = null, int? donViId = null);

        /// <summary>Lấy toàn bộ cụm sân đang hoạt động, có thể giới hạn theo đơn vị sở hữu.</summary>
        /// <param name="donViId">ID đơn vị sở hữu; để trống khi đọc toàn hệ thống.</param>
        /// <returns>Danh sách cụm sân đang hoạt động.</returns>
        Task<IEnumerable<CumSanDto>> GetAllAsync(int? donViId = null);

        /// <summary>Lấy chi tiết một cụm sân và kiểm tra phạm vi đơn vị nếu được cung cấp.</summary>
        /// <param name="id">ID cụm sân cần lấy.</param>
        /// <param name="donViId">ID đơn vị sở hữu; để trống khi đọc toàn hệ thống.</param>
        /// <returns>Thông tin cụm sân hoặc null nếu không tồn tại trong phạm vi.</returns>
        Task<CumSanDto?> GetByIdAsync(int id, int? donViId = null);

        /// <summary>Tạo cụm sân cho dữ liệu quản trị hệ thống với đơn vị sở hữu do request chỉ định.</summary>
        /// <param name="dto">Thông tin cụm sân, bắt buộc có đơn vị sở hữu.</param>
        /// <param name="createdBy">Tên tài khoản tạo dữ liệu.</param>
        /// <returns>Cụm sân vừa tạo.</returns>
        Task<CumSanDto> CreateAsync(CreateUpdateCumSanDto dto, string? createdBy = null);

        /// <summary>Cập nhật cụm sân trong phạm vi quản trị hệ thống.</summary>
        /// <param name="id">ID cụm sân cần cập nhật.</param>
        /// <param name="dto">Thông tin mới của cụm sân.</param>
        /// <param name="updatedBy">Tên tài khoản cập nhật dữ liệu.</param>
        /// <returns>Cụm sân sau cập nhật hoặc null nếu không tồn tại.</returns>
        Task<CumSanDto?> UpdateAsync(int id, CreateUpdateCumSanDto dto, string? updatedBy = null);

        /// <summary>Xóa mềm một cụm sân trong phạm vi quản trị hệ thống.</summary>
        /// <param name="id">ID cụm sân cần xóa mềm.</param>
        /// <returns>True nếu cập nhật xóa mềm thành công.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>Tạo cụm sân và tự động gán quyền sở hữu cho đơn vị hiện tại.</summary>
        /// <param name="dto">Thông tin cụm sân không bao gồm quyền sở hữu tin cậy từ client.</param>
        /// <param name="donViId">ID đơn vị hiện tại.</param>
        /// <param name="createdBy">Tên tài khoản tạo dữ liệu.</param>
        /// <returns>Cụm sân vừa tạo thuộc đơn vị.</returns>
        Task<CumSanDto> CreateForDonViAsync(CreateUpdateCumSanDto dto, int donViId, string? createdBy = null);

        /// <summary>Cập nhật cụm sân nếu cụm sân thuộc đúng đơn vị hiện tại.</summary>
        /// <param name="id">ID cụm sân cần cập nhật.</param>
        /// <param name="dto">Thông tin mới của cụm sân.</param>
        /// <param name="donViId">ID đơn vị hiện tại.</param>
        /// <param name="updatedBy">Tên tài khoản cập nhật dữ liệu.</param>
        /// <returns>Cụm sân sau cập nhật hoặc null nếu ngoài phạm vi.</returns>
        Task<CumSanDto?> UpdateForDonViAsync(int id, CreateUpdateCumSanDto dto, int donViId, string? updatedBy = null);

        /// <summary>Xóa mềm cụm sân nếu cụm sân thuộc đúng đơn vị hiện tại.</summary>
        /// <param name="id">ID cụm sân cần xóa mềm.</param>
        /// <param name="donViId">ID đơn vị hiện tại.</param>
        /// <returns>True nếu xóa mềm thành công trong phạm vi đơn vị.</returns>
        Task<bool> DeleteForDonViAsync(int id, int donViId);
    }

    public interface ISanDauService
    {
        /// <summary>Lấy danh sách sân đấu có phân trang, bộ lọc và giới hạn theo đơn vị sở hữu.</summary>
        /// <param name="pageIndex">Số trang bắt đầu từ 1.</param>
        /// <param name="pageSize">Số lượng bản ghi trên một trang.</param>
        /// <param name="keyword">Từ khóa tìm theo mã, tên hoặc loại sân.</param>
        /// <param name="cumSanId">ID cụm sân cần lọc.</param>
        /// <param name="monTheThaoId">ID môn thể thao cần lọc.</param>
        /// <param name="trangThai">Trạng thái hoạt động cần lọc.</param>
        /// <param name="donViId">ID đơn vị sở hữu; để trống khi đọc toàn hệ thống.</param>
        /// <returns>Kết quả sân đấu đã phân trang, không bao gồm dữ liệu xóa mềm.</returns>
        Task<PagedResult<SanDauDto>> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null, int? cumSanId = null, int? monTheThaoId = null, bool? trangThai = null, int? donViId = null);

        /// <summary>Lấy toàn bộ sân đấu đang hoạt động theo cụm sân, môn thể thao và đơn vị sở hữu.</summary>
        /// <param name="cumSanId">ID cụm sân cần lọc.</param>
        /// <param name="monTheThaoId">ID môn thể thao cần lọc.</param>
        /// <param name="donViId">ID đơn vị sở hữu; để trống khi đọc toàn hệ thống.</param>
        /// <returns>Danh sách sân đấu đang hoạt động.</returns>
        Task<IEnumerable<SanDauDto>> GetAllAsync(int? cumSanId = null, int? monTheThaoId = null, int? donViId = null);

        /// <summary>Lấy chi tiết sân đấu và kiểm tra phạm vi đơn vị nếu được cung cấp.</summary>
        /// <param name="id">ID sân đấu cần lấy.</param>
        /// <param name="donViId">ID đơn vị sở hữu; để trống khi đọc toàn hệ thống.</param>
        /// <returns>Thông tin sân đấu hoặc null nếu không tồn tại trong phạm vi.</returns>
        Task<SanDauDto?> GetByIdAsync(int id, int? donViId = null);

        /// <summary>Tạo sân đấu trong phạm vi quản trị hệ thống.</summary>
        /// <param name="dto">Thông tin sân đấu.</param>
        /// <param name="createdBy">Tên tài khoản tạo dữ liệu.</param>
        /// <returns>Sân đấu vừa tạo.</returns>
        Task<SanDauDto> CreateAsync(CreateUpdateSanDauDto dto, string? createdBy = null);

        /// <summary>Cập nhật sân đấu trong phạm vi quản trị hệ thống.</summary>
        /// <param name="id">ID sân đấu cần cập nhật.</param>
        /// <param name="dto">Thông tin mới của sân đấu.</param>
        /// <param name="updatedBy">Tên tài khoản cập nhật dữ liệu.</param>
        /// <returns>Sân đấu sau cập nhật hoặc null nếu không tồn tại.</returns>
        Task<SanDauDto?> UpdateAsync(int id, CreateUpdateSanDauDto dto, string? updatedBy = null);

        /// <summary>Xóa mềm một sân đấu trong phạm vi quản trị hệ thống.</summary>
        /// <param name="id">ID sân đấu cần xóa mềm.</param>
        /// <returns>True nếu cập nhật xóa mềm thành công.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>Tạo sân đấu khi cụm sân cha thuộc đơn vị hiện tại.</summary>
        /// <param name="dto">Thông tin sân đấu.</param>
        /// <param name="donViId">ID đơn vị hiện tại.</param>
        /// <param name="createdBy">Tên tài khoản tạo dữ liệu.</param>
        /// <returns>Sân đấu vừa tạo.</returns>
        Task<SanDauDto> CreateForDonViAsync(CreateUpdateSanDauDto dto, int donViId, string? createdBy = null);

        /// <summary>Cập nhật sân đấu nếu sân và cụm sân đích cùng thuộc đơn vị hiện tại.</summary>
        /// <param name="id">ID sân đấu cần cập nhật.</param>
        /// <param name="dto">Thông tin mới của sân đấu.</param>
        /// <param name="donViId">ID đơn vị hiện tại.</param>
        /// <param name="updatedBy">Tên tài khoản cập nhật dữ liệu.</param>
        /// <returns>Sân đấu sau cập nhật hoặc null nếu ngoài phạm vi.</returns>
        Task<SanDauDto?> UpdateForDonViAsync(int id, CreateUpdateSanDauDto dto, int donViId, string? updatedBy = null);

        /// <summary>Xóa mềm sân đấu nếu sân thuộc cụm sân của đơn vị hiện tại.</summary>
        /// <param name="id">ID sân đấu cần xóa mềm.</param>
        /// <param name="donViId">ID đơn vị hiện tại.</param>
        /// <returns>True nếu xóa mềm thành công trong phạm vi đơn vị.</returns>
        Task<bool> DeleteForDonViAsync(int id, int donViId);
    }
}
