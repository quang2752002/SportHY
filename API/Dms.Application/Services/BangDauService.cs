using Dms.Application.DTOs;
using Dms.Application.Interfaces;
using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Application.Services
{
    public class BangDauService : IBangDauService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BangDauService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BangDauDto>> GetAllAsync(int? giaiDauMonTheThaoId = null)
        {
            var paged = await _unitOfWork.BangDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 500,
                predicate: b => b.IsDeleted != true &&
                                (!giaiDauMonTheThaoId.HasValue || b.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value),
                orderBy: q => q.OrderBy(b => b.ThuTu).ThenBy(b => b.Ten),
                b => b.GiaiDauMonTheThao,
                b => b.ThanhVienBangs
            );

            var bangDauList = paged.Items.ToList();
            var dtos = new List<BangDauDto>();

            // Lấy thêm thông tin DangKyThiDau để hiển thị tên đội / đơn vị
            var dangKyList = await _unitOfWork.DangKyThiDaus.GetPagedAsync(
                1, 1000,
                predicate: d => d.IsDeleted != true && (!giaiDauMonTheThaoId.HasValue || d.GiaiDauMonTheThaoId == giaiDauMonTheThaoId.Value),
                orderBy: null,
                d => d.Doi!,
                d => d.ChiTietDangKyThiDaus
            );
            var dangKyMap = dangKyList.Items.ToDictionary(d => d.Id);

            foreach (var b in bangDauList)
            {
                var members = b.ThanhVienBangs.Where(m => m.IsDeleted != true).OrderBy(m => m.HatGiong ?? 999).ThenBy(m => m.Id).ToList();
                var memberDtos = new List<ThanhVienBangDto>();

                foreach (var m in members)
                {
                    dangKyMap.TryGetValue(m.DangKyThiDauId, out var dk);
                    string? tenDoi = dk?.Doi?.Ten;
                    string? tenDonVi = dk?.Doi?.DonVi?.Ten;

                    memberDtos.Add(new ThanhVienBangDto
                    {
                        Id = m.Id,
                        BangDauId = m.BangDauId,
                        DangKyThiDauId = m.DangKyThiDauId,
                        TenDangKy = dk?.TenDangKy ?? dk?.SoDangKy,
                        TenDoi = tenDoi ?? dk?.TenDangKy,
                        TenDonVi = tenDonVi,
                        HatGiong = m.HatGiong,
                        SoTran = m.SoTran,
                        SoThang = m.SoThang,
                        SoHoa = m.SoHoa,
                        SoThua = m.SoThua,
                        DiemGhiDuoc = m.DiemGhiDuoc,
                        DiemBiGhi = m.DiemBiGhi,
                        Diem = m.Diem,
                        XepHang = m.XepHang
                    });
                }

                dtos.Add(new BangDauDto
                {
                    Id = b.Id,
                    GiaiDauMonTheThaoId = b.GiaiDauMonTheThaoId,
                    TenMonTheThao = b.GiaiDauMonTheThao?.MonTheThao?.Ten,
                    Ma = b.Ma,
                    Ten = b.Ten,
                    ThuTu = b.ThuTu,
                    SoDoi = memberDtos.Count,
                    ThanhViens = memberDtos,
                    Created = b.Created,
                    LastModified = b.LastModified
                });
            }

            return dtos;
        }

        public async Task<BangDauDto?> GetByIdAsync(int id)
        {
            var paged = await _unitOfWork.BangDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: b => b.Id == id && b.IsDeleted != true,
                orderBy: null,
                b => b.GiaiDauMonTheThao,
                b => b.ThanhVienBangs
            );

            var b = paged.Items.FirstOrDefault();
            if (b == null) return null;

            var list = await GetAllAsync(b.GiaiDauMonTheThaoId);
            return list.FirstOrDefault(x => x.Id == id);
        }

        public async Task<BangDauDto> CreateAsync(CreateUpdateBangDauDto dto, string? createdBy = null)
        {
            var entity = new BangDau
            {
                GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                Ma = string.IsNullOrWhiteSpace(dto.Ma) ? $"BANG_{Guid.NewGuid():N}".Substring(0, 10).ToUpper() : dto.Ma,
                Ten = dto.Ten.Trim(),
                ThuTu = dto.ThuTu,
                Created = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsDeleted = false
            };

            await _unitOfWork.BangDaus.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            if (dto.DangKyThiDauIds != null && dto.DangKyThiDauIds.Any())
            {
                foreach (var dkId in dto.DangKyThiDauIds.Distinct())
                {
                    await _unitOfWork.ThanhVienBangs.AddAsync(new ThanhVienBang
                    {
                        BangDauId = entity.Id,
                        DangKyThiDauId = dkId,
                        Created = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        IsDeleted = false
                    });
                }
                await _unitOfWork.CompleteAsync();
            }

            return (await GetByIdAsync(entity.Id))!;
        }

        public async Task<BangDauDto?> UpdateAsync(int id, CreateUpdateBangDauDto dto, string? updatedBy = null)
        {
            var paged = await _unitOfWork.BangDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: b => b.Id == id && b.IsDeleted != true,
                orderBy: null,
                b => b.ThanhVienBangs
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.Ma)) entity.Ma = dto.Ma;
            if (!string.IsNullOrWhiteSpace(dto.Ten)) entity.Ten = dto.Ten.Trim();
            entity.ThuTu = dto.ThuTu;
            entity.LastModified = DateTime.UtcNow;
            entity.LastModifiedBy = updatedBy;

            _unitOfWork.BangDaus.Update(entity);

            if (dto.DangKyThiDauIds != null)
            {
                // Cập nhật lại thành viên bảng
                var oldMembers = (await _unitOfWork.ThanhVienBangs.FindAsync(m => m.BangDauId == id)).ToList();
                foreach (var m in oldMembers)
                {
                    _unitOfWork.ThanhVienBangs.Delete(m);
                }

                foreach (var dkId in dto.DangKyThiDauIds.Distinct())
                {
                    await _unitOfWork.ThanhVienBangs.AddAsync(new ThanhVienBang
                    {
                        BangDauId = id,
                        DangKyThiDauId = dkId,
                        Created = DateTime.UtcNow,
                        CreatedBy = updatedBy,
                        IsDeleted = false
                    });
                }
            }

            await _unitOfWork.CompleteAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var paged = await _unitOfWork.BangDaus.GetPagedAsync(
                pageIndex: 1,
                pageSize: 1,
                predicate: b => b.Id == id && b.IsDeleted != true
            );

            var entity = paged.Items.FirstOrDefault();
            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.LastModified = DateTime.UtcNow;
            _unitOfWork.BangDaus.Update(entity);

            // Xóa mềm các thành viên trong bảng
            var members = await _unitOfWork.ThanhVienBangs.FindAsync(m => m.BangDauId == id);
            foreach (var m in members)
            {
                m.IsDeleted = true;
                _unitOfWork.ThanhVienBangs.Update(m);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> AssignTeamsAsync(AssignTeamsToBangDto dto, string? updatedBy = null)
        {
            var bang = (await _unitOfWork.BangDaus.FindAsync(b => b.Id == dto.BangDauId && b.IsDeleted != true)).FirstOrDefault();
            if (bang == null) return false;

            var existingMembers = (await _unitOfWork.ThanhVienBangs.FindAsync(m => m.BangDauId == dto.BangDauId)).ToList();
            foreach (var m in existingMembers)
            {
                _unitOfWork.ThanhVienBangs.Delete(m);
            }

            foreach (var dkId in dto.DangKyThiDauIds.Distinct())
            {
                await _unitOfWork.ThanhVienBangs.AddAsync(new ThanhVienBang
                {
                    BangDauId = dto.BangDauId,
                    DangKyThiDauId = dkId,
                    Created = DateTime.UtcNow,
                    CreatedBy = updatedBy,
                    IsDeleted = false
                });
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<BangDauDto>> AutoDistributeAsync(AutoDistributeBangDto dto, string? createdBy = null)
        {
            if (dto.SoBang <= 0) dto.SoBang = 2;

            // 1. Lấy danh sách đăng ký đã duyệt của môn thi đấu
            var dangKyList = (await _unitOfWork.DangKyThiDaus.FindAsync(
                d => d.GiaiDauMonTheThaoId == dto.GiaiDauMonTheThaoId && d.IsDeleted != true
            )).ToList();

            if (!dangKyList.Any()) return new List<BangDauDto>();

            // 2. Xóa các bảng cũ của môn thi đấu này nếu có
            var oldBangs = (await _unitOfWork.BangDaus.FindAsync(b => b.GiaiDauMonTheThaoId == dto.GiaiDauMonTheThaoId)).ToList();
            foreach (var b in oldBangs)
            {
                var oldMembers = (await _unitOfWork.ThanhVienBangs.FindAsync(m => m.BangDauId == b.Id)).ToList();
                foreach (var m in oldMembers)
                {
                    _unitOfWork.ThanhVienBangs.Delete(m);
                }
                _unitOfWork.BangDaus.Delete(b);
            }
            await _unitOfWork.CompleteAsync();

            // 3. Tạo các bảng mới (A, B, C, D...)
            var newBangs = new List<BangDau>();
            for (int i = 0; i < dto.SoBang; i++)
            {
                char groupChar = (char)('A' + i);
                var bang = new BangDau
                {
                    GiaiDauMonTheThaoId = dto.GiaiDauMonTheThaoId,
                    Ma = $"BANG_{groupChar}",
                    Ten = $"{dto.TienToBang}{groupChar}",
                    ThuTu = i + 1,
                    Created = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    IsDeleted = false
                };
                await _unitOfWork.BangDaus.AddAsync(bang);
                newBangs.Add(bang);
            }
            await _unitOfWork.CompleteAsync();

            // 4. Phân bổ các đội ngẫu nhiên / lần lượt vào các bảng
            var shuffledTeams = dangKyList.OrderBy(_ => Guid.NewGuid()).ToList();
            for (int i = 0; i < shuffledTeams.Count; i++)
            {
                var targetBang = newBangs[i % newBangs.Count];
                await _unitOfWork.ThanhVienBangs.AddAsync(new ThanhVienBang
                {
                    BangDauId = targetBang.Id,
                    DangKyThiDauId = shuffledTeams[i].Id,
                    Created = DateTime.UtcNow,
                    CreatedBy = createdBy,
                    IsDeleted = false
                });
            }
            await _unitOfWork.CompleteAsync();

            return await GetAllAsync(dto.GiaiDauMonTheThaoId);
        }
    }
}
