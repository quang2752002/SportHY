using Dms.Application.Common;
using Dms.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Permissions.Users.ManageRoles)]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<Dms.Domain.Entities.ApplicationUser> _userManager;
        private readonly Dms.Infrastructure.Persistence.ApplicationDbContext _dbContext;

        public RolesController(
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<Dms.Domain.Entities.ApplicationUser> userManager,
            Dms.Infrastructure.Persistence.ApplicationDbContext dbContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách Role kèm danh sách Permissions đã được cấp
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var result = new List<RoleDto>();

            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
                result.Add(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Permissions = permissions
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách cây phân nhóm toàn bộ các quyền trong hệ thống để hiển thị Checkbox và cấu hình UI
        /// </summary>
        [HttpGet("permissions-tree")]
        [Authorize]
        public IActionResult GetPermissionsTree()
        {
            var permissionNestedTypes = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
            var groups = new List<PermissionGroupDto>();

            foreach (var type in permissionNestedTypes)
            {
                var groupName = type.Name;
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                 .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

                var items = fields.Select(f => new PermissionItemDto
                {
                    Name = f.Name,
                    Value = (string)f.GetValue(null)!,
                    Description = GetPermissionDescription(groupName, f.Name)
                }).ToList();

                groups.Add(new PermissionGroupDto
                {
                    GroupName = groupName,
                    Description = GetGroupDescription(groupName),
                    Permissions = items
                });
            }

            return Ok(groups);
        }

        /// <summary>
        /// Cập nhật quyền Permissions cho 1 Role cụ thể bằng danh sách checkbox
        /// </summary>
        [HttpPost("update-permissions")]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRolePermissionsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoleName))
            {
                return BadRequest(new { message = "Tên vai trò không được để trống." });
            }

            var role = await _roleManager.FindByNameAsync(dto.RoleName);
            if (role == null)
            {
                return NotFound(new { message = $"Không tìm thấy vai trò '{dto.RoleName}'." });
            }

            // Không cho phép tước quyền của Super Admin (Admin luôn full quyền)
            if (dto.RoleName == AppRoles.Admin)
            {
                return BadRequest(new { message = "Vai trò Quản trị viên (Admin) luôn sở hữu toàn bộ quyền hệ thống, không được sửa đổi." });
            }

            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = existingClaims.Where(c => c.Type == "permission").ToList();

            // Xóa các quyền hiện tại
            foreach (var claim in permissionClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // Gán các quyền mới được chọn qua checkbox
            if (dto.Permissions != null && dto.Permissions.Count > 0)
            {
                var distinctPerms = dto.Permissions.Distinct();
                foreach (var perm in distinctPerms)
                {
                    await _roleManager.AddClaimAsync(role, new Claim("permission", perm));
                }
            }

            return Ok(new { message = $"Đã cập nhật quyền thành công cho vai trò '{dto.RoleName}'." });
        }

        /// <summary>
        /// Lấy danh sách nguồn ánh xạ cho tài khoản: Đơn vị (Đoàn) và Trọng tài
        /// </summary>
        [HttpGet("map-sources")]
        [Authorize]
        public async Task<IActionResult> GetMapSources()
        {
            var donVis = await _dbContext.DonVis
                .AsNoTracking()
                .OrderBy(d => d.Ten)
                .Select(d => new LookupItemDto
                {
                    Id = d.Id,
                    Name = d.Ten,
                    Code = d.Ma
                })
                .ToListAsync();

            var trongTais = await _dbContext.TrongTais
                .AsNoTracking()
                .OrderBy(t => t.HoTen)
                .Select(t => new LookupItemDto
                {
                    Id = t.Id,
                    Name = t.HoTen,
                    Code = t.Ma
                })
                .ToListAsync();

            var thuKys = await _dbContext.ThuKys
                .AsNoTracking()
                .OrderBy(k => k.HoTen)
                .Select(k => new LookupItemDto
                {
                    Id = k.Id,
                    Name = k.HoTen,
                    Code = k.Ma
                })
                .ToListAsync();

            return Ok(new MapSourcesDto
            {
                DonVis = donVis,
                TrongTais = trongTais,
                ThuKys = thuKys
            });
        }

        /// <summary>
        /// Lấy danh sách toàn bộ tài khoản người dùng kèm vai trò
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users
                .Include(u => u.DonVi)
                .Include(u => u.TrongTai)
                .Include(u => u.ThuKy)
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            var list = new List<UserManagementDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserManagementDto
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    FullName = u.FullName ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    Roles = roles.ToList(),
                    DonViId = u.DonViId,
                    TenDonVi = u.DonVi?.Ten,
                    TrongTaiId = u.TrongTaiId,
                    TenTrongTai = u.TrongTai?.HoTen,
                    ThuKyId = u.ThuKyId,
                    TenThuKy = u.ThuKy?.HoTen,
                    CreatedAt = u.CreatedAt
                });
            }

            return Ok(list);
        }

        /// <summary>
        /// Tạo mới tài khoản và gán trực tiếp vai trò (Role)
        /// </summary>
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUserWithRole([FromBody] CreateUserWithRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Tên đăng nhập và mật khẩu không được để trống." });
            }

            var existing = await _userManager.FindByNameAsync(dto.Username);
            if (existing != null)
            {
                return BadRequest(new { message = $"Tên đăng nhập '{dto.Username}' đã tồn tại." });
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new { message = $"Email '{dto.Email}' đã được sử dụng." });
                }
            }

            var user = new Dms.Domain.Entities.ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                FullName = string.IsNullOrWhiteSpace(dto.FullName) ? dto.Username : dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                DonViId = dto.DonViId,
                TrongTaiId = dto.TrongTaiId,
                ThuKyId = dto.ThuKyId,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createRes = await _userManager.CreateAsync(user, dto.Password);
            if (!createRes.Succeeded)
            {
                var errors = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            // Gán Role nếu được chỉ định
            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }
            }

            return Ok(new { message = $"Đã tạo tài khoản '{dto.Username}' và phân vai trò thành công!" });
        }

        /// <summary>
        /// Cập nhật / Đổi vai trò (Role) cho tài khoản người dùng
        /// </summary>
        [HttpPost("update-user-role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Cập nhật liên kết Đơn vị, Trọng tài, hoặc Thư ký
            user.DonViId = dto.DonViId;
            user.TrongTaiId = dto.TrongTaiId;
            user.ThuKyId = dto.ThuKyId;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrWhiteSpace(dto.Role))
            {
                if (await _roleManager.RoleExistsAsync(dto.Role))
                {
                    await _userManager.AddToRoleAsync(user, dto.Role);
                }
            }

            return Ok(new { message = $"Đã cập nhật vai trò mới cho tài khoản '{user.UserName}'." });
        }

        private static string GetGroupDescription(string groupName)
        {
            return groupName switch
            {
                "System" => "Cấu hình hệ thống, tham số và sao lưu dữ liệu",
                "Tournaments" => "Quản lý tạo mới, sửa, xóa và chỉ định quản lý giải đấu",
                "Sports" => "Quản lý môn thể thao, luật thời gian và hiệp đấu",
                "TournamentSports" => "Gán môn thể thao vào các giải đấu",
                "Groups" => "Quản lý bảng thi đấu (Bảng A, B, C...)",
                "Teams" => "Quản lý danh sách đội tuyển, câu lạc bộ thi đấu",
                "Athletes" => "Quản lý hồ sơ vận động viên, số áo và vị trí",
                "Matches" => "Quản lý lịch thi đấu, sân bãi và nhập kết quả trận",
                "Referees" => "Phân công và điều phối giám sát trọng tài",
                "Results" => "Xác nhận biên bản và xuất báo cáo PDF/Excel",
                "Delegations" => "Quản lý đoàn thể thao trực thuộc các đơn vị",
                "Categories" => "Quản lý danh mục, nhóm môn thể thao",
                "Users" => "Quản trị người dùng và phân quyền vai trò",
                "Menus" => "Quản lý hệ thống menu điều hướng",
                "ThuKy" => "Quản lý thông tin và hồ sơ thư ký bàn, thư ký giải",
                _ => $"Quản lý quyền {groupName}"
            };
        }

        private static string GetPermissionDescription(string groupName, string actionName)
        {
            return actionName switch
            {
                "View" => "Xem danh sách và thông tin chi tiết",
                "Create" => "Tạo mới bản ghi dữ liệu",
                "Edit" => "Chỉnh sửa, cập nhật dữ liệu",
                "Delete" => "Xóa dữ liệu khỏi hệ thống",
                "UpdateScore" => "Cập nhật tỉ số, diễn biến và kết quả trận đấu",
                "Assign" => "Phân công trọng tài chính, trọng tài phụ cho trận",
                "Supervise" => "Giám sát tiến độ thi đấu",
                "VerifyReport" => "Kiểm duyệt biên bản thi đấu sau trận",
                "ExportReport" => "Xuất báo cáo, biên bản kết quả (PDF/Excel)",
                "ManageAthletes" => "Quản lý & đăng ký danh sách VĐV của đoàn",
                "ViewTeams" => "Xem danh sách đội thi đấu",
                "AssignManager" => "Chỉ định thành viên điều hành giải đấu",
                "ManageRoles" => "Phân quyền và quản lý vai trò người dùng",
                _ => $"{actionName} {groupName}"
            };
        }
    }
}
