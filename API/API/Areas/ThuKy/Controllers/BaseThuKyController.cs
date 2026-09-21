using Dms.Application.Common;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Areas.ThuKy.Controllers
{
    /// <summary>
    /// Base Controller cho phân hệ Thư ký giải (ThuKy).
    /// Áp dụng quyền truy cập: Thư ký (Secretary), Quản trị viên (Admin), Quản lý giải (Manager).
    /// </summary>
    [Area("ThuKy")]
    [Authorize(Roles = AppRoles.Secretary + "," + AppRoles.Admin + "," + AppRoles.Manager)]
    public abstract class BaseThuKyController : Controller
    {
        protected readonly IThuKyGiaiService _thuKyService;
        protected readonly IGiaiDauService _giaiDauService;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseThuKyController(
            IThuKyGiaiService thuKyService,
            IGiaiDauService giaiDauService,
            UserManager<ApplicationUser> userManager)
        {
            _thuKyService = thuKyService;
            _giaiDauService = giaiDauService;
            _userManager = userManager;
        }

        /// <summary>
        /// Lấy danh sách tất cả các giải đấu đang quản lý.
        /// </summary>
        protected async Task<List<GiaiDauDto>> GetAllTournamentsAsync()
        {
            var list = await _giaiDauService.GetAllAsync();
            return list?.ToList() ?? new List<GiaiDauDto>();
        }

        /// <summary>
        /// Xác định giải đấu được chọn dựa trên tham số query string, cookie hoặc mặc định giải mới nhất.
        /// </summary>
        protected async Task<int?> GetSelectedGiaiDauIdAsync(int? reqGiaiDauId = null)
        {
            var tournaments = await GetAllTournamentsAsync();
            if (tournaments.Count == 0) return null;

            if (reqGiaiDauId.HasValue && tournaments.Any(t => t.Id == reqGiaiDauId.Value))
            {
                SaveSelectedTournamentCookie(reqGiaiDauId.Value);
                return reqGiaiDauId.Value;
            }

            if (Request.Cookies.TryGetValue("ThuKy_SelectedGiaiDauId", out var cVal) && int.TryParse(cVal, out int cId))
            {
                if (tournaments.Any(t => t.Id == cId))
                {
                    return cId;
                }
            }

            var first = tournaments.OrderByDescending(t => t.NgayBatDau).FirstOrDefault();
            if (first != null)
            {
                SaveSelectedTournamentCookie(first.Id);
                return first.Id;
            }

            return null;
        }

        /// <summary>
        /// Lưu cookie ghi nhớ giải đấu Thư ký đang làm việc.
        /// </summary>
        protected void SaveSelectedTournamentCookie(int giaiDauId)
        {
            Response.Cookies.Append("ThuKy_SelectedGiaiDauId", giaiDauId.ToString(), new Microsoft.AspNetCore.Http.CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
            });
        }

        /// <summary>
        /// Lấy tên người dùng hiện tại phục vụ ghi nhật ký ký duyệt.
        /// </summary>
        protected string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "Thư ký giải";
        }
    }
}
