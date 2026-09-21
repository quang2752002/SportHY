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
                                (!donViId.HasValue || (d.Doi != null && d.Doi.DonViId == donViId.Value) || d.ChiTietDangKyThiDaus.Any(c => c.VanDongVien.DonViId == donViId.Value)) &&
                                (!giaiDauId.HasValue || d.GiaiDauMonTheThao.GiaiDauId == giaiDauId.Value),
                orderBy: q => q.OrderByDescending(d => d.NgayDangKy),
                d => d.GiaiDauMonTheThao,
                d => d.GiaiDauMonTheThao.GiaiDau,
                d => d.GiaiDauMonTheThao.MonTheThao,
                d => d.Doi!,
                d => d.ChiTietDangKyThiDaus
            );

            var dtos = await MapToRichDtosAsync(pagedEntities.Items);
            return new PagedResult<DangKyThiDauDto>(dtos, pagedEntities.TotalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<DangKyThiDauDto>> GetAllAsync(int? giaiDauId = null, int? giaiDauMonTheThaoId = null, int? donViId = null)
        {
            var paged = await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1,
                1000,
                predicate: d => d.IsDeleted != true &&
                                (!giaiDauMonTheThaoId.HasValue || d.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value) &&
                                (!donViId.HasValue || (d.Doi != null && d.Doi.DonViId == donViId.Value) || d.ChiTietDangKyThiDaus.Any(c => c.VanDongVien.DonViId == donViId.Value)) &&
                                (!giaiDauId.HasValue || d.GiaiDauMonTheThao.GiaiDauId == giaiDauId.Value),
                orderBy: q => q.OrderByDescending(d => d.NgayDangKy),
                d => d.GiaiDauMonTheThao,
                d => d.GiaiDauMonTheThao.GiaiDau,
                d => d.GiaiDauMonTheThao.MonTheThao,
                d => d.Doi!,
                d => d.ChiTietDangKyThiDaus
            );

            return await MapToRichDtosAsync(paged.Items);
        }

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
                d => d.ChiTietDangKyThiDaus
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return null;

            var dtos = await MapToRichDtosAsync(new List<DangKyThiDau> { entity });
            return dtos.FirstOrDefault();
        }

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

            // Kiểm tra trùng VĐV đã đăng ký trong môn thi đấu này của giải đấu
            var activeRegistrations = (await _unitOfWork.DangKyThiDaus.FindAsync(
                d => d.GiaiDauMonTheThaoId == dto.GiaiDauMonTheThaoId &&
                     d.IsDeleted != true &&
                     d.TrangThai != "TuChoi"
            )).ToList();

            var activeRegIds = activeRegistrations.Select(r => r.Id).ToList();
            if (activeRegIds.Any())
            {
                var registeredDetails = await _unitOfWork.ChiTietDangKyThiDaus.FindAsync(
                    c => activeRegIds.Contains(c.DangKyThiDauId) &&
                         c.IsDeleted != true &&
                         vdvIds.Contains(c.VanDongVienId)
                );
                var dupVdvIds = registeredDetails.Select(c => c.VanDongVienId).Distinct().ToList();

                var activeDoiIds = activeRegistrations.Where(r => r.DoiId.HasValue).Select(r => r.DoiId!.Value).Distinct().ToList();
                if (activeDoiIds.Any())
                {
                    var teamMembers = await _unitOfWork.ThanhVienDois.FindAsync(
                        tv => activeDoiIds.Contains(tv.DoiId) &&
                              tv.IsDeleted != true &&
                              vdvIds.Contains(tv.VanDongVienId)
                    );
                    dupVdvIds = dupVdvIds.Union(teamMembers.Select(tv => tv.VanDongVienId)).Distinct().ToList();
                }

                if (dupVdvIds.Any())
                {
                    var dupVdvs = (await _unitOfWork.VanDongViens.FindAsync(v => dupVdvIds.Contains(v.Id))).ToList();
                    var dupNames = string.Join(", ", dupVdvs.Select(v => v.HoTen));
                    throw new InvalidOperationException($"Vận động viên [{dupNames}] đã được đăng ký tham gia môn thi đấu này. Không thể đăng ký trùng.");
                }
            }

            int? resolvedDoiId = dto.DoiId;

            // Tự động tạo Doi + ThanhVienDoi nếu chưa có DoiId
            if (!resolvedDoiId.HasValue && vdvIds.Any())
            {
                var vdvEntities = (await _unitOfWork.VanDongViens.FindAsync(v => vdvIds.Contains(v.Id))).ToList();
                int? donViId = dto.DonViId ?? vdvEntities.FirstOrDefault(v => v.DonViId.HasValue)?.DonViId;

                string tenDoi = dto.TenDoi?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(tenDoi))
                {
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

            var entity = new DangKyThiDau
            {
                GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                DoiId = resolvedDoiId,
                SoDangKy = string.IsNullOrWhiteSpace(dto.SoDangKy) ? $"DK_{DateTime.Now:yyyyMMddHHmmss}" : dto.SoDangKy,
                TenDangKy = !string.IsNullOrWhiteSpace(dto.TenDangKy)
                    ? dto.TenDangKy
                    : (!string.IsNullOrWhiteSpace(dto.TenDoi) ? dto.TenDoi : $"Tham gia - {monTheThao?.Ten ?? "Môn thi đấu"}"),
                TrangThai = "DaDuyet", // Luôn mặc định đã duyệt theo yêu cầu của hệ thống
                NgayDangKy = dto.NgayDangKy != default ? dto.NgayDangKy : DateTime.Now,
                GhiChu = dto.GhiChu,
                CreatedBy = createdBy,
                Created = DateTime.Now,
                IsDeleted = false
            };

            await _unitOfWork.DangKyThiDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            // Thêm ChiTietDangKyThiDau (liên kết VĐV trực tiếp vào hồ sơ)
            int stt = 1;
            foreach (var vdvId in vdvIds)
            {
                await _unitOfWork.ChiTietDangKyThiDaus.AddAsync(new ChiTietDangKyThiDau
                {
                    DangKyThiDauId = entity.Id,
                    VanDongVienId = vdvId,
                    SoThuTu = stt++,
                    VaiTro = stt == 2 && vdvIds.Count > 1 ? "Đội trưởng" : "Vận động viên thi đấu",
                    CreatedBy = createdBy,
                    Created = DateTime.Now
                });
            }
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

                var activeRegIds = activeRegistrations.Select(r => r.Id).ToList();
                if (activeRegIds.Any())
                {
                    var newVdvIds = dto.VanDongVienIds.Distinct().ToList();
                    var registeredDetails = await _unitOfWork.ChiTietDangKyThiDaus.FindAsync(
                        c => activeRegIds.Contains(c.DangKyThiDauId) &&
                             c.IsDeleted != true &&
                             newVdvIds.Contains(c.VanDongVienId)
                    );
                    var dupVdvIds = registeredDetails.Select(c => c.VanDongVienId).Distinct().ToList();

                    var activeDoiIds = activeRegistrations.Where(r => r.DoiId.HasValue).Select(r => r.DoiId!.Value).Distinct().ToList();
                    if (activeDoiIds.Any())
                    {
                        var teamMembers = await _unitOfWork.ThanhVienDois.FindAsync(
                            tv => activeDoiIds.Contains(tv.DoiId) &&
                                  tv.IsDeleted != true &&
                                  newVdvIds.Contains(tv.VanDongVienId)
                        );
                        dupVdvIds = dupVdvIds.Union(teamMembers.Select(tv => tv.VanDongVienId)).Distinct().ToList();
                    }

                    if (dupVdvIds.Any())
                    {
                        var dupVdvs = (await _unitOfWork.VanDongViens.FindAsync(v => dupVdvIds.Contains(v.Id))).ToList();
                        var dupNames = string.Join(", ", dupVdvs.Select(v => v.HoTen));
                        throw new InvalidOperationException($"Vận động viên [{dupNames}] đã được đăng ký tham gia môn thi đấu này. Không thể đăng ký trùng.");
                    }
                }

                var existingDetails = await _unitOfWork.ChiTietDangKyThiDaus.FindAsync(c => c.DangKyThiDauId == id);
                foreach (var d in existingDetails)
                {
                    _unitOfWork.ChiTietDangKyThiDaus.Delete(d);
                }

                int stt = 1;
                foreach (var vdvId in dto.VanDongVienIds)
                {
                    await _unitOfWork.ChiTietDangKyThiDaus.AddAsync(new ChiTietDangKyThiDau
                    {
                        DangKyThiDauId = id,
                        VanDongVienId = vdvId,
                        SoThuTu = stt++,
                        VaiTro = "Vận động viên thi đấu",
                        CreatedBy = updatedBy,
                        Created = DateTime.Now
                    });
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

            // Xóa mềm ChiTietDangKyThiDau
            var details = await _unitOfWork.ChiTietDangKyThiDaus.FindAsync(c => c.DangKyThiDauId == id);
            foreach (var d in details)
            {
                d.IsDeleted = true;
                _unitOfWork.ChiTietDangKyThiDaus.Update(d);
            }

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

        private async Task<List<DangKyThiDauDto>> MapToRichDtosAsync(IEnumerable<DangKyThiDau> items)
        {
            var list = items.ToList();
            if (!list.Any()) return new List<DangKyThiDauDto>();

            var allVdvIds = list.SelectMany(d => d.ChiTietDangKyThiDaus.Select(c => c.VanDongVienId)).Distinct().ToList();
            var vdvEntities = allVdvIds.Any()
                ? (await _unitOfWork.VanDongViens.FindAsync(v => allVdvIds.Contains(v.Id))).ToList()
                : new List<VanDongVien>();
            var vdvDict = vdvEntities.ToDictionary(v => v.Id, v => v.HoTen);
            var vdvDonViDict = vdvEntities.Where(v => v.DonViId.HasValue).ToDictionary(v => v.Id, v => v.DonViId!.Value);

            var result = new List<DangKyThiDauDto>();
            foreach (var d in list)
            {
                var gd = d.GiaiDauMonTheThao?.GiaiDau;
                var mon = d.GiaiDauMonTheThao?.MonTheThao;
                var vdvsInEntry = d.ChiTietDangKyThiDaus.Select(c => c.VanDongVienId).ToList();
                var vdvNames = vdvsInEntry
                    .Where(id => vdvDict.ContainsKey(id))
                    .Select(id => vdvDict[id])
                    .ToList();

                int? resolvedDonViId = d.Doi?.DonViId;
                if (!resolvedDonViId.HasValue && vdvsInEntry.Any())
                {
                    var firstMatch = vdvsInEntry.FirstOrDefault(id => vdvDonViDict.ContainsKey(id));
                    if (firstMatch != 0)
                    {
                        resolvedDonViId = vdvDonViDict[firstMatch];
                    }
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
                    SoDangKy = d.SoDangKy,
                    TenDangKy = d.TenDangKy ?? d.SoDangKy,
                    TrangThai = d.TrangThai,
                    NgayDangKy = d.NgayDangKy,
                    GhiChu = d.GhiChu,
                    SoVdv = vdvsInEntry.Count,
                    VanDongVienIds = vdvsInEntry,
                    VanDongVienNames = vdvNames,
                    Created = d.Created,
                    LastModified = d.LastModified
                });
            }

            return result;
        }
    }
}
