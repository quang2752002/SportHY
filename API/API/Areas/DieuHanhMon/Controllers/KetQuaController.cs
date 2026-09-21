using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.DieuHanhMon.Controllers
{
    public class KetQuaController : BaseDieuHanhMonController
    {
        public KetQuaController(IDieuHanhMonService dieuHanhService, UserManager<ApplicationUser> userManager)
            : base(dieuHanhService, userManager)
        {
        }

        public async Task<IActionResult> Index(int? giaiDauId, int? danhMucId, int? monTheThaoId)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var currentReferee = await GetCurrentRefereeAsync();
            var (selectedGiaiDauId, selectedDanhMucId) = GetSelectedAssignment(giaiDauId, danhMucId, assignments);

            ViewBag.Assignments = assignments;
            ViewBag.CurrentReferee = currentReferee;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.SelectedDanhMucId = selectedDanhMucId;
            ViewBag.SelectedMonId = monTheThaoId;

            if (!selectedGiaiDauId.HasValue || !selectedDanhMucId.HasValue)
            {
                return View(new List<CoordinatorResultItemDto>());
            }

            var currentAssignment = assignments.FirstOrDefault(a => a.GiaiDauId == selectedGiaiDauId.Value && a.DanhMucId == selectedDanhMucId.Value);
            ViewBag.CurrentAssignment = currentAssignment;
            if (currentAssignment == null) return View(new List<CoordinatorResultItemDto>());

            ViewBag.SportList = currentAssignment.MonTheThaos;

            var list = await _dieuHanhService.GetResultsAsync(selectedGiaiDauId.Value, selectedDanhMucId.Value, monTheThaoId, assignments);

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveResult(int tranDauId)
        {
            var (success, message) = await _dieuHanhService.ApproveResultAsync(tranDauId);
            return Json(new { success = success, message = message });
        }
    }
}
