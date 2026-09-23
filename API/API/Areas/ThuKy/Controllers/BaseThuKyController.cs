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

        /// <summary>
        /// Xác định tài khoản chỉ có phạm vi Thư ký, không đồng thời có quyền quản trị hoặc quản lý giải.
        /// </summary>
        private bool IsRestrictedSecretary =>
            User.IsInRole(AppRoles.Secretary) &&
            !User.IsInRole(AppRoles.Admin) &&
            !User.IsInRole(AppRoles.Manager);

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
            if (IsRestrictedSecretary)
            {
                var secretaryUser = await _userManager.GetUserAsync(User);
                if (secretaryUser?.ThuKyId is not int thuKyId)
                {
                    return new List<GiaiDauDto>();
                }

                return await _thuKyService.GetAssignedTournamentsAsync(thuKyId);
            }

            var list = await _giaiDauService.GetAllAsync();
            return list?.ToList() ?? new List<GiaiDauDto>();
        }

        /// <summary>
        /// Kiểm tra quyền truy cập của tài khoản hiện tại đối với một giải đấu.
        /// Admin và Manager được giữ quyền xem toàn bộ giải; Secretary chỉ được xem giải đã phân công.
        /// </summary>
        /// <param name="giaiDauId">ID giải đấu cần kiểm tra.</param>
        /// <returns>True nếu tài khoản hiện tại được phép truy cập.</returns>
        protected async Task<bool> CanAccessTournamentAsync(int? giaiDauId)
        {
            if (!giaiDauId.HasValue || giaiDauId.Value <= 0)
            {
                return false;
            }

            if (!IsRestrictedSecretary)
            {
                return true;
            }

            var secretaryUser = await _userManager.GetUserAsync(User);
            return secretaryUser?.ThuKyId is int thuKyId &&
                   await _thuKyService.IsSecretaryAssignedAsync(thuKyId, giaiDauId.Value);
        }

        /// <summary>
        /// Kiểm tra quyền truy cập của tài khoản hiện tại đối với một trận đấu.
        /// </summary>
        /// <param name="tranDauId">ID trận đấu cần kiểm tra.</param>
        /// <returns>True nếu trận đấu thuộc giải được phép truy cập.</returns>
        protected async Task<bool> CanAccessMatchAsync(int tranDauId)
        {
            if (!IsRestrictedSecretary)
            {
                return true;
            }

            var giaiDauId = await _thuKyService.GetTournamentIdByMatchAsync(tranDauId);
            return await CanAccessTournamentAsync(giaiDauId);
        }

        /// <summary>
        /// Kiểm tra toàn bộ danh sách trận đấu đều thuộc các giải mà tài khoản thư ký được phân công.
        /// </summary>
        /// <param name="tranDauIds">Danh sách ID trận đấu cần kiểm tra.</param>
        /// <returns>True nếu mọi trận đấu đều được phép truy cập.</returns>
        protected async Task<bool> CanAccessMatchesAsync(IEnumerable<int> tranDauIds)
        {
            if (!IsRestrictedSecretary)
            {
                return true;
            }

            foreach (var tranDauId in tranDauIds.Distinct())
            {
                if (!await CanAccessMatchAsync(tranDauId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Kiểm tra quyền truy cập của tài khoản hiện tại đối với một nội dung thi đấu.
        /// </summary>
        /// <param name="giaiDauMonTheThaoId">ID nội dung thuộc giải đấu.</param>
        /// <returns>True nếu nội dung thuộc giải được phép truy cập.</returns>
        protected async Task<bool> CanAccessContentAsync(int giaiDauMonTheThaoId)
        {
            if (!IsRestrictedSecretary)
            {
                return true;
            }

            var giaiDauId = await _thuKyService.GetTournamentIdByContentAsync(giaiDauMonTheThaoId);
            return await CanAccessTournamentAsync(giaiDauId);
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
