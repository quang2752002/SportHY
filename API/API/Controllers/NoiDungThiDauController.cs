using Dms.Application.DTOs;
using Dms.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoiDungThiDauController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public NoiDungThiDauController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy danh sách nội dung thi đấu.
        /// Lọc theo giaiDauId (qua GiaiDauMonTheThao) hoặc giaiDauMonTheThaoId trực tiếp.
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? giaiDauId = null,
            [FromQuery] int? giaiDauMonTheThaoId = null)
        {
            var paged = await _unitOfWork.NoiDungThiDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 500,
                predicate: n =>
                    n.IsDeleted != true &&
                    n.TrangThai == true &&
                    (!giaiDauMonTheThaoId.HasValue || n.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value),
                orderBy: q => q.OrderBy(n => n.GiaiDauMonTheThaoId).ThenBy(n => n.Ten),
                n => n.GiaiDauMonTheThao,
                n => n.GiaiDauMonTheThao.GiaiDau,
                n => n.GiaiDauMonTheThao.MonTheThao
            );

            var items = paged.Items.AsEnumerable();

            // Lọc thêm theo giải đấu
            if (giaiDauId.HasValue)
            {
                items = items.Where(n => n.GiaiDauMonTheThao?.GiaiDauId == giaiDauId.Value);
            }

            var dtos = items.Select(n => new NoiDungThiDauDto
            {
                Id = n.Id,
                GiaiDauMonTheThaoId = n.GiaiDauMonTheThaoId,
                GiaiDauId = n.GiaiDauMonTheThao != null ? n.GiaiDauMonTheThao.GiaiDauId : 0,
                MonTheThaoId = n.GiaiDauMonTheThao != null ? n.GiaiDauMonTheThao.MonTheThaoId : 0,
                TenMonTheThao = n.GiaiDauMonTheThao?.MonTheThao?.Ten,
                TenGiaiDau = n.GiaiDauMonTheThao?.GiaiDau?.Ten,
                Ma = n.Ma,
                Ten = n.Ten,
                GioiTinh = n.GioiTinh,
                LoaiThiDau = n.LoaiThiDau,
                HinhThucThiDau = n.HinhThucThiDau.HasValue
                    ? n.HinhThucThiDau.Value.ToString()
                    : (n.GiaiDauMonTheThao?.MonTheThao != null ? n.GiaiDauMonTheThao.MonTheThao.HinhThucThiDau.ToString() : null),
                SoLuongToiThieu = n.SoLuongToiThieu,
                SoLuongToiDa = n.SoLuongToiDa,
                MoTa = n.MoTa,
                TrangThai = n.TrangThai,
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var paged = await _unitOfWork.NoiDungThiDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: n => n.Id == id && n.IsDeleted != true,
                orderBy: null,
                n => n.GiaiDauMonTheThao,
                n => n.GiaiDauMonTheThao.GiaiDau,
                n => n.GiaiDauMonTheThao.MonTheThao
            );

            var n = paged.Items.FirstOrDefault();
            if (n == null) return NotFound(new { message = "Không tìm thấy nội dung thi đấu." });

            return Ok(new NoiDungThiDauDto
            {
                Id = n.Id,
                GiaiDauMonTheThaoId = n.GiaiDauMonTheThaoId,
                GiaiDauId = n.GiaiDauMonTheThao != null ? n.GiaiDauMonTheThao.GiaiDauId : 0,
                MonTheThaoId = n.GiaiDauMonTheThao != null ? n.GiaiDauMonTheThao.MonTheThaoId : 0,
                TenMonTheThao = n.GiaiDauMonTheThao?.MonTheThao?.Ten,
                TenGiaiDau = n.GiaiDauMonTheThao?.GiaiDau?.Ten,
                Ma = n.Ma,
                Ten = n.Ten,
                GioiTinh = n.GioiTinh,
                LoaiThiDau = n.LoaiThiDau,
                HinhThucThiDau = n.HinhThucThiDau.HasValue
                    ? n.HinhThucThiDau.Value.ToString()
                    : (n.GiaiDauMonTheThao?.MonTheThao != null ? n.GiaiDauMonTheThao.MonTheThao.HinhThucThiDau.ToString() : null),
                SoLuongToiThieu = n.SoLuongToiThieu,
                SoLuongToiDa = n.SoLuongToiDa,
                MoTa = n.MoTa,
                TrangThai = n.TrangThai,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateUpdateNoiDungThiDauDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ten))
            {
                return BadRequest(new { message = "Tên nội dung thi đấu không được để trống." });
            }

            int gdmId = dto.GiaiDauMonTheThaoId;
            if (gdmId <= 0 && dto.GiaiDauId.HasValue && dto.MonTheThaoId.HasValue)
            {
                var existingGdm = (await _unitOfWork.GiaiDauMonTheThaos.FindAsync(
                    g => g.GiaiDauId == dto.GiaiDauId.Value && g.MonTheThaoId == dto.MonTheThaoId.Value && g.IsDeleted != true
                )).FirstOrDefault();

                if (existingGdm != null)
                {
                    gdmId = existingGdm.Id;
                }
                else
                {
                    var newGdm = new Dms.Domain.Entities.GiaiDauMonTheThao
                    {
                        GiaiDauId = dto.GiaiDauId.Value,
                        MonTheThaoId = dto.MonTheThaoId.Value,
                        TrangThai = true,
                        Created = System.DateTime.UtcNow
                    };
                    await _unitOfWork.GiaiDauMonTheThaos.AddAsync(newGdm);
                    await _unitOfWork.CompleteAsync();
                    gdmId = newGdm.Id;
                }
            }

            if (gdmId <= 0)
            {
                return BadRequest(new { message = "Vui lòng chọn môn thi đấu và giải đấu hợp lệ." });
            }

            Dms.Domain.Enums.HinhThucThiDau? resolvedHinhThuc = null;
            if (!string.IsNullOrWhiteSpace(dto.HinhThucThiDau) &&
                System.Enum.TryParse<Dms.Domain.Enums.HinhThucThiDau>(dto.HinhThucThiDau, true, out var parsedHt))
            {
                resolvedHinhThuc = parsedHt;
            }
            else
            {
                // Nếu NoiDungThiDau ko chọn, mặc định lấy theo MonTheThao
                var gdmList = await _unitOfWork.GiaiDauMonTheThaos.FindAsync(x => x.Id == gdmId);
                var gdm = gdmList.FirstOrDefault();
                if (gdm != null)
                {
                    var monList = await _unitOfWork.MonTheThaos.FindAsync(x => x.Id == gdm.MonTheThaoId);
                    var mon = monList.FirstOrDefault();
                    if (mon != null)
                    {
                        resolvedHinhThuc = mon.HinhThucThiDau;
                    }
                }
            }

            var entity = new Dms.Domain.Entities.NoiDungThiDau
            {
                GiaiDauMonTheThaoId = gdmId,
                Ma = string.IsNullOrWhiteSpace(dto.Ma) ? $"ND_{System.Guid.NewGuid():N}".Substring(0, 10).ToUpper() : dto.Ma,
                Ten = dto.Ten.Trim(),
                GioiTinh = string.IsNullOrWhiteSpace(dto.GioiTinh) ? "HonHop" : dto.GioiTinh,
                LoaiThiDau = string.IsNullOrWhiteSpace(dto.LoaiThiDau) ? "CaNhan" : dto.LoaiThiDau,
                HinhThucThiDau = resolvedHinhThuc,
                SoLuongToiThieu = dto.SoLuongToiThieu ?? (dto.LoaiThiDau == "DongDoi" ? 2 : 1),
                SoLuongToiDa = dto.SoLuongToiDa ?? (dto.LoaiThiDau == "DongDoi" ? 20 : 1),
                MoTa = dto.MoTa,
                TrangThai = dto.TrangThai,
                Created = System.DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.NoiDungThiDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return await GetById(entity.Id);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateNoiDungThiDauDto dto)
        {
            var paged = await _unitOfWork.NoiDungThiDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: n => n.Id == id && n.IsDeleted != true
            );
            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return NotFound(new { message = "Không tìm thấy nội dung thi đấu để cập nhật." });

            if (!string.IsNullOrWhiteSpace(dto.Ma)) entity.Ma = dto.Ma;
            if (!string.IsNullOrWhiteSpace(dto.Ten)) entity.Ten = dto.Ten.Trim();
            if (!string.IsNullOrWhiteSpace(dto.GioiTinh)) entity.GioiTinh = dto.GioiTinh;
            if (!string.IsNullOrWhiteSpace(dto.LoaiThiDau)) entity.LoaiThiDau = dto.LoaiThiDau;
            if (!string.IsNullOrWhiteSpace(dto.HinhThucThiDau) &&
                System.Enum.TryParse<Dms.Domain.Enums.HinhThucThiDau>(dto.HinhThucThiDau, true, out var updatedHt))
            {
                entity.HinhThucThiDau = updatedHt;
            }
            entity.SoLuongToiThieu = dto.SoLuongToiThieu;
            entity.SoLuongToiDa = dto.SoLuongToiDa;
            entity.MoTa = dto.MoTa;
            entity.TrangThai = dto.TrangThai;
            entity.LastModified = System.DateTime.UtcNow;

            _unitOfWork.NoiDungThiDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return await GetById(id);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var paged = await _unitOfWork.NoiDungThiDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: n => n.Id == id && n.IsDeleted != true
            );
            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return NotFound(new { message = "Không tìm thấy nội dung thi đấu để xóa." });

            entity.IsDeleted = true;
            entity.LastModified = System.DateTime.UtcNow;
            _unitOfWork.NoiDungThiDaus.Update(entity);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Đã xóa nội dung thi đấu thành công." });
        }
    }
}
