using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Areas.DonVi.Controllers
{
    public class MedalRankingItem
    {
        public int DonViId { get; set; }
        public string TenDonVi { get; set; } = string.Empty;
        public int Gold { get; set; }
        public int Silver { get; set; }
        public int Bronze { get; set; }
        public int Total => Gold + Silver + Bronze;
        public bool IsCurrentUnit { get; set; }
    }

    public class UnitMedalDetail
    {
        public int Id { get; set; }
        public string TenMonTheThao { get; set; } = string.Empty;
        public string TenDangKy { get; set; } = string.Empty;
        public string LoaiHuyChuong { get; set; } = string.Empty;
        public int XepHang { get; set; }
        public DateTime? NgayTrao { get; set; }
    }

    public class ThanhTichController : BaseDonViController
    {
        private readonly IHuyChuongService _huyChuongService;
        private readonly IGiaiDauService _giaiDauService;
        private readonly IDangKyThiDauService _dangKyThiDauService;

        public ThanhTichController(
            UserManager<ApplicationUser> userManager,
            IDonViService donViService,
            IHuyChuongService huyChuongService,
            IGiaiDauService giaiDauService,
            IDangKyThiDauService dangKyThiDauService)
            : base(userManager, donViService)
        {
            _huyChuongService = huyChuongService;
            _giaiDauService = giaiDauService;
            _dangKyThiDauService = dangKyThiDauService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? giaiDauId = null)
        {
            var currentUnit = await GetCurrentDonViAsync();
            var tournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if (!selectedGiaiDauId.HasValue && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            var medals = (await _huyChuongService.GetAllAsync(selectedGiaiDauId))?.ToList() ?? new();
            var regs = (await _dangKyThiDauService.GetAllAsync(giaiDauId: selectedGiaiDauId))?.ToList() ?? new();
            var regMap = regs.ToDictionary(r => r.Id, r => r);

            // Bảng tổng sắp theo đơn vị
            var unitStats = new Dictionary<string, MedalRankingItem>(StringComparer.OrdinalIgnoreCase);

            // Danh sách huy chương riêng của đơn vị hiện tại
            var myMedals = new List<UnitMedalDetail>();

            foreach (var m in medals)
            {
                string donViName = "Đoàn Tự Do";
                int donViIdVal = 0;

                if (regMap.TryGetValue(m.DangKyThiDauId, out var reg))
                {
                    if (!string.IsNullOrEmpty(reg.TenDonVi)) donViName = reg.TenDonVi;
                    if (reg.DonViId.HasValue) donViIdVal = reg.DonViId.Value;
                }

                if (!unitStats.TryGetValue(donViName, out var stat))
                {
                    stat = new MedalRankingItem
                    {
                        DonViId = donViIdVal,
                        TenDonVi = donViName,
                        IsCurrentUnit = currentUnit != null && (donViIdVal == currentUnit.Id || donViName.Equals(currentUnit.Ten, StringComparison.OrdinalIgnoreCase))
                    };
                    unitStats[donViName] = stat;
                }

                bool isGold = m.XepHang == 1 || (!string.IsNullOrEmpty(m.TenLoaiHuyChuong) && m.TenLoaiHuyChuong.Contains("Vàng", StringComparison.OrdinalIgnoreCase));
                bool isSilver = m.XepHang == 2 || (!string.IsNullOrEmpty(m.TenLoaiHuyChuong) && m.TenLoaiHuyChuong.Contains("Bạc", StringComparison.OrdinalIgnoreCase));
                bool isBronze = m.XepHang == 3 || (!string.IsNullOrEmpty(m.TenLoaiHuyChuong) && m.TenLoaiHuyChuong.Contains("Đồng", StringComparison.OrdinalIgnoreCase));

                if (isGold) stat.Gold++;
                else if (isSilver) stat.Silver++;
                else if (isBronze) stat.Bronze++;

                if (stat.IsCurrentUnit)
                {
                    myMedals.Add(new UnitMedalDetail
                    {
                        Id = m.Id,
                        TenMonTheThao = m.TenMonTheThao ?? "Môn thi",
                        TenDangKy = m.TenDangKy ?? (reg?.TenDangKy ?? reg?.TenDoi ?? "--"),
                        LoaiHuyChuong = m.TenLoaiHuyChuong ?? (isGold ? "Huy Chương Vàng" : (isSilver ? "Huy Chương Bạc" : "Huy Chương Đồng")),
                        XepHang = m.XepHang,
                        NgayTrao = m.NgayTrao
                    });
                }
            }

            var ranking = unitStats.Values
                .OrderByDescending(s => s.Gold)
                .ThenByDescending(s => s.Silver)
                .ThenByDescending(s => s.Bronze)
                .ThenBy(s => s.TenDonVi)
                .ToList();

            ViewBag.Ranking = ranking;
            ViewBag.MyMedals = myMedals;

            // Thống kê nhanh của đoàn
            var myStat = ranking.FirstOrDefault(r => r.IsCurrentUnit);
            ViewBag.MyStat = myStat;
            ViewBag.MyRank = myStat != null ? (ranking.IndexOf(myStat) + 1) : 0;

            return View();
        }
    }
}
