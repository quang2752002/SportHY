using System.Collections.Generic;

namespace Dms.Application.DTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class PermissionGroupDto
    {
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PermissionItemDto> Permissions { get; set; } = new();
    }

    public class PermissionItemDto
    {
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateRolePermissionsDto
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class CreateUserWithRoleDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public int? DonViId { get; set; }
        public int? TrongTaiId { get; set; }
        public int? ThuKyId { get; set; }
    }

    public class UpdateUserRoleDto
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public int? DonViId { get; set; }
        public int? TrongTaiId { get; set; }
        public int? ThuKyId { get; set; }
    }

    public class UserManagementDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public int? DonViId { get; set; }
        public string? TenDonVi { get; set; }
        public int? TrongTaiId { get; set; }
        public string? TenTrongTai { get; set; }
        public int? ThuKyId { get; set; }
        public string? TenThuKy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class MapSourcesDto
    {
        public List<LookupItemDto> DonVis { get; set; } = new();
        public List<LookupItemDto> TrongTais { get; set; } = new();
        public List<LookupItemDto> ThuKys { get; set; } = new();
    }

    public class LookupItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
    }
}
