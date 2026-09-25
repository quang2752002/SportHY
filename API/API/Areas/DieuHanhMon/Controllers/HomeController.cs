using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.DieuHanhMon.Controllers
{
    public class HomeController : BaseDieuHanhMonController
    {
        public HomeController(IDieuHanhMonService dieuHanhService, UserManager<ApplicationUser> userManager)
            : base(dieuHanhService, userManager)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId, int? danhMucId)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var currentReferee = await GetCurrentRefereeAsync();
            var (selectedGiaiDauId, selectedDanhMucId) = GetSelectedAssignment(giaiDauId, danhMucId, assignments);

            ViewBag.Assignments = assignments;
            ViewBag.CurrentReferee = currentReferee;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.SelectedDanhMucId = selectedDanhMucId;
            return View();
        }

        /// <summary>
        /// API endpoint AJAX lấy dữ liệu Dashboard tổng quan của Người điều hành môn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData(int? giaiDauId, int? danhMucId)
        {
            var assignments = await GetAssignedAssignmentsAsync();
            var (selectedGiaiDauId, selectedDanhMucId) = GetSelectedAssignment(giaiDauId, danhMucId, assignments);

            if (!selectedGiaiDauId.HasValue || !selectedDanhMucId.HasValue)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn chưa được phân công điều hành môn thể thao nào trong các giải đấu."
                });
            }

            var dashboard = await _dieuHanhService.GetDashboardAsync(selectedGiaiDauId.Value, selectedDanhMucId.Value, assignments);

            return Json(new
            {
                success = true,
                data = dashboard
            });
        }
    }
}
