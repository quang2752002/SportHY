using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.Manager.Controllers
{
    public class LichThiDauController : BaseManagerController
    {
        private readonly ITranDauService _tranDauService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly ISanDauService _sanDauService;
        private readonly IMonTheThaoService _monTheThaoService;
        private readonly IVongDauService _vongDauService;
        private readonly IBangDauService _bangDauService;
        private readonly IDanhMucMonTheThaoService _danhMucMonTheThaoService;

        public LichThiDauController(
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            ISanDauService sanDauService,
            IMonTheThaoService monTheThaoService,
            IVongDauService vongDauService,
            IBangDauService bangDauService,
            IDanhMucMonTheThaoService danhMucMonTheThaoService)
        {
            _tranDauService = tranDauService;
            _giaiDauService = giaiDauService;
            _sanDauService = sanDauService;
            _monTheThaoService = monTheThaoService;
            _vongDauService = vongDauService;
            _bangDauService = bangDauService;
            _danhMucMonTheThaoService = danhMucMonTheThaoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int? giaiDauId = null)
        {
            ViewBag.GiaiDaus = await _giaiDauService.GetAllAsync();
            ViewBag.DanhMucMonTheThaos = await _danhMucMonTheThaoService.GetAllAsync();
            ViewBag.MonTheThaos = await _monTheThaoService.GetAllAsync();
            ViewBag.SanDaus = await _sanDauService.GetAllAsync();
            ViewBag.SelectedGiaiDauId = giaiDauId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDanhMucs()
        {
            var list = await _danhMucMonTheThaoService.GetAllAsync();
            return Json(new { success = true, data = list });
        }

        [HttpGet]
        public async Task<IActionResult> GetPagedData(
            int pageIndex = 1,
            int pageSize = 20,
            string? keyword = null,
            int? giaiDauId = null,
            int? danhMucMonTheThaoId = null,
            int? giaiDauMonTheThaoId = null,
            int? vongDauId = null,
            int? bangDauId = null,
            int? sanDauId = null,
            DateTime? ngay = null,
            string? trangThai = null)
        {
            var result = await _tranDauService.GetPagedAsync(
                pageIndex, pageSize, keyword, giaiDauId, giaiDauMonTheThaoId, vongDauId, bangDauId, sanDauId, ngay, trangThai, danhMucMonTheThaoId);
            return Json(new { success = true, data = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _tranDauService.GetByIdAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy trận đấu." });
            return Json(new { success = true, data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CreateUpdateTranDauDto dto, [FromQuery] int? id = null)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var updated = await _tranDauService.UpdateAsync(id.Value, dto, username);
                    if (updated == null) return Json(new { success = false, message = "Không tìm thấy trận đấu để cập nhật." });
                    return Json(new { success = true, message = "Cập nhật trận đấu thành công!", data = updated });
                }
                else
                {
                    var created = await _tranDauService.CreateAsync(dto, username);
                    return Json(new { success = true, message = "Tạo trận đấu mới thành công!", data = created });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AutoSchedule([FromBody] AutoScheduleRequestDto request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                var result = await _tranDauService.AutoScheduleAsync(request, username);
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message ?? "Xếp lịch tự động không thành công." });
                }
                return Json(new { success = true, message = result.Message, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _tranDauService.DeleteAsync(id);
            if (!success) return Json(new { success = false, message = "Không tìm thấy trận đấu để xóa." });
            return Json(new { success = true, message = "Đã xóa trận đấu thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ClearSchedule(int giaiDauMonTheThaoId)
        {
            await _tranDauService.ClearByGiaiDauMonTheThaoAsync(giaiDauMonTheThaoId);
            return Json(new { success = true, message = "Đã xóa toàn bộ lịch thi đấu của môn này để xếp lại!" });
        }

        [HttpGet]
        public async Task<IActionResult> CheckConflicts(int giaiDauId)
        {
            var report = await _tranDauService.CheckAllConflictsAsync(giaiDauId);
            return Json(new { success = true, data = report });
        }

        [HttpGet]
        public async Task<IActionResult> GetMonByGiaiDau(int giaiDauId)
        {
            var giaiDau = await _giaiDauService.GetByIdAsync(giaiDauId);
            if (giaiDau == null) return Json(new { success = false, data = new List<object>() });
            return Json(new { success = true, data = giaiDau.MonTheThaos });
        }

        [HttpGet]
        public async Task<IActionResult> GetManualPairingData(int giaiDauMonTheThaoId, int? vongDauId = null, int? bangDauId = null)
        {
            try
            {
                var data = await _tranDauService.GetManualPairingDataAsync(giaiDauMonTheThaoId, vongDauId, bangDauId);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveManualPairing([FromBody] SaveManualPairingRequestDto request)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Manager";

            try
            {
                var result = await _tranDauService.SaveManualPairingAsync(request, username);
                return Json(new { success = true, message = "Đã lưu xếp cặp thi đấu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
