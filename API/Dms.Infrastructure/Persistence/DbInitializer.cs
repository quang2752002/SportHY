using Dms.Application.Common;
using Dms.Domain.Entities;
using Dms.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dms.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            // 1. Đảm bảo schema migration và các cột mở rộng đã sẵn sàng
            try
            {
                await context.Database.MigrateAsync();
            }
            catch { }

            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MonTheThao') AND name = 'LoaiThiDau')
                    BEGIN
                        ALTER TABLE MonTheThao ADD LoaiThiDau NVARCHAR(30) NULL;
                    END

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GiaiDau') AND name = 'HanDangKy')
                    BEGIN
                        ALTER TABLE GiaiDau ADD HanDangKy DATETIME2 NULL;
                    END

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GiaiDau') AND name = 'TruongBanTrongTaiId')
                    BEGIN
                        ALTER TABLE GiaiDau ADD TruongBanTrongTaiId INT NULL;
                    END

                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GiaiDauMonTheThao') AND name = 'NguoiDieuHanhId')
                    BEGIN
                        ALTER TABLE GiaiDauMonTheThao ADD NguoiDieuHanhId INT NULL;
                    END
                ");
            }
            catch { }

            // 2. Khởi tạo các Role mặc định theo hệ thống
            string[] roleNames = AppRoles.AllRoles;
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // Gán Permissions cho Role Admin
            var adminRole = await roleManager.FindByNameAsync(AppRoles.Admin);
            if (adminRole != null)
            {
                var existingClaims = await roleManager.GetClaimsAsync(adminRole);
                var allPermissions = Permissions.GetAllPermissions();
                foreach (var permission in allPermissions)
                {
                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim("permission", permission));
                    }
                }
            }

            // 3. Khởi tạo các Đơn Vị (4 đơn vị đại học lớn)
            var khoiTruong = await context.Khois.FirstOrDefaultAsync(k => k.Ma == "KHOI_DAIHOC");
            if (khoiTruong == null)
            {
                khoiTruong = new Khoi
                {
                    Ma = "KHOI_DAIHOC",
                    Ten = "Khối Các Trường Đại Học & Học Viện",
                    MoTa = "Các trường Đại học, Học viện trên toàn quốc",
                    TrangThai = true
                };
                await context.Khois.AddAsync(khoiTruong);
                await context.SaveChangesAsync();
            }

            var dvBk = await context.DonVis.FirstOrDefaultAsync(d => d.Ma == "DV_BK");
            if (dvBk == null)
            {
                dvBk = new DonVi { Ma = "DV_BK", Ten = "Đại học Bách Khoa Hà Nội", KhoiId = khoiTruong.Id, LoaiDonVi = "TruongHoc", DiaChi = "Số 1 Đại Cồ Việt, Hai Bà Trưng, Hà Nội", NguoiDaiDien = "Nguyễn Văn Hùng", SoDienThoai = "0901234567", Email = "sport@hust.edu.vn", TrangThai = true };
                await context.DonVis.AddAsync(dvBk);
            }

            var dvHvtc = await context.DonVis.FirstOrDefaultAsync(d => d.Ma == "DV_HVTC");
            if (dvHvtc == null)
            {
                dvHvtc = new DonVi { Ma = "DV_HVTC", Ten = "Học Viện Tài Chính", KhoiId = khoiTruong.Id, LoaiDonVi = "TruongHoc", DiaChi = "Số 58 Lê Văn Hiến, Bắc Từ Liêm, Hà Nội", NguoiDaiDien = "Trần Thị Mai", SoDienThoai = "0902345678", Email = "sport@hvtc.edu.vn", TrangThai = true };
                await context.DonVis.AddAsync(dvHvtc);
            }

            var dvSphn = await context.DonVis.FirstOrDefaultAsync(d => d.Ma == "DV_SPHN");
            if (dvSphn == null)
            {
                dvSphn = new DonVi { Ma = "DV_SPHN", Ten = "Đại học Sư Phạm Hà Nội", KhoiId = khoiTruong.Id, LoaiDonVi = "TruongHoc", DiaChi = "136 Xuân Thủy, Cầu Giấy, Hà Nội", NguoiDaiDien = "Lê Hoàng Quân", SoDienThoai = "0903456789", Email = "sport@hnue.edu.vn", TrangThai = true };
                await context.DonVis.AddAsync(dvSphn);
            }

            var dvQghn = await context.DonVis.FirstOrDefaultAsync(d => d.Ma == "DV_QGHN");
            if (dvQghn == null)
            {
                dvQghn = new DonVi { Ma = "DV_QGHN", Ten = "Đại học Quốc Gia Hà Nội", KhoiId = khoiTruong.Id, LoaiDonVi = "TruongHoc", DiaChi = "144 Xuân Thủy, Cầu Giấy, Hà Nội", NguoiDaiDien = "Phạm Quốc Tuấn", SoDienThoai = "0904567890", Email = "sport@vnu.edu.vn", TrangThai = true };
                await context.DonVis.AddAsync(dvQghn);
            }
            await context.SaveChangesAsync();

            // 4. Khởi tạo danh sách Trọng Tài trong bảng TrongTai
            var ttA = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_A");
            if (ttA == null)
            {
                ttA = new TrongTai { Ma = "TT_A", HoTen = "Tài A (Trưởng ban)", GioiTinh = "Nam", SoDienThoai = "0988000001", Email = "taia@sport.vn", CapBac = "Trọng tài Quốc gia", TrangThai = true };
                await context.TrongTais.AddAsync(ttA);
            }

            var ttB = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_B");
            if (ttB == null)
            {
                ttB = new TrongTai { Ma = "TT_B", HoTen = "Trọng tài B", GioiTinh = "Nam", SoDienThoai = "0988000002", Email = "taib@sport.vn", CapBac = "Trọng tài cấp 1", TrangThai = true };
                await context.TrongTais.AddAsync(ttB);
            }

            var ttC = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_C");
            if (ttC == null)
            {
                ttC = new TrongTai { Ma = "TT_C", HoTen = "Trọng tài C", GioiTinh = "Nam", SoDienThoai = "0988000003", Email = "taic@sport.vn", CapBac = "Trọng tài cấp 1", TrangThai = true };
                await context.TrongTais.AddAsync(ttC);
            }

            var ttD = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_D");
            if (ttD == null)
            {
                ttD = new TrongTai { Ma = "TT_D", HoTen = "Trọng tài D", GioiTinh = "Nam", SoDienThoai = "0988000004", Email = "taid@sport.vn", CapBac = "Trọng tài cấp 2", TrangThai = true };
                await context.TrongTais.AddAsync(ttD);
            }

            var ttE = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_E");
            if (ttE == null)
            {
                ttE = new TrongTai { Ma = "TT_E", HoTen = "Trọng tài E", GioiTinh = "Nam", SoDienThoai = "0988000005", Email = "taie@sport.vn", CapBac = "Trọng tài cấp 2", TrangThai = true };
                await context.TrongTais.AddAsync(ttE);
            }

            var ttF = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_F");
            if (ttF == null)
            {
                ttF = new TrongTai { Ma = "TT_F", HoTen = "Trọng tài F", GioiTinh = "Nam", SoDienThoai = "0988000006", Email = "taif@sport.vn", CapBac = "Trọng tài cấp 2", TrangThai = true };
                await context.TrongTais.AddAsync(ttF);
            }

            var ttTrong = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_TRONG");
            if (ttTrong == null)
            {
                ttTrong = new TrongTai { Ma = "TT_TRONG", HoTen = "Trần Văn Trọng", GioiTinh = "Nam", SoDienThoai = "0988000007", Email = "trong@sport.vn", CapBac = "Trọng tài cấp 1", TrangThai = true };
                await context.TrongTais.AddAsync(ttTrong);
            }

            var ttHanh = await context.TrongTais.FirstOrDefaultAsync(t => t.Ma == "TT_HANH");
            if (ttHanh == null)
            {
                ttHanh = new TrongTai { Ma = "TT_HANH", HoTen = "Lê Thị Bích Hạnh", GioiTinh = "Nu", SoDienThoai = "0988000008", Email = "hanh@sport.vn", CapBac = "Giám sát trọng tài FIFA", TrangThai = true };
                await context.TrongTais.AddAsync(ttHanh);
            }
            await context.SaveChangesAsync();

            // 5. Khởi tạo danh sách Tài Khoản Users đăng nhập
            var seedUsers = new[]
            {
                new { Username = "admin", Email = "admin@sport.vn", Name = "Quản trị viên hệ thống", Pass = "Admin@123", Roles = new[] { AppRoles.Admin }, DonViId = (int?)null, TrongTaiId = (int?)null },
                new { Username = "manager", Email = "manager@sport.vn", Name = "Nguyễn Văn Quản Lý", Pass = "Manager@123", Roles = new[] { AppRoles.Manager }, DonViId = (int?)null, TrongTaiId = (int?)null },
                
                // 4 Đơn vị
                new { Username = "donvi_bk", Email = "bk@hust.edu.vn", Name = "Đoàn ĐH Bách Khoa Hà Nội", Pass = "Delegation@123", Roles = new[] { AppRoles.Delegation }, DonViId = (int?)dvBk.Id, TrongTaiId = (int?)null },
                new { Username = "donvi_hvtc", Email = "tc@hvtc.edu.vn", Name = "Đoàn Học Viện Tài Chính", Pass = "Delegation@123", Roles = new[] { AppRoles.Delegation }, DonViId = (int?)dvHvtc.Id, TrongTaiId = (int?)null },
                new { Username = "donvi_sphn", Email = "sp@hnue.edu.vn", Name = "Đoàn ĐH Sư Phạm Hà Nội", Pass = "Delegation@123", Roles = new[] { AppRoles.Delegation }, DonViId = (int?)dvSphn.Id, TrongTaiId = (int?)null },
                new { Username = "donvi_qghn", Email = "qg@vnu.edu.vn", Name = "Đoàn ĐH Quốc Gia Hà Nội", Pass = "Delegation@123", Roles = new[] { AppRoles.Delegation }, DonViId = (int?)dvQghn.Id, TrongTaiId = (int?)null },

                // Trưởng ban & Trọng tài
                new { Username = "taia", Email = "taia@sport.vn", Name = "Tài A (Trưởng ban trọng tài)", Pass = "123", Roles = new[] { AppRoles.HeadReferee, AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttA.Id },
                new { Username = "taib", Email = "taib@sport.vn", Name = "Trọng tài B", Pass = "123", Roles = new[] { AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttB.Id },
                new { Username = "taic", Email = "taic@sport.vn", Name = "Trọng tài C", Pass = "123", Roles = new[] { AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttC.Id },
                new { Username = "taid", Email = "taid@sport.vn", Name = "Trọng tài D", Pass = "123", Roles = new[] { AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttD.Id },
                new { Username = "taie", Email = "taie@sport.vn", Name = "Trọng tài E", Pass = "123", Roles = new[] { AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttE.Id },
                new { Username = "taif", Email = "taif@sport.vn", Name = "Trọng tài F", Pass = "123", Roles = new[] { AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttF.Id },
                new { Username = "trong", Email = "trong@sport.vn", Name = "Trần Văn Trọng", Pass = "123", Roles = new[] { AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttTrong.Id },
                new { Username = "hanh", Email = "hanh@sport.vn", Name = "Lê Thị Bích Hạnh (Trọng tài)", Pass = "123", Roles = new[] { AppRoles.Referee }, DonViId = (int?)null, TrongTaiId = (int?)ttHanh.Id },
                new { Username = "dieuhanh", Email = "dieuhanh@sport.vn", Name = "Người điều hành môn A", Pass = "123", Roles = new[] { AppRoles.SportCoordinator }, DonViId = (int?)null, TrongTaiId = (int?)null },
            };

            foreach (var u in seedUsers)
            {
                var userEntity = await userManager.FindByNameAsync(u.Username);
                if (userEntity == null)
                {
                    userEntity = new ApplicationUser
                    {
                        UserName = u.Username,
                        Email = u.Email,
                        FullName = u.Name,
                        EmailConfirmed = true,
                        DonViId = u.DonViId,
                        TrongTaiId = u.TrongTaiId,
                        CreatedAt = DateTime.UtcNow
                    };
                    var createResult = await userManager.CreateAsync(userEntity, u.Pass);
                    if (createResult.Succeeded)
                    {
                        foreach (var role in u.Roles)
                        {
                            await userManager.AddToRoleAsync(userEntity, role);
                        }
                    }
                }
                else
                {
                    // Cập nhật lại mapping DonViId / TrongTaiId nếu chưa có
                    userEntity.DonViId = u.DonViId;
                    userEntity.TrongTaiId = u.TrongTaiId;
                    userEntity.FullName = u.Name;
                    await userManager.UpdateAsync(userEntity);

                    foreach (var role in u.Roles)
                    {
                        if (!await userManager.IsInRoleAsync(userEntity, role))
                        {
                            await userManager.AddToRoleAsync(userEntity, role);
                        }
                    }
                }
            }

            var refereeHanhAccount = await userManager.FindByNameAsync("hanh");
            if (refereeHanhAccount != null && await userManager.IsInRoleAsync(refereeHanhAccount, AppRoles.SportCoordinator))
            {
                await userManager.RemoveFromRoleAsync(refereeHanhAccount, AppRoles.SportCoordinator);
            }

            // Bảo đảm mỗi tài khoản Người điều hành môn có hồ sơ nghiệp vụ riêng.
            var coordinatorAccounts = await userManager.GetUsersInRoleAsync(AppRoles.SportCoordinator);
            foreach (var coordinatorAccount in coordinatorAccounts)
            {
                var coordinatorProfile = coordinatorAccount.NguoiDieuHanhMonId.HasValue
                    ? await context.NguoiDieuHanhMons.FirstOrDefaultAsync(profile => profile.Id == coordinatorAccount.NguoiDieuHanhMonId.Value)
                    : null;

                if (coordinatorProfile == null)
                {
                    coordinatorProfile = new NguoiDieuHanhMon
                    {
                        Ma = $"NDH-{coordinatorAccount.Id:D6}",
                        HoTen = string.IsNullOrWhiteSpace(coordinatorAccount.FullName) ? coordinatorAccount.UserName ?? "Người điều hành môn" : coordinatorAccount.FullName,
                        Email = coordinatorAccount.Email,
                        SoDienThoai = coordinatorAccount.PhoneNumber,
                        TrangThai = true,
                        IsDeleted = false,
                        CreatedBy = "DbInitializer"
                    };
                    await context.NguoiDieuHanhMons.AddAsync(coordinatorProfile);
                    await context.SaveChangesAsync();
                    coordinatorAccount.NguoiDieuHanhMonId = coordinatorProfile.Id;
                    await userManager.UpdateAsync(coordinatorAccount);
                }
                else
                {
                    coordinatorProfile.HoTen = string.IsNullOrWhiteSpace(coordinatorAccount.FullName) ? coordinatorAccount.UserName ?? coordinatorProfile.HoTen : coordinatorAccount.FullName;
                    coordinatorProfile.Email = coordinatorAccount.Email;
                    coordinatorProfile.SoDienThoai = coordinatorAccount.PhoneNumber;
                    coordinatorProfile.TrangThai = true;
                    coordinatorProfile.IsDeleted = false;
                    await context.SaveChangesAsync();
                }
            }

            // 6. Khởi tạo 20 Vận Động Viên cho MỖI đơn vị (Tổng cộng 80 VĐV cho 4 trường)
            var existingVdvs = await context.VanDongViens.ToListAsync();
            var units = new[]
            {
                new {
                    DonVi = dvBk, Prefix = "BK",
                    Names = new[] {
                        "Nguyễn Văn Quang", "Lê Hồng Phát", "Đặng Hoàng Long", "Trần Minh Đức", "Vũ Quốc Bảo",
                        "Hoàng Tuấn Anh", "Đỗ Trọng Hiếu", "Bùi Đức Huy", "Phạm Gia Khiêm", "Nguyễn Hải Nam",
                        "Trương Minh Trí", "Phan Đình Luật", "Đinh Hữu Phước", "Lê Công Hoàng", "Ngô Thành Nam",
                        "Dương Anh Tuấn", "Vũ Khắc Việt", "Hồ Trọng Tấn", "Đỗ Viết Cường", "Tạ Quang Bách"
                    }
                },
                new {
                    DonVi = dvHvtc, Prefix = "TC",
                    Names = new[] {
                        "Phạm Minh Triết", "Võ Hoàng Nam", "Đinh Trọng Đạt", "Lê Khắc Thịnh", "Nguyễn Đình Quân",
                        "Vũ Tiến Đạt", "Phan Nhật Minh", "Chu Trọng Đại", "Trần Hữu Thắng", "Hoàng Quốc Việt",
                        "Lê Anh Tuấn", "Đỗ Thành Chung", "Phạm Hải Đăng", "Bùi Khắc Tiệp", "Nguyễn Hoàng Giang",
                        "Vũ Minh Hiếu", "Trần Quốc Toản", "Đặng Đình Bách", "Ngô Văn Hậu", "Lý Gia Hưng"
                    }
                },
                new {
                    DonVi = dvSphn, Prefix = "SP",
                    Names = new[] {
                        "Nguyễn Tiến Dũng", "Hoàng Văn Khang", "Lê Minh Tuấn", "Trần Đình Duy", "Vũ Xuân Bách",
                        "Đoàn Công Vinh", "Nguyễn Quang Huy", "Bùi Xuân Trường", "Đỗ Việt Hoàng", "Trương Tuấn Kiệt",
                        "Phan Văn Đức", "Võ Nhật Anh", "Đặng Minh Tâm", "Hồ Văn Quân", "Trần Bảo Lâm",
                        "Lê Quốc Thái", "Bùi Hữu Đạt", "Nguyễn Duy Hưng", "Phạm Hoàng Sơn", "Vũ Anh Khoa"
                    }
                },
                new {
                    DonVi = dvQghn, Prefix = "QG",
                    Names = new[] {
                        "Trần Quang Minh", "Nguyễn Thành Đạt", "Lê Tuấn Hưng", "Vũ Hoàng Long", "Phạm Quốc Huy",
                        "Nguyễn Duy Anh", "Trịnh Đình Phong", "Đặng Tuấn Khang", "Hoàng Mạnh Hùng", "Lưu Việt Cường",
                        "Đỗ Quốc Bảo", "Phan Gia Huy", "Tô Văn Vũ", "Vũ Minh Quân", "Nguyễn Hữu Tài",
                        "Lê Đại Hành", "Trần Thế Bảo", "Đinh Văn Thanh", "Phạm Minh Đức", "Bùi Tuấn Ngọc"
                    }
                },
            };

            var allVdvs = new List<VanDongVien>();
            foreach (var u in units)
            {
                for (int i = 0; i < u.Names.Length; i++)
                {
                    string maVdv = $"VDV_{u.Prefix}_{i + 1:D2}";
                    var vdv = existingVdvs.FirstOrDefault(v => v.Ma == maVdv);
                    if (vdv == null)
                    {
                        vdv = new VanDongVien
                        {
                            Ma = maVdv,
                            HoTen = u.Names[i],
                            DonViId = u.DonVi.Id,
                            GioiTinh = "Nam",
                            NgaySinh = new DateTime(2003, 1 + (i % 12), 1 + (i * 2 % 28)),
                            SoDienThoai = $"09{u.DonVi.Id:D2}{i + 1:D2}{i + 1:D4}",
                            Email = $"{u.Prefix.ToLower()}{i + 1}@student.edu.vn",
                            TrangThai = true
                        };
                        await context.VanDongViens.AddAsync(vdv);
                    }
                    allVdvs.Add(vdv);
                }
            }
            await context.SaveChangesAsync();

            // 7. Tạo 1 Danh mục Môn & 1 Môn "Bóng đá 3 người"
            var dmBongDa = await context.DanhMucMonTheThaos.FirstOrDefaultAsync(d => d.Ma == "DM_BONGDA");
            if (dmBongDa == null)
            {
                dmBongDa = new DanhMucMonTheThao
                {
                    Ma = "DM_BONGDA",
                    Ten = "Bóng đá",
                    MoTa = "Bóng đá mini và các thể thức bóng đá phong trào",
                    TrangThai = true
                };
                await context.DanhMucMonTheThaos.AddAsync(dmBongDa);
                await context.SaveChangesAsync();
            }

            var monBongDa3 = await context.MonTheThaos.FirstOrDefaultAsync(m => m.Ma == "BONG_DA_3");
            if (monBongDa3 == null)
            {
                monBongDa3 = new MonTheThao
                {
                    DanhMucId = dmBongDa.Id,
                    Ma = "BONG_DA_3",
                    Ten = "Bóng đá sân 3 nam",
                    LaMonDongDoi = true,
                    LoaiThiDau = "DongDoi",
                    GioiTinh = "Nam",
                    SoLuongVanDongVienToiThieu = 3,
                    SoLuongVanDongVienToiDa = 5,
                    HinhThucThiDau = HinhThucThiDau.KetHopVongBangVaLoaiTrucTiep,
                    MoTa = "Bóng đá mini 3 người, thi đấu vòng bảng và loại trực tiếp",
                    TrangThai = true
                };
                await context.MonTheThaos.AddAsync(monBongDa3);
                await context.SaveChangesAsync();
            }

            // 8. Tạo Cấu hình thể thức thi đấu cho Bóng đá sân 3 nam
            var cauHinhTheThuc = await context.CauHinhTheThucThiDaus.FirstOrDefaultAsync(c => c.MonTheThaoId == monBongDa3.Id);
            if (cauHinhTheThuc == null)
            {
                cauHinhTheThuc = new CauHinhTheThucThiDau
                {
                    MonTheThaoId = monBongDa3.Id,
                    LoaiTheThuc = "ThoiGianHiep",
                    SoHiepToiDa = 2,
                    ThoiGianHiepChinhPhut = 15,
                    ChoPhepHoaVongBang = true,
                    ChoPhepHoaKnockout = false,
                    CoHiepPhu = false,
                    CoPenalty = true,
                    SoLuotPenaltyMoiDoi = 3,
                    CoThePhat = true,
                    DiemThang = 3,
                    DiemHoa = 1,
                    DiemThua = 0,
                    CachTinhDiemTheoSet = false,
                    TieuChiXepHangJson = "[\"Diem\",\"HieuSo\",\"DiemGhiDuoc\",\"DoiDau\",\"SoTranThang\"]"
                };
                await context.CauHinhTheThucThiDaus.AddAsync(cauHinhTheThuc);
                await context.SaveChangesAsync();
            }

            // 9. Tạo 1 Cụm Sân & Các Sân Đấu
            var cumSan = await context.CumSans.FirstOrDefaultAsync(c => c.Ma == "CS_MYDINH");
            if (cumSan == null)
            {
                cumSan = new CumSan
                {
                    Ma = "CS_MYDINH",
                    Ten = "Khu liên hợp thể thao Quốc gia Mỹ Đình",
                    DiaChi = "Đường Lê Đức Thọ, Nam Từ Liêm, Hà Nội",
                    SoLuongSan = 2,
                    TrangThai = true
                };
                await context.CumSans.AddAsync(cumSan);
                await context.SaveChangesAsync();
            }

            var san1 = await context.SanDaus.FirstOrDefaultAsync(s => s.Ma == "SAN_BD3_01");
            if (san1 == null)
            {
                san1 = new SanDau { CumSanId = cumSan.Id, Ma = "SAN_BD3_01", Ten = "Sân Bóng Đá 3 Người Số 1", LoaiSan = "SanBongDa", SoSan = 1, TrangThai = true };
                await context.SanDaus.AddAsync(san1);
            }

            var san2 = await context.SanDaus.FirstOrDefaultAsync(s => s.Ma == "SAN_BD3_02");
            if (san2 == null)
            {
                san2 = new SanDau { CumSanId = cumSan.Id, Ma = "SAN_BD3_02", Ten = "Sân Bóng Đá 3 Người Số 2", LoaiSan = "SanBongDa", SoSan = 2, TrangThai = true };
                await context.SanDaus.AddAsync(san2);
            }
            await context.SaveChangesAsync();

            // 10. Tạo 1 GIẢI ĐẤU & Gán Trưởng Ban Trọng Tài (ttA)
            var giaiDau = await context.GiaiDaus.FirstOrDefaultAsync(g => g.Ma == "GIAI_SV_BD3_2026");
            var nowTime = DateTime.UtcNow;
            if (giaiDau == null)
            {
                giaiDau = new GiaiDau
                {
                    Ma = "GIAI_SV_BD3_2026",
                    Ten = "Giải Vô Địch Bóng Đá 3 Người Sinh Viên Toàn Quốc 2026",
                    Slug = "giai-vo-dich-bong-da-3-nguoi-sinh-vien-toan-quoc-2026",
                    MoTa = "Giải đấu bóng đá mini 3 người đỉnh cao dành cho sinh viên các trường đại học hàng đầu tranh tài.",
                    NgayBatDau = nowTime.Date,
                    NgayKetThuc = nowTime.Date.AddDays(10),
                    HanDangKy = nowTime.Date.AddDays(-2),
                    DiaDiem = "Khu liên hợp thể thao Mỹ Đình, Hà Nội",
                    PhamVi = PhamViGiaiDau.TheoKhoi,
                    TrangThai = TrangThaiGiaiDau.DangDienRa,
                    TruongBanTrongTaiId = ttA.Id // Gán Trưởng Ban Trọng Tài Tài A
                };
                await context.GiaiDaus.AddAsync(giaiDau);
                await context.SaveChangesAsync();

                // Gán giải đấu vào Khối
                await context.GiaiDauKhois.AddAsync(new GiaiDauKhoi { GiaiDauId = giaiDau.Id, KhoiId = khoiTruong.Id });
                await context.SaveChangesAsync();
            }
            else
            {
                giaiDau.TruongBanTrongTaiId = ttA.Id;
                await context.SaveChangesAsync();
            }

            // 11. Đưa môn Bóng đá 3 người vào giải đấu (GiaiDauMonTheThao)
            var gdm = await context.GiaiDauMonTheThaos.FirstOrDefaultAsync(g => g.GiaiDauId == giaiDau.Id && g.MonTheThaoId == monBongDa3.Id);
            if (gdm == null)
            {
                gdm = new GiaiDauMonTheThao
                {
                    GiaiDauId = giaiDau.Id,
                    MonTheThaoId = monBongDa3.Id,
                    MoTa = "Môn bóng đá 3 người nam sinh viên tranh tài 2026",
                    TrangThai = true
                };
                await context.GiaiDauMonTheThaos.AddAsync(gdm);
                await context.SaveChangesAsync();
            }

            // 12. TẠO 16 ĐỘI THI ĐẤU (Mỗi đơn vị cử 4 đội, mỗi đội 5 VĐV đầy đủ vị trí)
            var vdvsBk = await context.VanDongViens.Where(v => v.DonViId == dvBk.Id).OrderBy(v => v.Ma).ToListAsync();
            var vdvsHvtc = await context.VanDongViens.Where(v => v.DonViId == dvHvtc.Id).OrderBy(v => v.Ma).ToListAsync();
            var vdvsSphn = await context.VanDongViens.Where(v => v.DonViId == dvSphn.Id).OrderBy(v => v.Ma).ToListAsync();
            var vdvsQghn = await context.VanDongViens.Where(v => v.DonViId == dvQghn.Id).OrderBy(v => v.Ma).ToListAsync();

            var teamConfigs = new[]
            {
                // Bảng A (4 trường khác nhau)
                new { Ma = "DOI_BK_01", Ten = "Bách Khoa 1 (BK Warriors)", DonViId = dvBk.Id, Vdvs = vdvsBk.Skip(0).Take(5).ToList(), Bang = "A" },
                new { Ma = "DOI_TC_01", Ten = "Tài Chính 1 (AOF Kings)", DonViId = dvHvtc.Id, Vdvs = vdvsHvtc.Skip(0).Take(5).ToList(), Bang = "A" },
                new { Ma = "DOI_SP_01", Ten = "Sư Phạm 1 (HNUE Stars)", DonViId = dvSphn.Id, Vdvs = vdvsSphn.Skip(0).Take(5).ToList(), Bang = "A" },
                new { Ma = "DOI_QG_01", Ten = "Quốc Gia 1 (VNU Thunder)", DonViId = dvQghn.Id, Vdvs = vdvsQghn.Skip(0).Take(5).ToList(), Bang = "A" },

                // Bảng B (4 trường khác nhau)
                new { Ma = "DOI_BK_02", Ten = "Bách Khoa 2 (BK Phoenix)", DonViId = dvBk.Id, Vdvs = vdvsBk.Skip(5).Take(5).ToList(), Bang = "B" },
                new { Ma = "DOI_TC_02", Ten = "Tài Chính 2 (AOF Tigers)", DonViId = dvHvtc.Id, Vdvs = vdvsHvtc.Skip(5).Take(5).ToList(), Bang = "B" },
                new { Ma = "DOI_SP_02", Ten = "Sư Phạm 2 (HNUE Eagles)", DonViId = dvSphn.Id, Vdvs = vdvsSphn.Skip(5).Take(5).ToList(), Bang = "B" },
                new { Ma = "DOI_QG_02", Ten = "Quốc Gia 2 (VNU Storm)", DonViId = dvQghn.Id, Vdvs = vdvsQghn.Skip(5).Take(5).ToList(), Bang = "B" },

                // Bảng C (4 trường khác nhau)
                new { Ma = "DOI_BK_03", Ten = "Bách Khoa 3 (BK Titans)", DonViId = dvBk.Id, Vdvs = vdvsBk.Skip(10).Take(5).ToList(), Bang = "C" },
                new { Ma = "DOI_TC_03", Ten = "Tài Chính 3 (AOF Stars)", DonViId = dvHvtc.Id, Vdvs = vdvsHvtc.Skip(10).Take(5).ToList(), Bang = "C" },
                new { Ma = "DOI_SP_03", Ten = "Sư Phạm 3 (HNUE Lions)", DonViId = dvSphn.Id, Vdvs = vdvsSphn.Skip(10).Take(5).ToList(), Bang = "C" },
                new { Ma = "DOI_QG_03", Ten = "Quốc Gia 3 (VNU Lightning)", DonViId = dvQghn.Id, Vdvs = vdvsQghn.Skip(10).Take(5).ToList(), Bang = "C" },

                // Bảng D (4 trường khác nhau)
                new { Ma = "DOI_BK_04", Ten = "Bách Khoa 4 (BK Dragons)", DonViId = dvBk.Id, Vdvs = vdvsBk.Skip(15).Take(5).ToList(), Bang = "D" },
                new { Ma = "DOI_TC_04", Ten = "Tài Chính 4 (AOF Wolves)", DonViId = dvHvtc.Id, Vdvs = vdvsHvtc.Skip(15).Take(5).ToList(), Bang = "D" },
                new { Ma = "DOI_SP_04", Ten = "Sư Phạm 4 (HNUE Hawks)", DonViId = dvSphn.Id, Vdvs = vdvsSphn.Skip(15).Take(5).ToList(), Bang = "D" },
                new { Ma = "DOI_QG_04", Ten = "Quốc Gia 4 (VNU Cyclones)", DonViId = dvQghn.Id, Vdvs = vdvsQghn.Skip(15).Take(5).ToList(), Bang = "D" },
            };

            var doiMap = new Dictionary<string, Doi>();
            var dkMap = new Dictionary<string, DangKyThiDau>();

            foreach (var tc in teamConfigs)
            {
                var doi = await context.Dois.FirstOrDefaultAsync(d => d.Ma == tc.Ma);
                if (doi == null)
                {
                    doi = new Doi
                    {
                        Ma = tc.Ma,
                        Ten = tc.Ten,
                        DonViId = tc.DonViId,
                        TrangThai = true,
                        NguoiQuanLy = "Ban Thể Thao Đoàn",
                        SoDienThoai = "0988123456"
                    };
                    await context.Dois.AddAsync(doi);
                    await context.SaveChangesAsync();

                    // Thêm 5 thành viên đội (ThanhVienDoi)
                    string[] viTris = { "Tiền đạo", "Hậu vệ", "Thủ môn", "Dự bị", "Dự bị" };
                    string[] soAos = { "10", "04", "01", "07", "09" };
                    for (int vi = 0; vi < tc.Vdvs.Count; vi++)
                    {
                        var tvd = new ThanhVienDoi
                        {
                            DoiId = doi.Id,
                            VanDongVienId = tc.Vdvs[vi].Id,
                            SoAo = soAos[vi],
                            ViTri = viTris[vi],
                            LaDoiTruong = (vi == 0),
                            NgayThamGia = nowTime.AddDays(-10)
                        };
                        await context.ThanhVienDois.AddAsync(tvd);
                    }
                    await context.SaveChangesAsync();
                }
                doiMap[tc.Ma] = doi;

                // Tạo hồ sơ Đăng Ký Thi Đấu (DangKyThiDau) cho đội
                string soDk = $"DK_{tc.Ma}";
                var dk = await context.DangKyThiDaus.FirstOrDefaultAsync(d => d.SoDangKy == soDk);
                if (dk == null)
                {
                    dk = new DangKyThiDau
                    {
                        GiaiDauMonTheThaoId = gdm.Id,
                        DoiId = doi.Id,
                        SoDangKy = soDk,
                        TenDangKy = tc.Ten,
                        TrangThai = "DaDuyet",
                        NgayDangKy = nowTime.AddDays(-5),
                        GhiChu = "Đã phê duyệt hồ sơ tham gia giải đấu"
                    };
                    await context.DangKyThiDaus.AddAsync(dk);
                    await context.SaveChangesAsync();
                }
                dkMap[tc.Ma] = dk;
            }

            // 13. TẠO 4 BẢNG ĐẤU: Bảng A, Bảng B, Bảng C, Bảng D
            var bangA = await context.BangDaus.FirstOrDefaultAsync(b => b.GiaiDauMonTheThaoId == gdm.Id && b.Ma == "BANG_A");
            if (bangA == null)
            {
                bangA = new BangDau { GiaiDauMonTheThaoId = gdm.Id, Ma = "BANG_A", Ten = "Bảng A", ThuTu = 1 };
                await context.BangDaus.AddAsync(bangA);
            }

            var bangB = await context.BangDaus.FirstOrDefaultAsync(b => b.GiaiDauMonTheThaoId == gdm.Id && b.Ma == "BANG_B");
            if (bangB == null)
            {
                bangB = new BangDau { GiaiDauMonTheThaoId = gdm.Id, Ma = "BANG_B", Ten = "Bảng B", ThuTu = 2 };
                await context.BangDaus.AddAsync(bangB);
            }

            var bangC = await context.BangDaus.FirstOrDefaultAsync(b => b.GiaiDauMonTheThaoId == gdm.Id && b.Ma == "BANG_C");
            if (bangC == null)
            {
                bangC = new BangDau { GiaiDauMonTheThaoId = gdm.Id, Ma = "BANG_C", Ten = "Bảng C", ThuTu = 3 };
                await context.BangDaus.AddAsync(bangC);
            }

            var bangD = await context.BangDaus.FirstOrDefaultAsync(b => b.GiaiDauMonTheThaoId == gdm.Id && b.Ma == "BANG_D");
            if (bangD == null)
            {
                bangD = new BangDau { GiaiDauMonTheThaoId = gdm.Id, Ma = "BANG_D", Ten = "Bảng D", ThuTu = 4 };
                await context.BangDaus.AddAsync(bangD);
            }
            await context.SaveChangesAsync();

            // Xếp các đội vào 4 Bảng đấu (ThanhVienBang)
            var existingTvb = await context.ThanhVienBangs.ToListAsync();
            var bangMap = new Dictionary<string, BangDau>
            {
                { "A", bangA },
                { "B", bangB },
                { "C", bangC },
                { "D", bangD }
            };

            foreach (var tc in teamConfigs)
            {
                var targetBang = bangMap[tc.Bang];
                var dk = dkMap[tc.Ma];
                if (!existingTvb.Any(tv => tv.BangDauId == targetBang.Id && tv.DangKyThiDauId == dk.Id))
                {
                    await context.ThanhVienBangs.AddAsync(new ThanhVienBang
                    {
                        BangDauId = targetBang.Id,
                        DangKyThiDauId = dk.Id,
                        HatGiong = null,
                        SoTran = 0,
                        SoThang = 0,
                        SoHoa = 0,
                        SoThua = 0,
                        DiemGhiDuoc = 0,
                        DiemBiGhi = 0,
                        Diem = 0
                    });
                }
            }
            await context.SaveChangesAsync();

            // 14. TẠO CÁC VÒNG ĐẤU (VongDau)
            var vongBang = await context.VongDaus.FirstOrDefaultAsync(v => v.GiaiDauMonTheThaoId == gdm.Id && v.LoaiVong == "VongBang");
            if (vongBang == null)
            {
                vongBang = new VongDau { GiaiDauMonTheThaoId = gdm.Id, Ten = "Vòng Bảng", LoaiVong = "VongBang", ThuTu = 1 };
                await context.VongDaus.AddAsync(vongBang);
            }

            var vongTuKet = await context.VongDaus.FirstOrDefaultAsync(v => v.GiaiDauMonTheThaoId == gdm.Id && v.LoaiVong == "TuKet");
            if (vongTuKet == null)
            {
                vongTuKet = new VongDau { GiaiDauMonTheThaoId = gdm.Id, Ten = "Vòng Tứ Kết", LoaiVong = "TuKet", ThuTu = 2 };
                await context.VongDaus.AddAsync(vongTuKet);
            }

            var vongBanKet = await context.VongDaus.FirstOrDefaultAsync(v => v.GiaiDauMonTheThaoId == gdm.Id && v.LoaiVong == "BanKet");
            if (vongBanKet == null)
            {
                vongBanKet = new VongDau { GiaiDauMonTheThaoId = gdm.Id, Ten = "Vòng Bán Kết", LoaiVong = "BanKet", ThuTu = 3 };
                await context.VongDaus.AddAsync(vongBanKet);
            }

            var vongTranh3 = await context.VongDaus.FirstOrDefaultAsync(v => v.GiaiDauMonTheThaoId == gdm.Id && v.LoaiVong == "TranhHangBa");
            if (vongTranh3 == null)
            {
                vongTranh3 = new VongDau { GiaiDauMonTheThaoId = gdm.Id, Ten = "Tranh Hạng 3 - 4", LoaiVong = "TranhHangBa", ThuTu = 4 };
                await context.VongDaus.AddAsync(vongTranh3);
            }

            var vongChungKet = await context.VongDaus.FirstOrDefaultAsync(v => v.GiaiDauMonTheThaoId == gdm.Id && v.LoaiVong == "ChungKet");
            if (vongChungKet == null)
            {
                vongChungKet = new VongDau { GiaiDauMonTheThaoId = gdm.Id, Ten = "Trận Chung Kết", LoaiVong = "ChungKet", ThuTu = 5 };
                await context.VongDaus.AddAsync(vongChungKet);
            }
            await context.SaveChangesAsync();

            // 15. TẠO 24 TRẬN ĐẤU VÒNG BẢNG Ở TRẠNG THÁI "CHƯA ĐẤU" (CHƯA CÓ TỈ SỐ) & PHÂN CÔNG TOÀN BỘ TRỌNG TÀI
            // Tỉ số sẽ được các trọng tài cập nhật trực tiếp sau khi điều hành trận đấu!
            if (!await context.TranDaus.AnyAsync(t => t.GiaiDauMonTheThaoId == gdm.Id))
            {
                var refList = new[] { ttA, ttB, ttC, ttD, ttE, ttF, ttTrong, ttHanh };

                // Định nghĩa 6 trận cho mỗi bảng 4 đội (thi đấu vòng tròn 1 lượt: Lượt 1, Lượt 2, Lượt 3)
                // Thứ tự đội trong bảng: [0] BK, [1] TC, [2] SP, [3] QG
                var bangDefinitions = new[]
                {
                    new { Bang = bangA, Char = "A", Teams = new[] { dkMap["DOI_BK_01"], dkMap["DOI_TC_01"], dkMap["DOI_SP_01"], dkMap["DOI_QG_01"] } },
                    new { Bang = bangB, Char = "B", Teams = new[] { dkMap["DOI_BK_02"], dkMap["DOI_TC_02"], dkMap["DOI_SP_02"], dkMap["DOI_QG_02"] } },
                    new { Bang = bangC, Char = "C", Teams = new[] { dkMap["DOI_BK_03"], dkMap["DOI_TC_03"], dkMap["DOI_SP_03"], dkMap["DOI_QG_03"] } },
                    new { Bang = bangD, Char = "D", Teams = new[] { dkMap["DOI_BK_04"], dkMap["DOI_TC_04"], dkMap["DOI_SP_04"], dkMap["DOI_QG_04"] } },
                };

                int matchIndex = 1;
                var startDate = nowTime.Date.AddDays(1); // Bắt đầu từ ngày mai

                foreach (var bDef in bangDefinitions)
                {
                    var t = bDef.Teams;
                    // 6 cặp đấu vòng tròn:
                    // Lượt 1: (0 vs 1), (2 vs 3)
                    // Lượt 2: (0 vs 2), (1 vs 3)
                    // Lượt 3: (0 vs 3), (1 vs 2)
                    var roundMatches = new[]
                    {
                        new { D1 = t[0], D2 = t[1], Luot = 1, DayOffset = 0, Hour = 8, San = san1 },
                        new { D1 = t[2], D2 = t[3], Luot = 1, DayOffset = 0, Hour = 9, San = san2 },
                        new { D1 = t[0], D2 = t[2], Luot = 2, DayOffset = 1, Hour = 8, San = san1 },
                        new { D1 = t[1], D2 = t[3], Luot = 2, DayOffset = 1, Hour = 9, San = san2 },
                        new { D1 = t[0], D2 = t[3], Luot = 3, DayOffset = 2, Hour = 8, San = san1 },
                        new { D1 = t[1], D2 = t[2], Luot = 3, DayOffset = 2, Hour = 9, San = san2 },
                    };

                    foreach (var rm in roundMatches)
                    {
                        var scheduledTime = startDate.AddDays(rm.DayOffset).AddHours(rm.Hour);
                        string matchName = $"{rm.D1.TenDangKy} vs {rm.D2.TenDangKy} (Lượt {rm.Luot} Bảng {bDef.Char})";

                        var tran = new TranDau
                        {
                            GiaiDauMonTheThaoId = gdm.Id,
                            VongDauId = vongBang.Id,
                            BangDauId = bDef.Bang.Id,
                            SanDauId = rm.San.Id,
                            SoTran = matchIndex,
                            TenTran = matchName,
                            ThoiGianDuKien = scheduledTime,
                            ThoiGianBatDau = null,
                            ThoiGianKetThuc = null,
                            TrangThai = "ChuaDau", // Chưa đấu, chờ trọng tài cập nhật tỉ số
                            TySoDoi1 = null,
                            TySoDoi2 = null,
                            DoiThangDangKyId = null,
                            DoiThuaDangKyId = null,
                            IsHoa = false,
                            GhiChu = null
                        };
                        await context.TranDaus.AddAsync(tran);
                        await context.SaveChangesAsync();

                        // Thành phần tham gia trận đấu (2 đội)
                        await context.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau { TranDauId = tran.Id, DangKyThiDauId = rm.D1.Id, ViTri = 1, TrangThai = "ThamGia" });
                        await context.ThanhPhanTranDaus.AddAsync(new ThanhPhanTranDau { TranDauId = tran.Id, DangKyThiDauId = rm.D2.Id, ViTri = 2, TrangThai = "ThamGia" });

                        // Phân công Trọng tài chính và Trọng tài bàn luân phiên xoay vòng giữa 8 trọng tài
                        var refMain = refList[(matchIndex - 1) % refList.Length];
                        var refAss = refList[(matchIndex) % refList.Length];

                        await context.PhanCongTrongTais.AddAsync(new PhanCongTrongTai
                        {
                            TranDauId = tran.Id,
                            TrongTaiId = refMain.Id,
                            VaiTro = "Trọng tài chính",
                            GhiChu = "Bắt chính điều hành trận đấu"
                        });

                        await context.PhanCongTrongTais.AddAsync(new PhanCongTrongTai
                        {
                            TranDauId = tran.Id,
                            TrongTaiId = refAss.Id,
                            VaiTro = "Trọng tài bàn",
                            GhiChu = "Ghi biên bản và hỗ trợ điều hành"
                        });

                        await context.SaveChangesAsync();
                        matchIndex++;
                    }
                }
            }
        }
    }
}
