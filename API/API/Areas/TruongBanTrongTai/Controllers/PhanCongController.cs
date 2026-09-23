using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.TruongBanTrongTai.Controllers
{
    public class PhanCongController : BaseTruongBanController
    {
        private readonly IMonTheThaoService _monTheThaoService;
        private readonly ITrongTaiService _refereeService;

        public PhanCongController(
            ITruongBanTrongTaiService truongBanService,
            IMonTheThaoService monTheThaoService,
            ITrongTaiService refereeService,
            UserManager<ApplicationUser> userManager)
            : base(truongBanService, userManager)
        {
            _monTheThaoService = monTheThaoService;
            _refereeService = refereeService;
        }

        public async Task<IActionResult> Index(int? giaiDauId, int? monTheThaoId, string? status, DateTime? date, int? trongTaiId)
        {
            var managedTournaments = await GetManagedTournamentsAsync();
            var currentReferee = await GetCurrentRefereeAsync();
            var selectedGiaiDauId = GetSelectedTournamentId(giaiDauId, managedTournaments);

            ViewBag.ManagedTournaments = managedTournaments;
            ViewBag.CurrentReferee = currentReferee;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;
            ViewBag.SelectedMonId = monTheThaoId;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
            ViewBag.SelectedRefereeId = trongTaiId;

            var selectedTournament = selectedGiaiDauId.HasValue
                ? managedTournaments.FirstOrDefault(tournament => tournament.Id == selectedGiaiDauId.Value)
                : null;
            ViewBag.HeadRefereeId = selectedTournament?.TruongBanTrongTaiId;

            if (!selectedGiaiDauId.HasValue)
            {
                ViewBag.Message = "Chưa có giải đấu nào được chọn.";
                return View(new List<MatchAssignmentDto>());
            }

            var assignments = await _truongBanService.GetMatchAssignmentsAsync(selectedGiaiDauId.Value, monTheThaoId, status, date, trongTaiId);

            // Môn thể thao và danh sách trọng tài phục vụ View
            var allMons = await _monTheThaoService.GetAllAsync();
            ViewBag.SportList = allMons.OrderBy(m => m.Ten).ToList();

            var allReferees = await _refereeService.GetAllAsync();
            ViewBag.AllReferees = allReferees
                .Where(referee => referee.TrangThai && referee.Id != selectedTournament?.TruongBanTrongTaiId)
                .OrderBy(referee => referee.HoTen)
                .ToList();

            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> CheckConflict(int trongTaiId, int tranDauId)
        {
            var result = await _truongBanService.CheckConflictAsync(trongTaiId, tranDauId);
            return Json(new
            {
                hasConflict = result.HasConflict,
                isOverloaded = result.IsOverloaded,
                overloadMessage = result.OverloadMessage,
                conflicts = result.Conflicts
            });
        }

        [HttpPost]
        public IActionResult AssignReferee(int tranDauId, string vaiTro, int? trongTaiId, bool force = false)
        {
            return BadRequest(new
            {
                success = false,
                message = "Phân công hiện hoạt động theo cơ chế bản nháp. Hãy cập nhật trên giao diện và bấm Lưu nháp để xác nhận."
            });
        }

        [HttpPost]
        public async Task<IActionResult> AutoAssignReferees([FromBody] AutoAssignRefereesRequestDto request)
        {
            var result = await _truongBanService.AutoAssignRefereesAsync(request);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAssignmentDraft([FromBody] SaveRefereeAssignmentDraftRequestDto request)
        {
            var savedBy = User?.Identity?.Name;
            var result = await _truongBanService.SaveRefereeAssignmentDraftAsync(request, savedBy);
            return Json(result);
        }
    }
}
