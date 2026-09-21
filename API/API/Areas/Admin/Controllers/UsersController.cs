using Dms.Application.Common;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Areas.Admin.Controllers
{
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public string? TenDonVi { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string Role { get; set; } = AppRoles.Admin;
        public int? DonViId { get; set; }
    }

    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : BaseAdminController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IDonViService _donViService;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IDonViService donViService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _donViService = donViService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Roles = AppRoles.AllRoles;
            ViewBag.DonVis = await _donViService.GetAllAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 10,
            string? keyword = null,
            string? role = null)
        {
            var query = _userManager.Users
                .Include(u => u.DonVi)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(kw) ||
                    u.FullName.ToLower().Contains(kw) ||
                    (u.Email != null && u.Email.ToLower().Contains(kw)));
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<UserListItemDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var userRole = roles.FirstOrDefault() ?? "None";

                if (!string.IsNullOrEmpty(role) && !string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                items.Add(new UserListItemDto
                {
                    Id = u.Id,
                    Username = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    Role = userRole,
                    TenDonVi = u.DonVi?.Ten,
                    CreatedAt = u.CreatedAt
                });
            }

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Json(new
            {
                success = true,
                data = new
                {
                    items = items,
                    totalCount = totalCount,
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalPages = totalPages
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            var roles = await _userManager.GetRolesAsync(user);
            return Json(new
            {
                success = true,
                data = new
                {
                    id = user.Id,
                    username = user.UserName,
                    fullName = user.FullName,
                    email = user.Email,
                    donViId = user.DonViId,
                    role = roles.FirstOrDefault() ?? AppRoles.Admin
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateUserDto dto, [FromQuery] int? id = null)
        {
            if (id.HasValue && id.Value > 0)
            {
                // Cập nhật người dùng hiện có
                var user = await _userManager.FindByIdAsync(id.Value.ToString());
                if (user == null) return Json(new { success = false, message = "Không tìm thấy tài khoản." });

                user.FullName = dto.FullName;
                user.Email = dto.Email;
                user.DonViId = dto.Role == AppRoles.Delegation ? dto.DonViId : null;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return Json(new { success = false, message = string.Join("; ", updateResult.Errors.Select(e => e.Description)) });
                }

                // Cập nhật đổi Role nếu khác
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(dto.Role))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }

                // Đổi mật khẩu nếu có nhập mật khẩu mới
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, dto.Password);
                }

                return Json(new { success = true, message = "Cập nhật tài khoản thành công!" });
            }
            else
            {
                // Thêm mới người dùng
                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return Json(new { success = false, message = "Tên đăng nhập và mật khẩu là bắt buộc." });
                }

                var existingUser = await _userManager.FindByNameAsync(dto.Username);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Tên đăng nhập đã tồn tại trong hệ thống." });
                }

                var user = new ApplicationUser
                {
                    UserName = dto.Username.Trim(),
                    FullName = dto.FullName.Trim(),
                    Email = dto.Email.Trim(),
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    DonViId = dto.Role == AppRoles.Delegation ? dto.DonViId : null
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    return Json(new { success = false, message = string.Join("; ", createResult.Errors.Select(e => e.Description)) });
                }

                // Gán vai trò (Role) trực tiếp cho tài khoản - KHÔNG CẦN PERMISSION NỮA
                await _userManager.AddToRoleAsync(user, dto.Role);

                return Json(new { success = true, message = "Tạo tài khoản người dùng thành công!" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            if (user.UserName?.ToLower() == "admin")
            {
                return Json(new { success = false, message = "Không thể xóa tài khoản Quản trị viên tối cao (admin)." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return Json(new { success = false, message = "Không thể xóa tài khoản này." });
            }

            return Json(new { success = true, message = "Đã xóa tài khoản thành công!" });
        }
    }
}
