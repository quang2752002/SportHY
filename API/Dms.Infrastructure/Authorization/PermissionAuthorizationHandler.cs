using Microsoft.AspNetCore.Authorization;

namespace Dms.Infrastructure.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            // 1. Quản trị viên (Admin) và Quản lý (Manager) có toàn quyền trên mọi policy
            if (context.User.IsInRole(Dms.Application.Common.AppRoles.Admin) || 
                context.User.IsInRole(Dms.Application.Common.AppRoles.Manager))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 2. Quyền xóa VĐV: Cho phép vai trò Đoàn thể thao hoặc có quyền Quản lý VĐV
            if (requirement.Permission == Dms.Application.Common.Permissions.VanDongVien.Delete)
            {
                if (context.User.IsInRole(Dms.Application.Common.AppRoles.Delegation) ||
                    context.User.Claims.Any(c => c.Type == "permission" &&
                        (c.Value == Dms.Application.Common.Permissions.DonVi.ManageAthletes ||
                         c.Value == Dms.Application.Common.Permissions.VanDongVien.Delete)))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            // 3. Quyền xóa / sửa đăng ký thi đấu: Cho phép vai trò Đoàn thể thao
            if (requirement.Permission == Dms.Application.Common.Permissions.DangKyThiDau.Delete ||
                requirement.Permission == Dms.Application.Common.Permissions.DangKyThiDau.Edit)
            {
                if (context.User.IsInRole(Dms.Application.Common.AppRoles.Delegation))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            // 4. Kiểm tra xem User có Claim type "permission" khớp với requirement.Permission hay không
            var hasPermission = context.User.Claims.Any(c =>
                c.Type == "permission" &&
                string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

            if (hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
