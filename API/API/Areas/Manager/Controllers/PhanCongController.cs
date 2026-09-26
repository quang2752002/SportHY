using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class PhanCongController : BaseManagerController
    {
        private readonly ITruongBanTrongTaiService _truongBanService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly ITrongTaiService _trongTaiService;
        private readonly IThuKyGiaiService _thuKyGiaiService;
        private readonly INguoiDieuHanhMonService _nguoiDieuHanhMonService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhanCongController(
            ITruongBanTrongTaiService truongBanService,
            IGiaiDauService giaiDauService,
            ITrongTaiService trongTaiService,
            IThuKyGiaiService thuKyGiaiService,
            INguoiDieuHanhMonService nguoiDieuHanhMonService,
            UserManager<ApplicationUser> userManager)
        {
            _truongBanService = truongBanService;
            _giaiDauService = giaiDauService;
            _trongTaiService = trongTaiService;
            _thuKyGiaiService = thuKyGiaiService;
            _nguoiDieuHanhMonService = nguoiDieuHanhMonService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId = null)
        {
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if (!selectedGiaiDauId.HasValue && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            var allReferees = (await _trongTaiService.GetAllAsync())?.ToList() ?? new();
            ViewBag.AllReferees = allReferees;
            var sportCoordinatorUsers = await _userManager.GetUsersInRoleAsync(Dms.Application.Common.AppRoles.SportCoordinator);
            ViewBag.SportCoordinatorUsers = await _nguoiDieuHanhMonService.GetProfilesForAccountsAsync(
                sportCoordinatorUsers.Select(user => user.Id).ToList());
            ViewBag.SecretaryAssignments = selectedGiaiDauId.HasValue
                ? await _thuKyGiaiService.GetSecretaryAssignmentsForManagerAsync(selectedGiaiDauId.Value)
                : new List<ThuKyPhanCongDto>();

            GiaiDauDto? currentTournament = null;
            var categoriesAssigned = new List<CategoryAssignmentViewModelDto>();

            if (selectedGiaiDauId.HasValue)
            {
                currentTournament = await _giaiDauService.GetByIdAsync(selectedGiaiDauId.Value);
                categoriesAssigned = await _truongBanService.GetCategoryAssignmentsForManagerAsync(selectedGiaiDauId.Value);
            }

            ViewBag.CurrentTournament = currentTournament;
            return View(categoriesAssigned);
        }

        [HttpPost]
        public async Task<IActionResult> AssignHeadReferee(int giaiDauId, int? trongTaiId)
        {
            var (success, message) = await _truongBanService.AssignHeadRefereeForManagerAsync(giaiDauId, trongTaiId);
            if (!success)
            {
                return Json(new { success = false, message = message });
            }

            // Đồng bộ cấp Role HeadReferee cho tài khoản user tương ứng nếu có
            if (trongTaiId.HasValue)
            {
                var refList = await _trongTaiService.GetAllAsync();
                var refEntity = refList.FirstOrDefault(t => t.Id == trongTaiId.Value);
                if (refEntity != null)
                {
                    // Tim user theo TrongTaiId hoac Ma hoac Email
                    var users = _userManager.Users.Where(u =>
                        u.TrongTaiId == trongTaiId.Value ||
                        u.UserName == refEntity.Ma ||
                        u.Email == refEntity.Email).ToList();
                    foreach (var u in users)
                    {
                        // Gan TrongTaiId neu chua duoc lien ket
                        if (u.TrongTaiId != trongTaiId.Value)
                        {
                            u.TrongTaiId = trongTaiId.Value;
                            await _userManager.UpdateAsync(u);
                        }

                        if (!await _userManager.IsInRoleAsync(u, Dms.Application.Common.AppRoles.HeadReferee))
                        {
                            await _userManager.AddToRoleAsync(u, Dms.Application.Common.AppRoles.HeadReferee);
                        }
                    }
                }
            }

            return Json(new { success = true, message = message });
        }

        [HttpPost]
        public async Task<IActionResult> AssignSportCoordinator(int giaiDauId, int danhMucId, int? nguoiDieuHanhMonId)
        {
            var coordinatorUsers = await _userManager.GetUsersInRoleAsync(Dms.Application.Common.AppRoles.SportCoordinator);
            var coordinatorProfiles = await _nguoiDieuHanhMonService.GetProfilesForAccountsAsync(
                coordinatorUsers.Select(user => user.Id).ToList());
            var (success, message) = await _truongBanService.AssignSportCoordinatorForManagerAsync(
                giaiDauId,
                danhMucId,
                nguoiDieuHanhMonId,
                coordinatorProfiles.Select(profile => profile.Id).ToList(),
                User.Identity?.Name);

            return Json(new { success, message });
        }

        /// <summary>
        /// Lưu danh sách thư ký được phân công cho giải đấu đang chọn.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu.</param>
        /// <param name="thuKyIds">Danh sách hồ sơ thư ký được chọn.</param>
        /// <returns>Kết quả thao tác dưới dạng JSON.</returns>
        [HttpPost]
        public async Task<IActionResult> AssignSecretaries(int giaiDauId, [FromBody] List<int>? thuKyIds)
        {
            var (success, message) = await _thuKyGiaiService.SaveSecretaryAssignmentsAsync(
                giaiDauId,
                thuKyIds ?? new List<int>());

            return Json(new { success, message });
        }
    }
}
