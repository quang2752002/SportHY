using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.DieuHanhMon.Controllers
{
    [Area("DieuHanhMon")]
    [Authorize(Roles = "SportCoordinator")]
    public abstract class BaseDieuHanhMonController : Controller
    {
        protected readonly IDieuHanhMonService _dieuHanhService;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseDieuHanhMonController(IDieuHanhMonService dieuHanhService, UserManager<ApplicationUser> userManager)
        {
            _dieuHanhService = dieuHanhService;
            _userManager = userManager;
        }

        protected async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        protected async Task<List<CoordinatorAssignmentDto>> GetAssignedAssignmentsAsync()
        {
            bool isAdminOrManager = User.IsInRole(AppRoles.Admin) || User.IsInRole(AppRoles.Manager);
            var currentUser = await GetCurrentUserAsync();

            return await _dieuHanhService.GetAssignedDisciplinesAsync(currentUser?.Id, isAdminOrManager);
        }

        protected (int? giaiDauId, int? danhMucId) GetSelectedAssignment(int? reqGiaiDauId, int? reqDanhMucId, List<CoordinatorAssignmentDto> assignments)
        {
            if (reqGiaiDauId.HasValue && reqDanhMucId.HasValue)
            {
                if (assignments.Any(a => a.GiaiDauId == reqGiaiDauId.Value && a.DanhMucId == reqDanhMucId.Value))
                {
                    SaveSelectedCookie(reqGiaiDauId.Value, reqDanhMucId.Value);
                    return (reqGiaiDauId.Value, reqDanhMucId.Value);
                }
            }

            if (Request.Cookies.TryGetValue("DieuHanh_SelectedGiaiDauId", out var cGiai) && int.TryParse(cGiai, out int gId) &&
                Request.Cookies.TryGetValue("DieuHanh_SelectedDanhMucId", out var cDm) && int.TryParse(cDm, out int dmId))
            {
                if (assignments.Any(a => a.GiaiDauId == gId && a.DanhMucId == dmId))
                {
                    return (gId, dmId);
                }
            }

            var first = assignments.FirstOrDefault();
            if (first != null)
            {
                SaveSelectedCookie(first.GiaiDauId, first.DanhMucId);
                return (first.GiaiDauId, first.DanhMucId);
            }

            return (null, null);
        }

        private void SaveSelectedCookie(int giaiDauId, int danhMucId)
        {
            Response.Cookies.Append("DieuHanh_SelectedGiaiDauId", giaiDauId.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = true
            });
            Response.Cookies.Append("DieuHanh_SelectedDanhMucId", danhMucId.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = true
            });
        }
    }
}
