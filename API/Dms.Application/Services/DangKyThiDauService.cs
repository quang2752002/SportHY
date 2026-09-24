using AutoMapper;
using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Common;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class DangKyThiDauService : IDangKyThiDauService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DangKyThiDauService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy danh sách hồ sơ đăng ký thi đấu có phân trang theo các điều kiện lọc (từ khóa, giải đấu, môn, đơn vị, trạng thái).
        /// </summary>
        /// <param name="pageIndex">Số trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng bản ghi trên một trang</param>
        /// <param name="keyword">Từ khóa tìm kiếm theo số đăng ký hoặc tên đăng ký</param>
        /// <param name="giaiDauId">Lọc theo mã định danh giải đấu</param>
        /// <param name="giaiDauMonTheThaoId">Lọc theo môn thi đấu trong giải</param>
        /// <param name="donViId">Lọc theo đơn vị / đoàn</param>
        /// <param name="trangThai">Lọc theo trạng thái hồ sơ</param>
        /// <returns>Danh sách phân trang các hồ sơ đăng ký thi đấu kèm thông tin chi tiết</returns>
        public async Task<PagedResult<DangKyThiDauDto>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword = null,
            int? giaiDauId = null,
            int? giaiDauMonTheThaoId = null,
            int? donViId = null,
            string? trangThai = null)
        {
            var pagedEntities = await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                pageIndex,
                pageSize,
                predicate: d => d.IsDeleted != true &&
                                (string.IsNullOrEmpty(keyword) || d.SoDangKy.Contains(keyword) || (d.TenDangKy != null && d.TenDangKy.Contains(keyword))) &&
                                (!giaiDauMonTheThaoId.HasValue || d.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value) &&
                                (string.IsNullOrEmpty(trangThai) || d.TrangThai == trangThai) &&
                                (!donViId.HasValue || (d.Doi != null && d.Doi.DonViId == donViId.Value)) &&
                                (!giaiDauId.HasValue || d.GiaiDauMonTheThao.GiaiDauId == giaiDauId.Value),
                orderBy: q => q.OrderByDescending(d => d.NgayDangKy),
                d => d.GiaiDauMonTheThao,
                d => d.GiaiDauMonTheThao.GiaiDau,
                d => d.GiaiDauMonTheThao.MonTheThao,
                d => d.Doi!,
                d => d.Doi!.DonVi!
            );

            var dtos = await MapToRichDtosAsync(pagedEntities.Items);
            return new PagedResult<DangKyThiDauDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        /// <summary>
        /// Lấy tất cả danh sách hồ sơ đăng ký thi đấu theo giải đấu, môn và đơn vị.
        /// </summary>
        /// <param name="giaiDauId">Mã định danh giải đấu</param>
        /// <param name="giaiDauMonTheThaoId">Mã định danh môn thi đấu trong giải</param>
        /// <param name="donViId">Mã định danh đơn vị / đoàn</param>
        /// <returns>Tập hợp các hồ sơ đăng ký thi đấu</returns>
        public async Task<IEnumerable<DangKyThiDauDto>> GetAllAsync(int? giaiDauId = null, int? giaiDauMonTheThaoId = null, int? donViId = null)
        {
            var paged = await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1,
                1000,
                predicate: d => d.IsDeleted != true &&
                                (!giaiDauMonTheThaoId.HasValue || d.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value) &&
                                (!donViId.HasValue || (d.Doi != null && d.Doi.DonViId == donViId.Value)) &&
                                (!giaiDauId.HasValue || d.GiaiDauMonTheThao.GiaiDauId == giaiDauId.Value),
                orderBy: q => q.OrderByDescending(d => d.NgayDangKy),
                d => d.GiaiDauMonTheThao,
                d => d.GiaiDauMonTheThao.GiaiDau,
                d => d.GiaiDauMonTheThao.MonTheThao,
                d => d.Doi!,
                d => d.Doi!.DonVi!
            );

            return await MapToRichDtosAsync(paged.Items);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một hồ sơ đăng ký thi đấu theo Id.
        /// </summary>
        /// <param name="id">Mã định danh hồ sơ đăng ký thi đấu</param>
        /// <returns>Chi tiết hồ sơ đăng ký thi đấu hoặc null nếu không tồn tại</returns>
        public async Task<DangKyThiDauDto?> GetByIdAsync(int id)
        {
            var paged = await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1,
                1,
                predicate: d => d.Id == id && d.IsDeleted != true,
                orderBy: null,
                d => d.GiaiDauMonTheThao,
                d => d.GiaiDauMonTheThao.GiaiDau,
                d => d.GiaiDauMonTheThao.MonTheThao,
                d => d.Doi!,
                d => d.Doi!.DonVi!
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return null;

            var dtos = await MapToRichDtosAsync(new List<DangKyThiDau> { entity });
            return dtos.FirstOrDefault();
        }

        /// <summary>
        /// Tạo mới hồ sơ đăng ký thi đấu, kiểm tra tính hợp lệ về thời hạn đăng ký, giới tính, số lượng VĐV, trùng lặp VĐV, bắt buộc tên đội với môn đồng đội và tự động giải mã HTML entities.
        /// </summary>
        /// <param name="dto">Dữ liệu đăng ký thi đấu</param>
        /// <param name="createdBy">Tài khoản tạo hồ sơ</param>
        /// <param name="isPrivileged">Cờ đặc quyền cho phép đăng ký quá hạn</param>
        /// <returns>Hồ sơ đăng ký đã tạo</returns>
        public async Task<DangKyThiDauDto> CreateAsync(CreateUpdateDangKyThiDauDto dto, string? createdBy = null, bool isPrivileged = false)
        {
            var gdMon = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(dto.GiaiDauMonTheThaoId);
            if (gdMon == null || gdMon.IsDeleted == true)
            {
                throw new InvalidOperationException("Môn thi đấu trong giải không tồn tại hoặc đã bị xóa.");
            }

            var monTheThao = await _unitOfWork.MonTheThaos.GetByIdAsync(gdMon.MonTheThaoId);

            // Kiểm tra hạn đăng ký giải đấu
            if (!isPrivileged)
            {
                var giaiDau = await _unitOfWork.GiaiDaus.GetByIdAsync(gdMon.GiaiDauId);
                if (giaiDau != null)
                {
                    var deadline = giaiDau.HanDangKy ?? giaiDau.NgayBatDau;
                    if (DateTime.Now > deadline || giaiDau.TrangThai == Dms.Domain.Enums.TrangThaiGiaiDau.KetThuc || giaiDau.TrangThai == Dms.Domain.Enums.TrangThaiGiaiDau.Huy)
                    {
                        throw new InvalidOperationException($"Giải đấu \"{giaiDau.Ten}\" đã hết hạn đăng ký thi đấu (Hạn chót: {deadline:dd/MM/yyyy HH:mm}). Không thể gửi thêm hồ sơ mới.");
                    }
                }
            }

            var vdvIds = dto.VanDongVienIds?.Distinct().ToList() ?? new List<int>();
            if (!vdvIds.Any())
            {
                throw new InvalidOperationException("Vui lòng chọn ít nhất một vận động viên.");
            }

            // Kiểm tra số lượng VĐV theo loại thi đấu: Cá nhân chỉ cho phép đúng 1 VĐV
            if (monTheThao != null && !monTheThao.LaMonDongDoi)
            {
                if (vdvIds.Count > 1)
                {
                    throw new InvalidOperationException($"Môn '{monTheThao.Ten}' là môn thi đấu cá nhân, chỉ được chọn 1 vận động viên.");
                }
            }
            else if (monTheThao != null)
            {
                // Nội dung tập thể/đồng đội/đôi
                if (monTheThao.SoLuongVanDongVienToiThieu.HasValue && vdvIds.Count < monTheThao.SoLuongVanDongVienToiThieu.Value)
                {
                    throw new InvalidOperationException($"Môn '{monTheThao.Ten}' yêu cầu tối thiểu {monTheThao.SoLuongVanDongVienToiThieu.Value} vận động viên (hiện có {vdvIds.Count}).");
                }
                if (monTheThao.SoLuongVanDongVienToiDa.HasValue && vdvIds.Count > monTheThao.SoLuongVanDongVienToiDa.Value)
                {
                    throw new InvalidOperationException($"Môn '{monTheThao.Ten}' chỉ cho phép tối đa {monTheThao.SoLuongVanDongVienToiDa.Value} vận động viên (hiện có {vdvIds.Count}).");
                }
            }

            // Kiểm tra giới tính VĐV so với quy định của môn thi đấu
            if (monTheThao != null && !string.IsNullOrWhiteSpace(monTheThao.GioiTinh))
            {
                var normSportGender = NormalizeGender(monTheThao.GioiTinh);
                if (normSportGender != "HonHop")
                {
                    var vdvs = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id))).ToList();
                    var invalidGenderVdvs = vdvs.Where(v => !string.IsNullOrEmpty(v.GioiTinh) &&
                        NormalizeGender(v.GioiTinh) != normSportGender).ToList();

                    if (invalidGenderVdvs.Any())
                    {
                        var genderLabel = normSportGender == "Nam" ? "Nam" : "Nữ";
                        var invalidNames = string.Join(", ", invalidGenderVdvs.Select(v => $"{v.HoTen} ({v.GioiTinh})"));
                        throw new InvalidOperationException($"Môn thi đấu '{monTheThao.Ten}' chỉ dành cho vận động viên {genderLabel}. Các vận động viên sau không đúng giới tính quy định: {invalidNames}.");
                    }
                }
            }

            // Kiểm tra trùng VĐV đã đăng ký trong môn thi đấu này của giải đấu
            var activeRegistrations = (await _unitOfWork.DangKyThiDaus.FindAsync(
                d => d.GiaiDauMonTheThaoId == dto.GiaiDauMonTheThaoId &&
                     d.IsDeleted != true &&
                     d.TrangThai != "TuChoi"
            )).ToList();

            var activeRegIds = activeRegistrations.Select(r => r.Id).ToList();
            if (activeRegIds.Any())
            {
                // Kiểm tra trùng VDV qua ThanhVienDoi của các đội đã đăng ký
                var activeDoiIds = activeRegistrations.Where(r => r.DoiId.HasValue).Select(r => r.DoiId!.Value).Distinct().ToList();
                var dupVdvIds = new List<int>();
                if (activeDoiIds.Any())
                {
                    var teamMembers = await _unitOfWork.ThanhVienDois.FindAsync(
                        tv => activeDoiIds.Contains(tv.DoiId) &&
                              tv.IsDeleted != true &&
                              vdvIds.Contains(tv.VanDongVienId)
                    );
                    dupVdvIds = teamMembers.Select(tv => tv.VanDongVienId).Distinct().ToList();
                }

                if (dupVdvIds.Any())
                {
                    var dupVdvs = (await _unitOfWork.VanDongViens.FindAsync(v => dupVdvIds.Contains(v.Id))).ToList();
                    var dupNames = string.Join(", ", dupVdvs.Select(v => v.HoTen));
                    throw new InvalidOperationException($"Vận động viên [{dupNames}] đã được đăng ký tham gia môn thi đấu này. Không thể đăng ký trùng.");
                }
            }

            int? resolvedDoiId = dto.DoiId;
            var vdvEntities = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id))).ToList();

            // Tự động tạo Doi + ThanhVienDoi nếu chưa có DoiId
            if (!resolvedDoiId.HasValue && vdvIds.Any())
            {
                int? donViId = dto.DonViId ?? vdvEntities.FirstOrDefault(v => v.DonViId.HasValue)?.DonViId;

                string tenDoi = dto.TenDoi?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(tenDoi))
                {
                    tenDoi = System.Net.WebUtility.HtmlDecode(tenDoi);
                }
                if (string.IsNullOrWhiteSpace(tenDoi))
                {
                    if (monTheThao != null && monTheThao.LaMonDongDoi)
                    {
                        throw new InvalidOperationException("Vui lòng nhập tên đội thi đấu cho môn thi đồng đội.");
                    }

                    if (vdvEntities.Count == 1)
                    {
                        tenDoi = vdvEntities[0].HoTen;
                    }
                    else
                    {
                        tenDoi = string.Join(" - ", vdvEntities.Select(v => v.HoTen));
                        if (tenDoi.Length > 190)
                        {
                            tenDoi = tenDoi.Substring(0, 187) + "...";
                        }
                    }
                }

                var randomSuffix = new Random().Next(100, 999);
                var maDoi = $"DOI_{DateTime.Now:yyyyMMddHHmmss}_{randomSuffix}";

                var newDoi = new Doi
                {
                    Ma = maDoi,
                    Ten = tenDoi,
                    DonViId = donViId,
                    TrangThai = true,
                    CreatedBy = createdBy,
                    Created = DateTime.Now
                };
                await _unitOfWork.Dois.AddAsync(newDoi);
                await _unitOfWork.CompleteAsync();

                // Thêm từng VĐV vào ThanhVienDoi
                bool isFirst = true;
                foreach (var vdvId in vdvIds)
                {
                    await _unitOfWork.ThanhVienDois.AddAsync(new ThanhVienDoi
                    {
                        DoiId = newDoi.Id,
                        VanDongVienId = vdvId,
                        LaDoiTruong = isFirst,
                        NgayThamGia = DateTime.Now,
                        CreatedBy = createdBy,
                        Created = DateTime.Now
                    });
                    isFirst = false;
                }
                await _unitOfWork.CompleteAsync();
                resolvedDoiId = newDoi.Id;
            }

            string resolvedTenDangKy;
            if (!string.IsNullOrWhiteSpace(dto.TenDangKy))
            {
                resolvedTenDangKy = dto.TenDangKy;
            }
            else if (monTheThao != null && !monTheThao.LaMonDongDoi)
            {
                // Môn cá nhân: ưu tiên lấy họ tên VĐV
                resolvedTenDangKy = vdvEntities.FirstOrDefault()?.HoTen ?? (!string.IsNullOrWhiteSpace(dto.TenDoi) ? dto.TenDoi : "Vận động viên");
            }
            else
            {
                // Môn đồng đội: lấy tên đội
                resolvedTenDangKy = !string.IsNullOrWhiteSpace(dto.TenDoi) ? dto.TenDoi : "Đội thi đấu";
            }

            var entity = new DangKyThiDau
            {
                GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                DoiId = resolvedDoiId,
                SoDangKy = string.IsNullOrWhiteSpace(dto.SoDangKy) ? $"DK_{DateTime.Now:yyyyMMddHHmmss}" : dto.SoDangKy,
                TenDangKy = resolvedTenDangKy,
                TrangThai = "DaDuyet", // Luôn mặc định đã duyệt theo yêu cầu của hệ thống
                NgayDangKy = dto.NgayDangKy != default ? dto.NgayDangKy : DateTime.Now,
                GhiChu = dto.GhiChu,
                CreatedBy = createdBy,
                Created = DateTime.Now,
                IsDeleted = false
            };

            await _unitOfWork.DangKyThiDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return (await GetByIdAsync(entity.Id))!;
        }


        public async Task<DangKyThiDauDto?> UpdateAsync(int id, CreateUpdateDangKyThiDauDto dto, string? updatedBy = null, bool isPrivileged = false)
        {
            var entity = await _unitOfWork.DangKyThiDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return null;

            // Kiểm tra hạn đăng ký giải đấu
            if (!isPrivileged)
            {
                var targetId = dto.GiaiDauMonTheThaoId != 0 ? dto.GiaiDauMonTheThaoId : entity.GiaiDauMonTheThaoId;
                var gdMon = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(targetId);
                if (gdMon != null)
                {
                    var giaiDau = await _unitOfWork.GiaiDaus.GetByIdAsync(gdMon.GiaiDauId);
                    if (giaiDau != null)
                    {
                        var deadline = giaiDau.HanDangKy ?? giaiDau.NgayBatDau;
                        if (DateTime.Now > deadline || giaiDau.TrangThai == Dms.Domain.Enums.TrangThaiGiaiDau.KetThuc || giaiDau.TrangThai == Dms.Domain.Enums.TrangThaiGiaiDau.Huy)
                        {
                            throw new InvalidOperationException($"Giải đấu \"{giaiDau.Ten}\" đã hết hạn đăng ký thi đấu (Hạn chót: {deadline:dd/MM/yyyy HH:mm}). Không thể chỉnh sửa hồ sơ.");
                        }
                    }
                }
            }

            entity.GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId;
            entity.DoiId = dto.DoiId;
            if (!string.IsNullOrWhiteSpace(dto.SoDangKy)) entity.SoDangKy = dto.SoDangKy;
            entity.TenDangKy = dto.TenDangKy;
            entity.TrangThai = dto.TrangThai;
            entity.NgayDangKy = dto.NgayDangKy;
            entity.GhiChu = dto.GhiChu;
            entity.LastModifiedBy = updatedBy;
            entity.LastModified = DateTime.Now;

            _unitOfWork.DangKyThiDaus.Update(entity);

            if (dto.VanDongVienIds != null && dto.VanDongVienIds.Any())
            {
                var targetId = dto.GiaiDauMonTheThaoId != 0 ? dto.GiaiDauMonTheThaoId : entity.GiaiDauMonTheThaoId;
                var activeRegistrations = (await _unitOfWork.DangKyThiDaus.FindAsync(
                    d => d.GiaiDauMonTheThaoId == targetId &&
                         d.Id != id &&
                         d.IsDeleted != true &&
                         d.TrangThai != "TuChoi"
                )).ToList();

                var activeDoiIds2 = activeRegistrations.Where(r => r.DoiId.HasValue).Select(r => r.DoiId!.Value).Distinct().ToList();
                var newVdvIds = dto.VanDongVienIds.Distinct().ToList();
                var dupVdvIds2 = new List<int>();
                if (activeDoiIds2.Any())
                {
                    var teamMembers = await _unitOfWork.ThanhVienDois.FindAsync(
                        tv => activeDoiIds2.Contains(tv.DoiId) &&
                              tv.IsDeleted != true &&
                              newVdvIds.Contains(tv.VanDongVienId)
                    );
                    dupVdvIds2 = teamMembers.Select(tv => tv.VanDongVienId).Distinct().ToList();
                }

                if (dupVdvIds2.Any())
                {
                    var dupVdvs = (await _unitOfWork.VanDongViens.FindAsync(v => dupVdvIds2.Contains(v.Id))).ToList();
                    var dupNames = string.Join(", ", dupVdvs.Select(v => v.HoTen));
                    throw new InvalidOperationException($"Vận động viên [{dupNames}] đã được đăng ký tham gia môn thi đấu này. Không thể đăng ký trùng.");
                }

                // Cập nhật thành viên đội nếu có DoiId
                if (entity.DoiId.HasValue)
                {
                    var existingMembers = await _unitOfWork.ThanhVienDois.FindAsync(tv => tv.DoiId == entity.DoiId.Value);
                    foreach (var m in existingMembers)
                    {
                        m.IsDeleted = true;
                        _unitOfWork.ThanhVienDois.Update(m);
                    }
                    int stt = 1;
                    foreach (var vdvId in newVdvIds)
                    {
                        await _unitOfWork.ThanhVienDois.AddAsync(new ThanhVienDoi
                        {
                            DoiId = entity.DoiId.Value,
                            VanDongVienId = vdvId,
                            LaDoiTruong = stt == 1,
                            NgayThamGia = DateTime.Now,
                            CreatedBy = updatedBy,
                            Created = DateTime.Now
                        });
                        stt++;
                    }
                }
            }

            await _unitOfWork.CompleteAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id, bool isPrivileged = false)
        {
            var entity = await _unitOfWork.DangKyThiDaus.GetByIdAsync(id);
            if (entity == null || entity.IsDeleted == true) return false;

            // Kiểm tra hạn đăng ký giải đấu
            if (!isPrivileged)
            {
                var gdMon = await _unitOfWork.GiaiDauMonTheThaos.GetByIdAsync(entity.GiaiDauMonTheThaoId);
                if (gdMon != null)
                {
                    var giaiDau = await _unitOfWork.GiaiDaus.GetByIdAsync(gdMon.GiaiDauId);
                    if (giaiDau != null)
                    {
                        var deadline = giaiDau.HanDangKy ?? giaiDau.NgayBatDau;
                        if (DateTime.Now > deadline || giaiDau.TrangThai == Dms.Domain.Enums.TrangThaiGiaiDau.KetThuc || giaiDau.TrangThai == Dms.Domain.Enums.TrangThaiGiaiDau.Huy)
                        {
                            throw new InvalidOperationException($"Giải đấu \"{giaiDau.Ten}\" đã hết hạn đăng ký thi đấu (Hạn chót: {deadline:dd/MM/yyyy HH:mm}). Không thể xóa hoặc hủy hồ sơ.");
                        }
                    }
                }
            }

            // Kiểm tra nếu hồ sơ đã gắn vào trận đấu đang diễn ra hoặc đã kết thúc
            var matchParticipants = (await _unitOfWork.ThanhPhanTranDaus.FindAsync(
                tp => tp.DangKyThiDauId == id && tp.IsDeleted != true
            )).ToList();

            if (matchParticipants.Any())
            {
                var tranDauIds = matchParticipants.Select(tp => tp.TranDauId).Distinct().ToList();
                var tranDaus = (await _unitOfWork.TranDaus.FindAsync(t => tranDauIds.Contains(t.Id) && t.IsDeleted != true)).ToList();
                if (tranDaus.Any(t => t.TrangThai == "KetThuc" || t.TrangThai == "DangDienRa"))
                {
                    throw new InvalidOperationException("Hồ sơ đăng ký này đã tham gia trận đấu đang diễn ra hoặc đã kết thúc, không thể xóa.");
                }
            }

            entity.IsDeleted = true;
            _unitOfWork.DangKyThiDaus.Update(entity);

            // Không còn dùng ChiTietDangKyThiDau — VDV quản lý qua ThanhVienDoi

            // Xóa mềm các thành phần trận đấu chưa diễn ra
            foreach (var tp in matchParticipants)
            {
                tp.IsDeleted = true;
                _unitOfWork.ThanhPhanTranDaus.Update(tp);
            }

            // Xóa mềm thành viên bảng đấu
            var groupMembers = await _unitOfWork.ThanhVienBangs.FindAsync(tv => tv.DangKyThiDauId == id);
            foreach (var tv in groupMembers)
            {
                tv.IsDeleted = true;
                _unitOfWork.ThanhVienBangs.Update(tv);
            }

            // Nếu đội được tự động tạo (bắt đầu bằng DOI_), xóa đội nếu không còn hồ sơ nào khác dùng
            if (entity.DoiId.HasValue)
            {
                var doi = await _unitOfWork.Dois.GetByIdAsync(entity.DoiId.Value);
                if (doi != null && doi.Ma.StartsWith("DOI_"))
                {
                    var otherRegs = await _unitOfWork.DangKyThiDaus.FindAsync(r => r.DoiId == doi.Id && r.Id != id && r.IsDeleted != true);
                    if (!otherRegs.Any())
                    {
                        doi.IsDeleted = true;
                        _unitOfWork.Dois.Update(doi);

                        var members = await _unitOfWork.ThanhVienDois.FindAsync(tv => tv.DoiId == doi.Id);
                        foreach (var m in members)
                        {
                            m.IsDeleted = true;
                            _unitOfWork.ThanhVienDois.Update(m);
                        }
                    }
                }
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        /// <summary>
        /// Chuyển đổi danh sách entity DangKyThiDau sang DTO giàu thông tin (nạp thông tin môn, giải, đội, đơn vị/đoàn và danh sách VĐV).
        /// </summary>
        /// <param name="items">Danh sách entity đăng ký thi đấu</param>
        /// <returns>Danh sách DangKyThiDauDto kèm tên đơn vị và danh sách VĐV</returns>
        private async Task<List<DangKyThiDauDto>> MapToRichDtosAsync(IEnumerable<DangKyThiDau> items)
        {
            var list = items.ToList();
            if (!list.Any()) return new List<DangKyThiDauDto>();

            // Lấy VDV từ ThanhVienDoi thay vì ChiTietDangKyThiDaus navigation property
            var doiIdsList = list.Where(d => d.DoiId.HasValue).Select(d => d.DoiId!.Value).Distinct().ToList();
            var allThanhViens = doiIdsList.Any()
                ? (await _unitOfWork.ThanhVienDois.FindAsync(tv => doiIdsList.Contains(tv.DoiId) && tv.IsDeleted != true)).ToList()
                : new List<ThanhVienDoi>();
            var allVdvIds2 = allThanhViens.Select(tv => tv.VanDongVienId).Distinct().ToList();
            var vdvEntities = allVdvIds2.Any()
                ? (await _unitOfWork.VanDongViens.FindAsync(v => allVdvIds2.Contains(v.Id))).ToList()
                : new List<VanDongVien>();
            var vdvObjDict = vdvEntities.ToDictionary(v => v.Id, v => v);
            var vdvDict = vdvEntities.ToDictionary(v => v.Id, v => v.HoTen);
            var vdvDonViDict = vdvEntities.Where(v => v.DonViId.HasValue).ToDictionary(v => v.Id, v => v.DonViId!.Value);

            // Nạp danh sách đơn vị để ánh xạ TenDonVi chính xác
            var allDonViIds = list
                .Where(d => d.Doi?.DonViId.HasValue == true)
                .Select(d => d.Doi!.DonViId!.Value)
                .Concat(vdvDonViDict.Values)
                .Distinct()
                .ToList();

            var donViDict = allDonViIds.Any()
                ? (await _unitOfWork.DonVis.FindAsync(dv => allDonViIds.Contains(dv.Id) && dv.IsDeleted != true))
                    .ToDictionary(dv => dv.Id, dv => dv.Ten)
                : new Dictionary<int, string>();

            var result = new List<DangKyThiDauDto>();
            foreach (var d in list)
            {
                var gd = d.GiaiDauMonTheThao?.GiaiDau;
                var mon = d.GiaiDauMonTheThao?.MonTheThao;

                // Lấy VDV từ thành viên đội
                var membersInEntry = d.DoiId.HasValue
                    ? allThanhViens.Where(tv => tv.DoiId == d.DoiId.Value).ToList()
                    : new List<ThanhVienDoi>();

                var memberDtos = membersInEntry.Select(tv => {
                    vdvObjDict.TryGetValue(tv.VanDongVienId, out var vdv);
                    return new ThanhVienDoiChiTietDto
                    {
                        Id = tv.Id,
                        VanDongVienId = tv.VanDongVienId,
                        TenVanDongVien = vdv?.HoTen,
                        HoTen = vdv?.HoTen,
                        MaVanDongVien = vdv?.Ma,
                        SoAo = tv.SoAo,
                        ViTri = tv.ViTri,
                        LaDoiTruong = tv.LaDoiTruong
                    };
                }).ToList();

                var vdvsInEntry = membersInEntry.Select(tv => tv.VanDongVienId).ToList();
                var vdvNames = memberDtos.Select(m => m.HoTen ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList();

                int? resolvedDonViId = d.Doi?.DonViId;
                if (!resolvedDonViId.HasValue && vdvsInEntry.Any())
                {
                    var firstMatch = vdvsInEntry.FirstOrDefault(id => vdvDonViDict.ContainsKey(id));
                    if (firstMatch != 0)
                    {
                        resolvedDonViId = vdvDonViDict[firstMatch];
                    }
                }

                string? resolvedTenDonVi = null;
                if (resolvedDonViId.HasValue && donViDict.TryGetValue(resolvedDonViId.Value, out var tenDv))
                {
                    resolvedTenDonVi = tenDv;
                }
                else if (d.Doi?.DonVi != null)
                {
                    resolvedTenDonVi = d.Doi.DonVi.Ten;
                }

                result.Add(new DangKyThiDauDto
                {
                    Id = d.Id,
                    GiaiDauMonTheThaoId = d.GiaiDauMonTheThaoId,
                    GiaiDauId = gd?.Id,
                    TenGiaiDau = gd?.Ten,
                    MonTheThaoId = mon?.Id,
                    TenMonTheThao = mon?.Ten,
                    DoiId = d.DoiId,
                    TenDoi = d.Doi?.Ten,
                    DonViId = resolvedDonViId,
                    TenDonVi = resolvedTenDonVi,
                    SoDangKy = d.SoDangKy,
                    TenDangKy = d.TenDangKy ?? d.SoDangKy,
                    TrangThai = d.TrangThai,
                    NgayDangKy = d.NgayDangKy,
                    GhiChu = d.GhiChu,
                    SoVdv = vdvsInEntry.Count,
                    VanDongVienIds = vdvsInEntry,
                    VanDongVienNames = vdvNames,
                    ThanhVienDois = memberDtos,
                    Created = d.Created,
                    LastModified = d.LastModified
                });
            }

            return result;
        }

        /// <summary>
        /// Chuẩn hóa chuỗi giới tính để so sánh chính xác giữa môn thể thao và vận động viên (Nam, Nu, HonHop)
        /// </summary>
        /// <param name="gioiTinh">Chuỗi giới tính đầu vào (Nam, Nu, Nữ, Male, Female...)</param>
        /// <returns>Chuỗi giới tính chuẩn hóa: "Nam", "Nu", hoặc "HonHop"</returns>
        private static string NormalizeGender(string? gioiTinh)
        {
            if (string.IsNullOrWhiteSpace(gioiTinh)) return "HonHop";
            var g = gioiTinh.Trim().ToLowerInvariant();
            if (g == "nam" || g == "male" || g == "1") return "Nam";
            if (g == "nu" || g == "nữ" || g == "female" || g == "0") return "Nu";
            return "HonHop";
        }
    }
}
