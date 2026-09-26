using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.DieuHanhMon.Controllers
{
    public class LichThiDauController : BaseDieuHanhMonController
    {
        public LichThiDauController(IDieuHanhMonService dieuHanhService, UserManager<ApplicationUser> userManager)
            : base(dieuHanhService, userManager)
        {
        }

        public async Task<IActionResult> Index(int? giaiDauId, int? danhMucId, int? monTheThaoId, string? status, DateTime? date)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var currentUser = await GetCurrentUserAsync();
            var (selectedGiaiDauId, selectedDanhMucId) = GetSelectedAssignment(giaiDauId, danhMucId, assignments);

            ViewBag.Assignments = assignments;
            ViewBag.CurrentUser = currentUser;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.SelectedDanhMucId = selectedDanhMucId;
            ViewBag.SelectedMonId = monTheThaoId;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");

            if (!selectedGiaiDauId.HasValue || !selectedDanhMucId.HasValue)
            {
                return View(new List<CoordinatorScheduleMatchDto>());
            }

            var currentAssignment = assignments.FirstOrDefault(a => a.GiaiDauId == selectedGiaiDauId.Value && a.DanhMucId == selectedDanhMucId.Value);
            ViewBag.CurrentAssignment = currentAssignment;
            if (currentAssignment == null) return View(new List<CoordinatorScheduleMatchDto>());

            ViewBag.SportList = currentAssignment.MonTheThaos;
            ViewBag.ScheduleTargets = await _dieuHanhService.GetEventsAsync(
                selectedGiaiDauId.Value, selectedDanhMucId.Value, null, assignments);

            var list = await _dieuHanhService.GetScheduleAsync(selectedGiaiDauId.Value, selectedDanhMucId.Value, monTheThaoId, status, date, assignments);

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int tranDauId, string status)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var allowedIds = assignments.SelectMany(a => a.GiaiDauMonTheThaoIds).Distinct().ToList();
            var (success, message) = await _dieuHanhService.UpdateMatchStatusAsync(
                tranDauId, status, allowedIds, User.Identity?.Name ?? "System");
            return Json(new { success = success, message = message });
        }

        [HttpGet]
        public async Task<IActionResult> AvailableCourts(int giaiDauMonTheThaoId)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var allowedIds = assignments.SelectMany(a => a.GiaiDauMonTheThaoIds).Distinct().ToList();
            var courts = await _dieuHanhService.GetAvailableCourtsAsync(giaiDauMonTheThaoId, allowedIds);
            return Json(courts);
        }

        [HttpPost]
        public async Task<IActionResult> PlanSchedule([FromBody] CoordinatorAutoScheduleRequestDto request)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var allowedIds = assignments.SelectMany(a => a.GiaiDauMonTheThaoIds).Distinct().ToList();
            var result = await _dieuHanhService.PlanScheduleAsync(request, allowedIds, User.Identity?.Name ?? "System");
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSchedule([FromBody] CoordinatorUpdateScheduleRequestDto request)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var allowedIds = assignments.SelectMany(a => a.GiaiDauMonTheThaoIds).Distinct().ToList();
            var (success, message) = await _dieuHanhService.UpdateMatchScheduleAsync(
                request, allowedIds, User.Identity?.Name ?? "System");
            return Json(new { success, message });
        }
    }
}
