using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Areas.TrongTai.Controllers
{
    internal sealed class MatchScoreSnapshot
    {
        public MatchScoreSnapshot() { }

        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public int? ExtraTimeScore1 { get; set; }
        public int? ExtraTimeScore2 { get; set; }
        public string? Winner { get; set; }
        public string? Notes { get; set; }
        public List<SetScoreDto> SetScores { get; set; } = new();
        public List<MatchEventItemDto> Events { get; set; } = new();
        public Dictionary<string, SignatureInfoDto>? Signatures { get; set; }
    }

    public class SaveMatchResultDto
    {
        public int TranDauId { get; set; }
        public int Score1 { get; set; }
        public int Score2 { get; set; }
        public string? Winner { get; set; } // "1", "2", "draw"
        public string TrangThai { get; set; } = "DangDau";
        public int? PenaltyScore1 { get; set; }
        public int? PenaltyScore2 { get; set; }
        public int? ExtraTimeScore1 { get; set; }
        public int? ExtraTimeScore2 { get; set; }
        public List<SetScoreDto>? SetScores { get; set; }
        public List<MatchEventItemDto>? Events { get; set; }
        public List<HeatParticipantResultDto>? HeatResults { get; set; }
        public string? GhiChu { get; set; }
    }

    public class StartMatchRequestDto
    {
        public int TranDauId { get; set; }
    }

    public class KetQuaController : BaseTrongTaiController
    {
        private readonly IGiaiDauService _giaiDauService;
        private readonly ICauHinhTheThucService _cauHinhTheThucService;
        private readonly IDangKyThiDauService _dangKyThiDauService;
        private readonly IAthleticsProgressionEngine _athleticsProgressionEngine;

        public KetQuaController(
            UserManager<ApplicationUser> userManager,
            ITrongTaiService trongTaiService,
            ITranDauService tranDauService,
            IGiaiDauService giaiDauService,
            ICauHinhTheThucService cauHinhTheThucService,
            IDangKyThiDauService dangKyThiDauService,
            ITruongBanTrongTaiService refereeAccessService,
            IAthleticsProgressionEngine athleticsProgressionEngine)
            : base(userManager, trongTaiService, refereeAccessService, tranDauService)
        {
            _giaiDauService = giaiDauService;
            _cauHinhTheThucService = cauHinhTheThucService;
            _dangKyThiDauService = dangKyThiDauService;
            _athleticsProgressionEngine = athleticsProgressionEngine;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? tranDauId = null,
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            string? vaiTroPhanCong = null)
        {
            var currentReferee = await GetCurrentRefereeAsync();
            var canBrowseAll = CanBrowseAllMatches();
            var allTournaments = (await _giaiDauService.GetAllAsync())?.ToList() ?? new();
            var assignedTournamentIds = await GetAssignedTournamentIdsAsync(currentReferee);
            var tournaments = canBrowseAll
                ? allTournaments
                : allTournaments.Where(tournament => assignedTournamentIds.Contains(tournament.Id)).ToList();
            ViewBag.Tournaments = tournaments;

            int? selectedGiaiDauId = giaiDauId;
            if ((!selectedGiaiDauId.HasValue || tournaments.All(tournament => tournament.Id != selectedGiaiDauId.Value)) && tournaments.Count > 0)
            {
                selectedGiaiDauId = tournaments[0].Id;
            }
            if (tournaments.Count == 0) selectedGiaiDauId = null;
            ViewBag.SelectedGiaiDauId = selectedGiaiDauId;

            var accessibleMatches = selectedGiaiDauId.HasValue
                ? (await _tranDauService.GetAccessibleMatchesAsync(selectedGiaiDauId, currentReferee?.Id, canBrowseAll))?.ToList() ?? new()
                : new List<TranDauDto>();

            var accessibleMatchIds = accessibleMatches.Select(match => match.Id).ToList();
            var mainRefereeMatchIds = currentReferee != null
                ? await _refereeAccessService.GetHeadRefereeMatchIdsAsync(currentReferee.Id, accessibleMatchIds)
                : new HashSet<int>();
            var assistantRefereeMatchIds = currentReferee != null
                ? await _refereeAccessService.GetAssistantRefereeMatchIdsAsync(currentReferee.Id, accessibleMatchIds)
                : new HashSet<int>();

            var selectedAssignmentRole = vaiTroPhanCong is "Chinh" or "Phu" ? vaiTroPhanCong : string.Empty;
            var requestedMatch = tranDauId.HasValue
                ? accessibleMatches.FirstOrDefault(match => match.Id == tranDauId.Value)
                : null;
            if (tranDauId.HasValue && requestedMatch == null) return Forbid();
            if (requestedMatch != null)
            {
                if (mainRefereeMatchIds.Contains(requestedMatch.Id)) selectedAssignmentRole = "Chinh";
                else if (assistantRefereeMatchIds.Contains(requestedMatch.Id)) selectedAssignmentRole = "Phu";
            }

            var roleFilteredMatches = selectedAssignmentRole switch
            {
                "Chinh" => accessibleMatches.Where(match => mainRefereeMatchIds.Contains(match.Id)).ToList(),
                "Phu" => accessibleMatches.Where(match => assistantRefereeMatchIds.Contains(match.Id)).ToList(),
                _ => accessibleMatches
            };
            ViewBag.SelectedAssignmentRole = selectedAssignmentRole;

            // Danh sách môn dựa trên toàn bộ trận được phép truy cập, không phụ thuộc vai trò đang lọc.
            // Nhờ vậy đổi vai trò không làm mất môn đã chọn; kết quả trận vẫn giao giữa hai bộ lọc.
            var availableSports = accessibleMatches
                .GroupBy(match => match.GiaiDauMonTheThaoId)
                .Select(group => new KeyValuePair<int, string>(
                    group.Key,
                    group.Select(match => match.TenMonTheThao).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
                        ?? group.Select(match => match.TenDanhMucMonTheThao).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
                        ?? $"Môn thi đấu #{group.Key}"))
                .OrderBy(sport => sport.Value)
                .ToList();
            if (giaiDauMonTheThaoId.HasValue && availableSports.All(sport => sport.Key != giaiDauMonTheThaoId.Value))
            {
                giaiDauMonTheThaoId = null;
            }

            if (requestedMatch != null && requestedMatch.GiaiDauMonTheThaoId != giaiDauMonTheThaoId)
            {
                giaiDauMonTheThaoId = requestedMatch.GiaiDauMonTheThaoId;
            }

            ViewBag.Sports = availableSports;
            ViewBag.SelectedGiaiDauMonTheThaoId = giaiDauMonTheThaoId;
            var matches = giaiDauMonTheThaoId.HasValue
                ? roleFilteredMatches.Where(match => match.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value).ToList()
                : roleFilteredMatches;
            ViewBag.Matches = matches;

            TranDauDto? currentMatch = null;
            if (tranDauId.HasValue)
            {
                currentMatch = matches.FirstOrDefault(m => m.Id == tranDauId.Value);
                if (currentMatch == null) return Forbid();
            }
            else if (matches.Count > 0)
            {
                currentMatch = matches[0];
            }

            ViewBag.CurrentMatch = currentMatch;
            var canRecordResult = await CanRecordMatchAsync(currentMatch);
            ViewBag.CanRecordResult = canRecordResult;

            CauHinhTheThucDto? matchFormat = null;
            if (currentMatch != null)
            {
                matchFormat = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(currentMatch.Id);
            }
            ViewBag.MatchFormat = matchFormat;
            ViewBag.HeatResults = currentMatch != null && (matchFormat?.IsPerformanceSport == true || (currentMatch.ThanhPhanTranDaus?.Count > 2))
                ? await _tranDauService.GetHeatResultsByMatchIdAsync(currentMatch.Id)
                : new List<HeatParticipantResultDto>();

            // Đọc dữ liệu JSON chi tiết từ GhiChu nếu có
            int score1 = 0, score2 = 0;
            string winner = "";
            string notes = "";
            var setScores = new List<SetScoreDto>();
            var events = new List<MatchEventItemDto>();
            var signatures = new Dictionary<string, SignatureInfoDto>(StringComparer.OrdinalIgnoreCase);

            if (currentMatch != null && !string.IsNullOrEmpty(currentMatch.GhiChu) && currentMatch.GhiChu.StartsWith("{"))
            {
                try
                {
                    var savedScore = JsonSerializer.Deserialize<MatchScoreSnapshot>(
                        currentMatch.GhiChu,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (savedScore != null)
                    {
                        score1 = savedScore.Score1;
                        score2 = savedScore.Score2;
                        winner = savedScore.Winner ?? "";
                        notes = savedScore.Notes ?? "";
                        setScores = savedScore.SetScores ?? new List<SetScoreDto>();
                        events = savedScore.Events ?? new List<MatchEventItemDto>();
                        signatures = savedScore.Signatures ?? signatures;
                        ViewBag.ExtraTimeScore1 = savedScore.ExtraTimeScore1;
                        ViewBag.ExtraTimeScore2 = savedScore.ExtraTimeScore2;
                    }
                }
                catch { }
            }

            if (currentMatch != null && matchFormat != null && !matchFormat.IsPerformanceSport)
            {
                var persistedPeriods = await _tranDauService.GetMatchPeriodScoresAsync(currentMatch.Id);
                if (persistedPeriods.Count > 0)
                {
                    setScores = persistedPeriods;
                }
                score1 = currentMatch.TySoDoi1 ?? score1;
                score2 = currentMatch.TySoDoi2 ?? score2;
            }

            ViewBag.Score1 = score1;
            ViewBag.Score2 = score2;
            ViewBag.Winner = winner;
            ViewBag.Notes = notes;
            ViewBag.SetScores = setScores;
            ViewBag.Events = events;
            ViewBag.Signatures = signatures;
            ViewBag.CanCreateReport = canRecordResult;

            DangKyThiDauDto? regTeam1 = null;
            DangKyThiDauDto? regTeam2 = null;
            if (currentMatch?.Doi1DangKyId is int team1RegistrationId)
            {
                regTeam1 = await _dangKyThiDauService.GetByIdAsync(team1RegistrationId);
            }
            if (currentMatch?.Doi2DangKyId is int team2RegistrationId)
            {
                regTeam2 = await _dangKyThiDauService.GetByIdAsync(team2RegistrationId);
            }
            ViewBag.RegTeam1 = regTeam1;
            ViewBag.RegTeam2 = regTeam2;

            return View(currentMatch);
        }

        [HttpPost]
        public async Task<IActionResult> StartMatch([FromBody] StartMatchRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return BadRequest(new { success = false, message = "Thông tin trận đấu không hợp lệ." });
            }

            var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
            if (match == null) return NotFound(new { success = false, message = "Không tìm thấy trận đấu." });
            if (!await CanRecordMatchAsync(match)) return Forbid();

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";
            try
            {
                var started = await _tranDauService.StartMatchAsync(dto.TranDauId, username);
                if (!started) return NotFound(new { success = false, message = "Không tìm thấy trận đấu." });
                return Json(new { success = true, message = "Trận đấu đã bắt đầu. Trọng tài có thể nhập điểm." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveResult([FromBody] SaveMatchResultDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }
            if (dto.Score1 < 0 || dto.Score2 < 0 || !new[] { "ChuaDau", "DangDau", "KetThuc" }.Contains(dto.TrangThai))
            {
                return BadRequest(new { success = false, message = "Tỷ số hoặc trạng thái trận đấu không hợp lệ." });
            }

            var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
            if (match == null)
            {
                return Json(new { success = false, message = "Không tìm thấy trận đấu." });
            }

            if (!await CanRecordMatchAsync(match)) return Forbid();

            var matchFormat = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(match.Id);
            if (matchFormat?.CoThePhat != true && dto.Events?.Any(IsPenaltyCardEvent) == true)
            {
                return BadRequest(new { success = false, message = "Môn thi đấu này không áp dụng thẻ phạt theo cấu hình." });
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";
            var nowUtc = DateTime.UtcNow;

            if (matchFormat?.IsPerformanceSport == true)
            {
                if (dto.HeatResults != null && dto.HeatResults.Any())
                {
                    await _athleticsProgressionEngine.SaveDraftHeatResultsAsync(dto.TranDauId, dto.HeatResults, matchFormat, username);
                }
                return Json(new { success = true, message = "Đã lưu tạm kết quả và lượt thi thành công!" });
            }

            try
            {
                var scoreData = new
                {
                    score1 = dto.Score1,
                    score2 = dto.Score2,
                    penaltyScore1 = dto.PenaltyScore1,
                    penaltyScore2 = dto.PenaltyScore2,
                    extraTimeScore1 = dto.ExtraTimeScore1,
                    extraTimeScore2 = dto.ExtraTimeScore2,
                    winner = dto.Winner,
                    status = dto.TrangThai,
                    notes = dto.GhiChu,
                    setScores = dto.SetScores ?? new List<SetScoreDto>(),
                    events = dto.Events ?? new List<MatchEventItemDto>(),
                    updatedBy = username,
                    updatedAt = nowUtc
                };

                string scoreJson = JsonSerializer.Serialize(scoreData);

                var isDraw = dto.TrangThai == "KetThuc" && dto.Winner == "draw";
                var winnerId = dto.TrangThai == "KetThuc" && dto.Winner == "1" ? match.Doi1DangKyId
                    : dto.TrangThai == "KetThuc" && dto.Winner == "2" ? match.Doi2DangKyId
                    : null;
                var loserId = dto.TrangThai == "KetThuc" && dto.Winner == "1" ? match.Doi2DangKyId
                    : dto.TrangThai == "KetThuc" && dto.Winner == "2" ? match.Doi1DangKyId
                    : null;
                var updateDto = new UpdateMatchProgressDto
                {
                    Score1 = dto.Score1,
                    Score2 = dto.Score2,
                    PenaltyScore1 = dto.PenaltyScore1,
                    PenaltyScore2 = dto.PenaltyScore2,
                    ExtraTimeScore1 = dto.ExtraTimeScore1,
                    ExtraTimeScore2 = dto.ExtraTimeScore2,
                    SetScores = dto.SetScores,
                    TrangThai = dto.TrangThai,
                    GhiChu = scoreJson,
                    IsHoa = isDraw,
                    DoiThangDangKyId = winnerId,
                    DoiThuaDangKyId = loserId
                };

                await _tranDauService.UpdateMatchProgressAsync(dto.TranDauId, updateDto, username, requireInProgress: true);
                return Json(new { success = true, message = "Đã lưu kết quả, điểm số và diễn biến trận đấu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMatchFormat(int tranDauId)
        {
            try
            {
                var match = await _tranDauService.GetByIdAsync(tranDauId);
                if (match == null) return NotFound(new { success = false, message = "Không tìm thấy trận đấu." });
                if (!await CanAccessMatchAsync(match)) return Forbid();

                var config = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(tranDauId);
                return Json(new { success = true, data = config });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteMatch([FromBody] CompleteMatchRequestDto dto)
        {
            if (dto == null || dto.TranDauId <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                var match = await _tranDauService.GetByIdAsync(dto.TranDauId);
                if (match == null) return NotFound(new { success = false, message = "Không tìm thấy trận đấu." });
                if (!await CanRecordMatchAsync(match)) return Forbid();

                var matchFormat = await _cauHinhTheThucService.GetConfigByTranDauIdAsync(match.Id);
                if (matchFormat?.CoThePhat != true && dto.Events?.Any(IsPenaltyCardEvent) == true)
                {
                    return BadRequest(new { success = false, message = "Môn thi đấu này không áp dụng thẻ phạt theo cấu hình." });
                }

                var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "TrongTai";
                var response = await _tranDauService.CompleteMatchResultAsync(dto, username);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static bool IsPenaltyCardEvent(MatchEventItemDto matchEvent)
        {
            return string.Equals(matchEvent.Type, "yellow_card", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(matchEvent.Type, "red_card", StringComparison.OrdinalIgnoreCase);
        }
    }
}
