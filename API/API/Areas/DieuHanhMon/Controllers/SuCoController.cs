using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.DieuHanhMon.Controllers
{
    public class SuCoController : BaseDieuHanhMonController
    {
        public SuCoController(IDieuHanhMonService dieuHanhService, UserManager<ApplicationUser> userManager)
            : base(dieuHanhService, userManager)
        {
        }

        public async Task<IActionResult> Index(int? giaiDauId, int? danhMucId, int? giaiDauMonTheThaoId, string? status)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var currentUser = await GetCurrentUserAsync();
            var (selectedGiaiDauId, selectedDanhMucId) = GetSelectedAssignment(giaiDauId, danhMucId, assignments);
            var allowedIds = assignments.SelectMany(a => a.GiaiDauMonTheThaoIds).Distinct().ToList();

            ViewBag.Assignments = assignments;
            ViewBag.CurrentUser = currentUser;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.SelectedDanhMucId = selectedDanhMucId;
            ViewBag.SelectedEventId = giaiDauMonTheThaoId;
            ViewBag.SelectedStatus = status;
            ViewBag.IssueTargets = new List<CoordinatorEventDto>();
            ViewBag.IssueMatches = new List<CoordinatorScheduleMatchDto>();

            var selectedAssignment = selectedGiaiDauId.HasValue && selectedDanhMucId.HasValue
                ? assignments.FirstOrDefault(a => a.GiaiDauId == selectedGiaiDauId && a.DanhMucId == selectedDanhMucId)
                : null;
            if (selectedAssignment != null)
            {
                ViewBag.IssueTargets = await _dieuHanhService.GetEventsAsync(
                    selectedAssignment.GiaiDauId, selectedAssignment.DanhMucId, null, assignments);
                ViewBag.IssueMatches = await _dieuHanhService.GetScheduleAsync(
                    selectedAssignment.GiaiDauId, selectedAssignment.DanhMucId, null, null, null, assignments);
            }

            var issues = await _dieuHanhService.GetIssuesAsync(allowedIds, giaiDauMonTheThaoId, status);
            return View(issues);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoordinatorIssueRequestDto request)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var allowedIds = assignments.SelectMany(a => a.GiaiDauMonTheThaoIds).Distinct().ToList();
            var (success, message) = await _dieuHanhService.CreateIssueAsync(
                request, allowedIds, User.Identity?.Name ?? "System");
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateCoordinatorIssueRequestDto request)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var allowedIds = assignments.SelectMany(a => a.GiaiDauMonTheThaoIds).Distinct().ToList();
            var (success, message) = await _dieuHanhService.UpdateIssueAsync(
                request, allowedIds, User.Identity?.Name ?? "System");
            return Json(new { success, message });
        }
    }
}
