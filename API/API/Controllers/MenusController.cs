using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenusController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách menu phẳng (đã sắp xếp theo SortOrder)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var menus = await _menuService.GetAllAsync();
            return Ok(menus);
        }

        /// <summary>
        /// Lấy toàn bộ danh sách menu theo cấu trúc cây Cha - Con
        /// </summary>
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            var tree = await _menuService.GetTreeAsync();
            return Ok(tree);
        }

        /// <summary>
        /// Lấy chi tiết 1 menu theo Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var menu = await _menuService.GetByIdAsync(id);
            if (menu == null)
            {
                return NotFound(new { message = $"Không tìm thấy menu với ID: {id}" });
            }
            return Ok(menu);
        }

        /// <summary>
        /// Tạo menu mới (có thể là menu cha hoặc menu con)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = Permissions.Menus.Create)]
        public async Task<IActionResult> Create([FromBody] MenuDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdMenu = await _menuService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdMenu.Id }, createdMenu);
        }

        /// <summary>
        /// Cập nhật thông tin menu
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = Permissions.Menus.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] MenuDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedMenu = await _menuService.UpdateAsync(id, dto);
            if (updatedMenu == null)
            {
                return NotFound(new { message = $"Không tìm thấy menu với ID: {id}" });
            }

            return Ok(updatedMenu);
        }

        /// <summary>
        /// Xóa menu (kèm theo xử lý menu con)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Menus.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _menuService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Không tìm thấy menu với ID: {id}" });
            }

            return NoContent();
        }
    }
}
