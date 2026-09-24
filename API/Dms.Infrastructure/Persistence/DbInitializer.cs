using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dms.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'MonTheThao' AND COLUMN_NAME = 'LoaiThiDau'
)
BEGIN
    ALTER TABLE MonTheThao ADD LoaiThiDau NVARCHAR(30) NULL;
END
");
                await context.Database.ExecuteSqlRawAsync(@"
UPDATE MonTheThao 
SET LoaiThiDau = CASE WHEN LaMonDongDoi = 1 THEN 'DongDoi' ELSE 'CaNhan' END 
WHERE LoaiThiDau IS NULL OR LoaiThiDau = '';
");
            }
            catch { }

            // Đảm bảo Database đã được tạo hoặc được migrate
            await context.Database.MigrateAsync();

            try
            {
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('GiaiDau') AND name = 'HanDangKy'
                    )
                    BEGIN
                        ALTER TABLE GiaiDau ADD HanDangKy DATETIME2 NULL;
                    END

                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('GiaiDau') AND name = 'TruongBanTrongTaiId'
                    )
                    BEGIN
                        ALTER TABLE GiaiDau ADD TruongBanTrongTaiId INT NULL;
                    END

                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('GiaiDauMonTheThao') AND name = 'NguoiDieuHanhId'
                    )
                    BEGIN
                        ALTER TABLE GiaiDauMonTheThao ADD NguoiDieuHanhId INT NULL;
                    END
                ");
            }
            catch { }

            // Khởi tạo các Role mặc định theo hệ thống thể thao giải đấu
            string[] roleNames = Dms.Application.Common.AppRoles.AllRoles;
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // Gán Permissions cho Role Admin (Toàn bộ quyền hệ thống)
            var adminRole = await roleManager.FindByNameAsync(Dms.Application.Common.AppRoles.Admin);
            if (adminRole != null)
            {
                var existingClaims = await roleManager.GetClaimsAsync(adminRole);
                var allPermissions = Dms.Application.Common.Permissions.GetAllPermissions();

                foreach (var permission in allPermissions)
                {
                    if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                    {
                        await roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim("permission", permission));
                    }
                }
            }

            // Cấu hình permissions cho từng Role nghiệp vụ
            var rolePermissionsMap = new Dictionary<string, List<string>>
            {
                [Dms.Application.Common.AppRoles.Manager] = new()
                {
                    Dms.Application.Common.Permissions.GiaiDau.View,
                    Dms.Application.Common.Permissions.GiaiDau.Create,
                    Dms.Application.Common.Permissions.GiaiDau.Edit,
                    Dms.Application.Common.Permissions.GiaiDau.Delete,
                    Dms.Application.Common.Permissions.GiaiDau.AssignManager,
                    Dms.Application.Common.Permissions.Khoi.View,
                    Dms.Application.Common.Permissions.Khoi.Create,
                    Dms.Application.Common.Permissions.Khoi.Edit,
                    Dms.Application.Common.Permissions.DonVi.View,
                    Dms.Application.Common.Permissions.DonVi.Create,
                    Dms.Application.Common.Permissions.DonVi.Edit,
                    Dms.Application.Common.Permissions.DanhMucMonTheThao.View,
                    Dms.Application.Common.Permissions.DanhMucMonTheThao.Create,
                    Dms.Application.Common.Permissions.DanhMucMonTheThao.Edit,
                    Dms.Application.Common.Permissions.MonTheThao.View,
                    Dms.Application.Common.Permissions.MonTheThao.Create,
                    Dms.Application.Common.Permissions.MonTheThao.Edit,
                    Dms.Application.Common.Permissions.MonTheThao.Delete,
                    Dms.Application.Common.Permissions.GiaiDauMonTheThao.View,
                    Dms.Application.Common.Permissions.GiaiDauMonTheThao.Create,
                    Dms.Application.Common.Permissions.GiaiDauMonTheThao.Delete,
                    Dms.Application.Common.Permissions.NoiDungThiDau.View,
                    Dms.Application.Common.Permissions.NoiDungThiDau.Create,
                    Dms.Application.Common.Permissions.NoiDungThiDau.Edit,
                    Dms.Application.Common.Permissions.BangDau.View,
                    Dms.Application.Common.Permissions.BangDau.Create,
                    Dms.Application.Common.Permissions.BangDau.Edit,
                    Dms.Application.Common.Permissions.BangDau.Delete,
                    Dms.Application.Common.Permissions.VongDau.View,
                    Dms.Application.Common.Permissions.VongDau.Create,
                    Dms.Application.Common.Permissions.VongDau.Edit,
                    Dms.Application.Common.Permissions.Doi.View,
                    Dms.Application.Common.Permissions.Doi.Create,
                    Dms.Application.Common.Permissions.Doi.Edit,
                    Dms.Application.Common.Permissions.Doi.Delete,
                    Dms.Application.Common.Permissions.VanDongVien.View,
                    Dms.Application.Common.Permissions.VanDongVien.Create,
                    Dms.Application.Common.Permissions.VanDongVien.Edit,
                    Dms.Application.Common.Permissions.VanDongVien.Delete,
                    Dms.Application.Common.Permissions.DangKyThiDau.View,
                    Dms.Application.Common.Permissions.DangKyThiDau.Create,
                    Dms.Application.Common.Permissions.DangKyThiDau.Edit,
                    Dms.Application.Common.Permissions.DangKyThiDau.Approve,
                    Dms.Application.Common.Permissions.TranDau.View,
                    Dms.Application.Common.Permissions.TranDau.Create,
                    Dms.Application.Common.Permissions.TranDau.Edit,
                    Dms.Application.Common.Permissions.TranDau.Delete,
                    Dms.Application.Common.Permissions.SanDau.View,
                    Dms.Application.Common.Permissions.SanDau.Create,
                    Dms.Application.Common.Permissions.SanDau.Edit,
                    Dms.Application.Common.Permissions.HuyChuong.View,
                    Dms.Application.Common.Permissions.HuyChuong.Create,
                    Dms.Application.Common.Permissions.HuyChuong.Edit,
                },
                [Dms.Application.Common.AppRoles.HeadReferee] = new()
                {
                    Dms.Application.Common.Permissions.TrongTai.Assign,
                    Dms.Application.Common.Permissions.TrongTai.Supervise,
                    Dms.Application.Common.Permissions.TrongTai.View,
                    Dms.Application.Common.Permissions.GiaiDau.View,
                    Dms.Application.Common.Permissions.MonTheThao.View,
                    Dms.Application.Common.Permissions.TranDau.View,
                    Dms.Application.Common.Permissions.TranDau.UpdateScore,
                },
                [Dms.Application.Common.AppRoles.Referee] = new()
                {
                    Dms.Application.Common.Permissions.TranDau.View,
                    Dms.Application.Common.Permissions.TranDau.UpdateScore,
                    Dms.Application.Common.Permissions.GiaiDau.View,
                    Dms.Application.Common.Permissions.MonTheThao.View,
                    Dms.Application.Common.Permissions.SanDau.View,
                    Dms.Application.Common.Permissions.TrongTai.View,
                    Dms.Application.Common.Permissions.BangDau.View,
                    Dms.Application.Common.Permissions.VongDau.View,
                    Dms.Application.Common.Permissions.DonVi.View,
                    Dms.Application.Common.Permissions.Doi.View,
                    Dms.Application.Common.Permissions.VanDongVien.View,
                    Dms.Application.Common.Permissions.DangKyThiDau.View
                },
                [Dms.Application.Common.AppRoles.Secretary] = new()
                {
                    Dms.Application.Common.Permissions.GiaiDau.View,
                    Dms.Application.Common.Permissions.TranDau.View,
                    Dms.Application.Common.Permissions.TranDau.VerifyReport,
                    Dms.Application.Common.Permissions.TranDau.ExportReport
                },
                [Dms.Application.Common.AppRoles.Delegation] = new()
                {
                    Dms.Application.Common.Permissions.DonVi.ManageAthletes,
                    Dms.Application.Common.Permissions.GiaiDau.View,
                    Dms.Application.Common.Permissions.Doi.View,
                    Dms.Application.Common.Permissions.Doi.Create,
                    Dms.Application.Common.Permissions.Doi.Edit,
                    Dms.Application.Common.Permissions.VanDongVien.View,
                    Dms.Application.Common.Permissions.VanDongVien.Create,
                    Dms.Application.Common.Permissions.VanDongVien.Edit,
                    Dms.Application.Common.Permissions.VanDongVien.Delete,
                    Dms.Application.Common.Permissions.DangKyThiDau.View,
                    Dms.Application.Common.Permissions.DangKyThiDau.Create,
                    Dms.Application.Common.Permissions.DangKyThiDau.Edit,
                    Dms.Application.Common.Permissions.DangKyThiDau.Delete,
                }
            };

            foreach (var kvp in rolePermissionsMap)
            {
                var roleObj = await roleManager.FindByNameAsync(kvp.Key);
                if (roleObj != null)
                {
                    var existingClaims = await roleManager.GetClaimsAsync(roleObj);
                    foreach (var permission in kvp.Value)
                    {
                        if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                        {
                            await roleManager.AddClaimAsync(roleObj, new System.Security.Claims.Claim("permission", permission));
                        }
                    }
                }
            }

            // Khởi tạo tài khoản Admin mặc định
            // Danh sách tài khoản mẫu cho 6 vai trò
            var defaultUsers = new[]
            {

                new { Username = "admin", Email = "admin@sportdms.com", Name = "Quản trị viên hệ thống", Role = Dms.Application.Common.AppRoles.Admin, Pass = "Admin@123" },
                new { Username = "manager", Email = "manager@sportdms.com", Name = "Nguyễn Văn Quản Lý", Role = Dms.Application.Common.AppRoles.Manager, Pass = "Manager@123" },
                new { Username = "head_referee", Email = "head_ref@sportdms.com", Name = "Trần Trưởng Trọng Tài", Role = Dms.Application.Common.AppRoles.HeadReferee, Pass = "HeadReferee@123" },
                new { Username = "referee", Email = "referee@sportdms.com", Name = "Lê Văn Trọng Tài", Role = Dms.Application.Common.AppRoles.Referee, Pass = "Referee@123" },
                new { Username = "delegation", Email = "delegation@sportdms.com", Name = "Đoàn VĐV Sở VH-TT", Role = Dms.Application.Common.AppRoles.Delegation, Pass = "Delegation@123" },
                new { Username = "donvi_bk", Email = "bk_hcm@sportdms.com", Name = "Đại diện Đại học Bách Khoa", Role = Dms.Application.Common.AppRoles.Delegation, Pass = "Delegation@123" },
                new { Username = "donvi_hvtc", Email = "tc@hanoi.edu.vn", Name = "Đại diện Học Viện Tài Chính", Role = Dms.Application.Common.AppRoles.Delegation, Pass = "Delegation@123" },
                new { Username = "donvi_sphn", Email = "sp@hanoi.edu.vn", Name = "Đại diện ĐH Sư Phạm Hà Nội", Role = Dms.Application.Common.AppRoles.Delegation, Pass = "Delegation@123" },
                new { Username = "donvi_qghn", Email = "qg@hanoi.edu.vn", Name = "Đại diện ĐH Quốc Gia Hà Nội", Role = Dms.Application.Common.AppRoles.Delegation, Pass = "Delegation@123" }
            };
            foreach (var u in defaultUsers)
            {
                var existing = await userManager.FindByNameAsync(u.Username);
                if (existing == null)
                {
                    var userEntity = new ApplicationUser
                    {
                        UserName = u.Username,
                        Email = u.Email,
                        FullName = u.Name,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var res = await userManager.CreateAsync(userEntity, u.Pass);
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(userEntity, u.Role);
                    }
                }
            }

            // Khởi tạo Menu Cha - Con mặc định nếu chưa có
            if (!await context.Menus.AnyAsync())
            {
                var homeMenu = new Menu { Title = "Trang chủ", Url = "/", Icon = "bi-house-door", SortOrder = 1, IsActive = true };
                var serviceMenu = new Menu { Title = "Dịch vụ sửa chữa", Url = "#services", Icon = "bi-tools", SortOrder = 2, IsActive = true };
                var pricingMenu = new Menu { Title = "Bảng giá dịch vụ", Url = "/#pricing", Icon = "bi-tags", SortOrder = 3, IsActive = true };
                var tipsMenu = new Menu { Title = "Cẩm nang & Mẹo vặt", Url = "/tips", Icon = "bi-journal-text", SortOrder = 4, IsActive = true };
                var contactMenu = new Menu { Title = "Liên hệ", Url = "/#contact", Icon = "bi-telephone", SortOrder = 5, IsActive = true };

                await context.Menus.AddRangeAsync(homeMenu, serviceMenu, pricingMenu, tipsMenu, contactMenu);
                await context.SaveChangesAsync();

                // Menu con của "Dịch vụ sửa chữa"
                var childServices = new List<Menu>
                {
                    new Menu { Title = "Bảo Dưỡng & Vệ Sinh Máy Lạnh", Url = "/repair/1", Icon = "bi-snow", SortOrder = 1, IsActive = true, ParentId = serviceMenu.Id },
                    new Menu { Title = "Sửa Chữa Tủ Lạnh Inverter", Url = "/repair/2", Icon = "bi-patch-check", SortOrder = 2, IsActive = true, ParentId = serviceMenu.Id },
                    new Menu { Title = "Sửa Chữa & Vệ Sinh Máy Giặt", Url = "/repair/3", Icon = "bi-water", SortOrder = 3, IsActive = true, ParentId = serviceMenu.Id },
                    new Menu { Title = "Lắp Đặt & Di Dời Máy Lạnh", Url = "/repair/4", Icon = "bi-tools", SortOrder = 4, IsActive = true, ParentId = serviceMenu.Id },
                };

                // Menu con của "Cẩm nang & Mẹo vặt"
                var childTips = new List<Menu>
                {
                    new Menu { Title = "5 Mẹo Dùng Máy Lạnh Tiết Kiệm Điện", Url = "/tips", Icon = "bi-lightning-charge", SortOrder = 1, IsActive = true, ParentId = tipsMenu.Id },
                    new Menu { Title = "Nhận Biết Máy Lạnh Bị Thiếu Gas", Url = "/tips", Icon = "bi-exclamation-diamond", SortOrder = 2, IsActive = true, ParentId = tipsMenu.Id },
                    new Menu { Title = "Tự Vệ Sinh Lưới Lọc Tại Nhà", Url = "/tips", Icon = "bi-brush", SortOrder = 3, IsActive = true, ParentId = tipsMenu.Id },
                    new Menu { Title = "Xem Tất Cả Bài Viết Cẩm Nang", Url = "/tips", Icon = "bi-grid", SortOrder = 4, IsActive = true, ParentId = tipsMenu.Id },
                };

                await context.Menus.AddRangeAsync(childServices);
                await context.Menus.AddRangeAsync(childTips);
                await context.SaveChangesAsync();
            }

            // ==========================================
            // SEED DỮ LIỆU THỂ THAO TOÀN DIỆN CHO TẤT CẢ 22 BẢNG
            // ==========================================
            if (!await context.GiaiDaus.AnyAsync() && !await context.DonVis.AnyAsync())
            {
                var now = DateTime.UtcNow;

                // 1. Seed Loại Huy Chương
                var lhcVang = new LoaiHuyChuong { Ma = "VANG", Ten = "Huy chương Vàng", ThuTu = 1 };
                var lhcBac = new LoaiHuyChuong { Ma = "BAC", Ten = "Huy chương Bạc", ThuTu = 2 };
                var lhcDong = new LoaiHuyChuong { Ma = "DONG", Ten = "Huy chương Đồng", ThuTu = 3 };
                await context.LoaiHuyChuongs.AddRangeAsync(lhcVang, lhcBac, lhcDong);
                await context.SaveChangesAsync();

                // 2. Seed Khối
                var khoiTruongHoc = new Khoi { Ma = "KHOI_TRUONG", Ten = "Khối Trường Học & Sinh Viên", MoTa = "Các trường ĐH, CĐ và THPT", TrangThai = true };
                var khoiDoanhNghiep = new Khoi { Ma = "KHOI_DN", Ten = "Khối Cơ Quan & Doanh Nghiệp", MoTa = "Các cơ quan ban ngành và doanh nghiệp trên địa bàn", TrangThai = true };
                var khoiCauLacBo = new Khoi { Ma = "KHOI_CLB", Ten = "Khối Câu Lạc Bộ Chuyên Nghiệp", MoTa = "Các câu lạc bộ thể thao mở rộng", TrangThai = true };
                await context.Khois.AddRangeAsync(khoiTruongHoc, khoiDoanhNghiep, khoiCauLacBo);
                await context.SaveChangesAsync();

                // 3. Seed Đơn Vị (thuộc khối)
                var dvBachKhoa = new DonVi { Ma = "DV_BK", Ten = "Đại học Bách Khoa", KhoiId = khoiTruongHoc.Id, LoaiDonVi = "TruongHoc", DiaChi = "268 Lý Thường Kiệt, Q.10, TP.HCM", NguoiDaiDien = "Nguyễn Văn Hùng", SoDienThoai = "0901234567", Email = "sport@hcmut.edu.vn", TrangThai = true };
                var dvKinhTe = new DonVi { Ma = "DV_UEH", Ten = "Đại học Kinh Tế TP.HCM", KhoiId = khoiTruongHoc.Id, LoaiDonVi = "TruongHoc", DiaChi = "59C Nguyễn Đình Chiểu, Q.3, TP.HCM", NguoiDaiDien = "Trần Thị Mai", SoDienThoai = "0902345678", Email = "sport@ueh.edu.vn", TrangThai = true };
                var dvFpt = new DonVi { Ma = "DV_FPT", Ten = "Tập đoàn FPT", KhoiId = khoiDoanhNghiep.Id, LoaiDonVi = "DoanhNghiep", DiaChi = "Khu Công nghệ cao, TP.Thủ Đức", NguoiDaiDien = "Lê Hoàng Quân", SoDienThoai = "0903456789", Email = "sport@fpt.com.vn", TrangThai = true };
                var dvViettel = new DonVi { Ma = "DV_VTL", Ten = "Tập đoàn Viettel", KhoiId = khoiDoanhNghiep.Id, LoaiDonVi = "DoanhNghiep", DiaChi = "285 Cách Mạng Tháng 8, Q.10", NguoiDaiDien = "Phạm Quốc Tuấn", SoDienThoai = "0904567890", Email = "sport@viettel.vn", TrangThai = true };
                await context.DonVis.AddRangeAsync(dvBachKhoa, dvKinhTe, dvFpt, dvViettel);
                await context.SaveChangesAsync();

                // 4. Seed Danh Mục Môn Thể Thao & Môn Thể Thao (10 môn thể thao)
                var dmBong = new DanhMucMonTheThao { Ma = "DM_BONG", Ten = "Các môn bóng", MoTa = "Bóng đá, bóng chuyền, bóng rổ...", TrangThai = true };
                var dmVot = new DanhMucMonTheThao { Ma = "DM_VOT", Ten = "Các môn dùng vợt", MoTa = "Cầu lông, bóng bàn, tennis, pickleball...", TrangThai = true };
                var dmDienKinh = new DanhMucMonTheThao { Ma = "DM_DIENKINH", Ten = "Điền kinh & Dưới nước", MoTa = "Chạy cự ly, bơi tự do, bơi ếch...", TrangThai = true };
                var dmTriTue = new DanhMucMonTheThao { Ma = "DM_TRITUE", Ten = "Thể thao trí tuệ", MoTa = "Cờ vua, cờ tướng...", TrangThai = true };
                var dmVoThuat = new DanhMucMonTheThao { Ma = "DM_VOTHUAT", Ten = "Võ thuật & Đối kháng", MoTa = "Taekwondo, Karate, Vovinam...", TrangThai = true };
                await context.DanhMucMonTheThaos.AddRangeAsync(dmBong, dmVot, dmDienKinh, dmTriTue, dmVoThuat);
                await context.SaveChangesAsync();

                // 10 môn thể thao phong phú
                var monBongDa = new MonTheThao { DanhMucId = dmBong.Id, Ma = "BONG_DA", Ten = "Bóng đá sân 7", LaMonDongDoi = true, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.KetHopVongBangVaLoaiTrucTiep, MoTa = "Bóng đá mini cỏ nhân tạo 7 người (vòng bảng + knockout)", TrangThai = true };
                var monBongChuyen = new MonTheThao { DanhMucId = dmBong.Id, Ma = "BONG_CHUYEN", Ten = "Bóng chuyền da", LaMonDongDoi = true, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.KetHopVongBangVaLoaiTrucTiep, MoTa = "Bóng chuyền 6 người tiêu chuẩn", TrangThai = true };
                var monBongRo = new MonTheThao { DanhMucId = dmBong.Id, Ma = "BONG_RO", Ten = "Bóng rổ 3x3", LaMonDongDoi = true, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.LoaiTrucTiep, MoTa = "Bóng rổ nửa sân 3x3 nhịp độ cao", TrangThai = true };
                var monCauLong = new MonTheThao { DanhMucId = dmVot.Id, Ma = "CAU_LONG", Ten = "Cầu lông", LaMonDongDoi = false, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.LoaiTrucTiep, MoTa = "Thi đấu đơn và đôi loại trực tiếp", TrangThai = true };
                var monBongBan = new MonTheThao { DanhMucId = dmVot.Id, Ma = "BONG_BAN", Ten = "Bóng bàn", LaMonDongDoi = false, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.VongBang, MoTa = "Thi đấu vòng tròn tính điểm", TrangThai = true };
                var monTennis = new MonTheThao { DanhMucId = dmVot.Id, Ma = "QUAN_VOT", Ten = "Quần vợt (Tennis)", LaMonDongDoi = false, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.LoaiTrucTiep, MoTa = "Thi đấu đơn nam, đôi nam phong trào", TrangThai = true };
                var monPickleball = new MonTheThao { DanhMucId = dmVot.Id, Ma = "PICKLEBALL", Ten = "Pickleball", LaMonDongDoi = false, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.KetHopVongBangVaLoaiTrucTiep, MoTa = "Thi đấu đôi nam nữ pickleball hiện đại", TrangThai = true };
                var monBoi = new MonTheThao { DanhMucId = dmDienKinh.Id, Ma = "BOI_LOI", Ten = "Bơi lội 50m", LaMonDongDoi = false, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.TinhDiemXepHang, MoTa = "Bơi tự do 50m bấm giờ xếp hạng", TrangThai = true };
                var monChayDienKinh = new MonTheThao { DanhMucId = dmDienKinh.Id, Ma = "CHAY_100M", Ten = "Chạy cự ly ngắn 100m", LaMonDongDoi = false, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.TinhDiemXepHang, MoTa = "Điền kinh chạy nước rút 100m tính giây xếp hạng", TrangThai = true };
                var monCoVua = new MonTheThao { DanhMucId = dmTriTue.Id, Ma = "CO_VUA", Ten = "Cờ vua tiêu chuẩn", LaMonDongDoi = false, HinhThucThiDau = Dms.Domain.Enums.HinhThucThiDau.HeThuySi, MoTa = "Thi đấu cờ vua hệ Thụy Sĩ 7 ván tính điểm", TrangThai = true };

                await context.MonTheThaos.AddRangeAsync(
                    monBongDa,
                    monBongChuyen,
                    monBongRo,
                    monCauLong,
                    monBongBan,
                    monTennis,
                    monPickleball,
                    monBoi,
                    monChayDienKinh,
                    monCoVua
                );
                await context.SaveChangesAsync();

                // 5. Seed Giải Đấu & Liên kết Khối (GiaiDauKhoi)
                var giaiHoiThao = new GiaiDau
                {
                    Ma = "HSSV_2026",
                    Ten = "Đại Hội Thể Thao Học Sinh Sinh Viên Mở Rộng 2026",
                    Slug = "dai-hoi-the-thao-hoc-sinh-sinh-vien-mo-rong-2026",
                    MoTa = "Giải đấu thường niên dành cho sinh viên và thanh niên các trường đại học, cao đẳng.",
                    NgayBatDau = now.AddDays(-10),
                    NgayKetThuc = now.AddDays(20),
                    DiaDiem = "Trung tâm Văn hóa Thể thao Quận 10",
                    PhamVi = Dms.Domain.Enums.PhamViGiaiDau.TheoKhoi,
                    TrangThai = Dms.Domain.Enums.TrangThaiGiaiDau.DangDienRa
                };

                var giaiDoanhNghiep = new GiaiDau
                {
                    Ma = "CUP_DN_2026",
                    Ten = "Giải Thể Thao Doanh Nghiệp Trẻ Thành Phố 2026",
                    Slug = "giai-the-thao-doanh-nghiep-tre-thanh-pho-2026",
                    MoTa = "Tranh cúp giao lưu giữa các doanh nghiệp và tập đoàn công nghệ hàng đầu.",
                    NgayBatDau = now.AddDays(15),
                    NgayKetThuc = now.AddDays(30),
                    DiaDiem = "Nhà thi đấu Phú Thọ, TP.HCM",
                    PhamVi = Dms.Domain.Enums.PhamViGiaiDau.TatCa,
                    TrangThai = Dms.Domain.Enums.TrangThaiGiaiDau.SapDienRa
                };
                await context.GiaiDaus.AddRangeAsync(giaiHoiThao, giaiDoanhNghiep);
                await context.SaveChangesAsync();

                // Liên kết GiaiDau - Khoi
                var gdk1 = new GiaiDauKhoi { GiaiDauId = giaiHoiThao.Id, KhoiId = khoiTruongHoc.Id };
                var gdk2 = new GiaiDauKhoi { GiaiDauId = giaiDoanhNghiep.Id, KhoiId = khoiDoanhNghiep.Id };
                await context.GiaiDauKhois.AddRangeAsync(gdk1, gdk2);
                await context.SaveChangesAsync();

                // 6. GiaiDauMonTheThao (Đưa môn thể thao vào giải đấu)
                var gdmBongDa = new GiaiDauMonTheThao { GiaiDauId = giaiHoiThao.Id, MonTheThaoId = monBongDa.Id, MoTa = "Môn bóng đá nam sinh viên", TrangThai = true };
                var gdmCauLong = new GiaiDauMonTheThao { GiaiDauId = giaiHoiThao.Id, MonTheThaoId = monCauLong.Id, MoTa = "Môn cầu lông nam nữ", TrangThai = true };
                await context.GiaiDauMonTheThaos.AddRangeAsync(gdmBongDa, gdmCauLong);
                await context.SaveChangesAsync();

                // 7. Cụm Sân & Sân Đấu (CumSan, SanDau)
                var cumSanPhuTho = new CumSan { Ma = "CS_PHUTHO", Ten = "Khu liên hợp thể thao Phú Thọ", DiaChi = "Số 1 Lữ Gia, P.15, Q.11, TP.HCM", SoLuongSan = 5, TrangThai = true };
                await context.CumSans.AddAsync(cumSanPhuTho);
                await context.SaveChangesAsync();

                var sanBong1 = new SanDau { CumSanId = cumSanPhuTho.Id, Ma = "SAN_BONG_01", Ten = "Sân Bóng Đá 1 (Cỏ nhân tạo)", LoaiSan = "SanBongDa", SoSan = 1, TrangThai = true };
                var sanCauLong1 = new SanDau { CumSanId = cumSanPhuTho.Id, Ma = "SAN_CL_01", Ten = "Sân Cầu Lông A1", LoaiSan = "SanCauLong", SoSan = 2, TrangThai = true };
                await context.SanDaus.AddRangeAsync(sanBong1, sanCauLong1);
                await context.SaveChangesAsync();

                // 9. Trọng Tài (TrongTai)
                var tt1 = new TrongTai { Ma = "TT_001", HoTen = "Trần Trọng Tài Quốc Gia", GioiTinh = "Nam", SoDienThoai = "0988112233", Email = "ref1@sport.vn", CapBac = "Trọng tài cấp 1", TrangThai = true };
                var tt2 = new TrongTai { Ma = "TT_002", HoTen = "Lê Thị Bích Hạnh", GioiTinh = "Nu", SoDienThoai = "0988223344", Email = "ref2@sport.vn", CapBac = "Trọng tài FIFA", TrangThai = true };
                await context.TrongTais.AddRangeAsync(tt1, tt2);
                await context.SaveChangesAsync();

                // 10. Vận động viên & Đội (VanDongVien, Doi, ThanhVienDoi)
                var vdv1 = new VanDongVien { Ma = "VDV_001", HoTen = "Nguyễn Văn Quang", DonViId = dvBachKhoa.Id, GioiTinh = "Nam", NgaySinh = new DateTime(2002, 5, 27), SoDienThoai = "0911223344", Email = "quang@hcmut.edu.vn", TrangThai = true };
                var vdv2 = new VanDongVien { Ma = "VDV_002", HoTen = "Lê Hồng Phát", DonViId = dvBachKhoa.Id, GioiTinh = "Nam", NgaySinh = new DateTime(2003, 8, 15), SoDienThoai = "0911223345", Email = "phat@hcmut.edu.vn", TrangThai = true };
                var vdv3 = new VanDongVien { Ma = "VDV_003", HoTen = "Phạm Minh Triết", DonViId = dvKinhTe.Id, GioiTinh = "Nam", NgaySinh = new DateTime(2002, 11, 20), SoDienThoai = "0922334455", Email = "triet@ueh.edu.vn", TrangThai = true };
                var vdv4 = new VanDongVien { Ma = "VDV_004", HoTen = "Võ Hoàng Nam", DonViId = dvKinhTe.Id, GioiTinh = "Nam", NgaySinh = new DateTime(2004, 3, 10), SoDienThoai = "0922334456", Email = "nam@ueh.edu.vn", TrangThai = true };
                await context.VanDongViens.AddRangeAsync(vdv1, vdv2, vdv3, vdv4);
                await context.SaveChangesAsync();

                var doiBk = new Doi { Ma = "DOI_BK_FC", Ten = "FC Bách Khoa TP.HCM", DonViId = dvBachKhoa.Id, NguoiQuanLy = "Thầy Hùng", SoDienThoai = "0901234567", TrangThai = true };
                var doiUeh = new Doi { Ma = "DOI_UEH_FC", Ten = "FC Kinh Tế UEH", DonViId = dvKinhTe.Id, NguoiQuanLy = "Cô Mai", SoDienThoai = "0902345678", TrangThai = true };
                await context.Dois.AddRangeAsync(doiBk, doiUeh);
                await context.SaveChangesAsync();

                var tvd1 = new ThanhVienDoi { DoiId = doiBk.Id, VanDongVienId = vdv1.Id, SoAo = "10", ViTri = "Tiền đạo", LaDoiTruong = true, NgayThamGia = now.AddMonths(-3) };
                var tvd2 = new ThanhVienDoi { DoiId = doiBk.Id, VanDongVienId = vdv2.Id, SoAo = "07", ViTri = "Tiền vệ", LaDoiTruong = false, NgayThamGia = now.AddMonths(-3) };
                var tvd3 = new ThanhVienDoi { DoiId = doiUeh.Id, VanDongVienId = vdv3.Id, SoAo = "09", ViTri = "Tiền đạo", LaDoiTruong = true, NgayThamGia = now.AddMonths(-3) };
                var tvd4 = new ThanhVienDoi { DoiId = doiUeh.Id, VanDongVienId = vdv4.Id, SoAo = "01", ViTri = "Thủ môn", LaDoiTruong = false, NgayThamGia = now.AddMonths(-3) };
                await context.ThanhVienDois.AddRangeAsync(tvd1, tvd2, tvd3, tvd4);
                await context.SaveChangesAsync();

                // 11. Đăng ký thi đấu & Chi tiết đăng ký (DangKyThiDau, ChiTietDangKyThiDau)
                var dkBk = new DangKyThiDau { GiaiDauMonTheThaoId = gdmBongDa.Id, DoiId = doiBk.Id, SoDangKy = "DK_BK_BD", TenDangKy = "Đội tuyển Bóng đá ĐH Bách Khoa", TrangThai = "DaDuyet", NgayDangKy = now.AddDays(-15) };
                var dkUeh = new DangKyThiDau { GiaiDauMonTheThaoId = gdmBongDa.Id, DoiId = doiUeh.Id, SoDangKy = "DK_UEH_BD", TenDangKy = "Đội tuyển Bóng đá ĐH Kinh Tế", TrangThai = "DaDuyet", NgayDangKy = now.AddDays(-14) };
                var dkCauLong1 = new DangKyThiDau { GiaiDauMonTheThaoId = gdmCauLong.Id, SoDangKy = "DK_CL_VDV1", TenDangKy = "Nguyễn Văn Quang (BK)", TrangThai = "DaDuyet", NgayDangKy = now.AddDays(-12) };
                var dkCauLong2 = new DangKyThiDau { GiaiDauMonTheThaoId = gdmCauLong.Id, SoDangKy = "DK_CL_VDV3", TenDangKy = "Phạm Minh Triết (UEH)", TrangThai = "DaDuyet", NgayDangKy = now.AddDays(-12) };
                await context.DangKyThiDaus.AddRangeAsync(dkBk, dkUeh, dkCauLong1, dkCauLong2);
                await context.SaveChangesAsync();

               

                // 12. Bảng đấu & Thành viên bảng (BangDau, ThanhVienBang)
                var bangA = new BangDau { GiaiDauMonTheThaoId = gdmBongDa.Id, Ma = "BANG_A", Ten = "Bảng A Bóng Đá", ThuTu = 1 };
                await context.BangDaus.AddAsync(bangA);
                await context.SaveChangesAsync();

                var tvb1 = new ThanhVienBang { BangDauId = bangA.Id, DangKyThiDauId = dkBk.Id, HatGiong = 1, SoTran = 1, SoThang = 1, SoHoa = 0, SoThua = 0, DiemGhiDuoc = 3, DiemBiGhi = 1, Diem = 3, XepHang = 1 };
                var tvb2 = new ThanhVienBang { BangDauId = bangA.Id, DangKyThiDauId = dkUeh.Id, HatGiong = 2, SoTran = 1, SoThang = 0, SoHoa = 0, SoThua = 1, DiemGhiDuoc = 1, DiemBiGhi = 3, Diem = 0, XepHang = 2 };
                await context.ThanhVienBangs.AddRangeAsync(tvb1, tvb2);
                await context.SaveChangesAsync();

                // 13. Vòng đấu (VongDau)
                var vongBang = new VongDau { GiaiDauMonTheThaoId = gdmBongDa.Id, Ten = "Vòng Bảng", LoaiVong = "VongBang", ThuTu = 1 };
                var vongChungKet = new VongDau { GiaiDauMonTheThaoId = gdmBongDa.Id, Ten = "Trận Chung Kết", LoaiVong = "ChungKet", ThuTu = 2 };
                await context.VongDaus.AddRangeAsync(vongBang, vongChungKet);
                await context.SaveChangesAsync();

                // 14. Trận đấu & Phân công trọng tài (TranDau, PhanCongTrongTai)
                var tranBong1 = new TranDau
                {
                    GiaiDauMonTheThaoId = gdmBongDa.Id,
                    VongDauId = vongBang.Id,
                    BangDauId = bangA.Id,
                    SanDauId = sanBong1.Id,
                    SoTran = 1,
                    TenTran = "Bách Khoa vs Kinh Tế (Lượt 1 Bảng A)",
                    ThoiGianDuKien = now.AddDays(-2),
                    ThoiGianBatDau = now.AddDays(-2).AddHours(8),
                    ThoiGianKetThuc = now.AddDays(-2).AddHours(9).AddMinutes(30),
                    TrangThai = "KetThuc",
                    GhiChu = "Trận đấu sôi nổi, thời tiết đẹp"
                };
                await context.TranDaus.AddAsync(tranBong1);
                await context.SaveChangesAsync();

                var pc1 = new PhanCongTrongTai { TranDauId = tranBong1.Id, TrongTaiId = tt1.Id, VaiTro = "Trọng tài chính", GhiChu = "Điều hành tốt trận đấu" };
                var pc2 = new PhanCongTrongTai { TranDauId = tranBong1.Id, TrongTaiId = tt2.Id, VaiTro = "Trọng tài bàn", GhiChu = "Ghi chép biên bản chính xác" };
                await context.PhanCongTrongTais.AddRangeAsync(pc1, pc2);
                await context.SaveChangesAsync();

                // 15. Thành phần trận đấu (ThanhPhanTranDau)
                var tp1 = new ThanhPhanTranDau { TranDauId = tranBong1.Id, DangKyThiDauId = dkBk.Id, ViTri = 1, TrangThai = "ThamGia" };
                var tp2 = new ThanhPhanTranDau { TranDauId = tranBong1.Id, DangKyThiDauId = dkUeh.Id, ViTri = 2, TrangThai = "ThamGia" };
                await context.ThanhPhanTranDaus.AddRangeAsync(tp1, tp2);
                await context.SaveChangesAsync();

                // 16. Hiệp đấu (HiepDau)
                var hiep1 = new HiepDau { TranDauId = tranBong1.Id, SoHiep = 1, ThoiGianBatDau = tranBong1.ThoiGianBatDau, ThoiGianKetThuc = tranBong1.ThoiGianBatDau?.AddMinutes(35), TrangThai = "KetThuc" };
                var hiep2 = new HiepDau { TranDauId = tranBong1.Id, SoHiep = 2, ThoiGianBatDau = tranBong1.ThoiGianBatDau?.AddMinutes(45), ThoiGianKetThuc = tranBong1.ThoiGianKetThuc, TrangThai = "KetThuc" };
                await context.HiepDaus.AddRangeAsync(hiep1, hiep2);
                await context.SaveChangesAsync();

                // 17. Kết quả hiệp đấu (KetQuaHiepDau)
                var kqHiep1_Tp1 = new KetQuaHiepDau { HiepDauId = hiep1.Id, ThanhPhanTranDauId = tp1.Id, Diem = 2, GhiChu = "Ghi 2 bàn trong hiệp 1" };
                var kqHiep1_Tp2 = new KetQuaHiepDau { HiepDauId = hiep1.Id, ThanhPhanTranDauId = tp2.Id, Diem = 0 };
                var kqHiep2_Tp1 = new KetQuaHiepDau { HiepDauId = hiep2.Id, ThanhPhanTranDauId = tp1.Id, Diem = 1, GhiChu = "Ghi thêm 1 bàn trong hiệp 2" };
                var kqHiep2_Tp2 = new KetQuaHiepDau { HiepDauId = hiep2.Id, ThanhPhanTranDauId = tp2.Id, Diem = 1, GhiChu = "Gỡ lại 1 bàn danh dự" };
                await context.KetQuaHiepDaus.AddRangeAsync(kqHiep1_Tp1, kqHiep1_Tp2, kqHiep2_Tp1, kqHiep2_Tp2);
                await context.SaveChangesAsync();

                // 18. Kết quả chung cuộc trận đấu (KetQuaTranDau)
                var kqTran_Tp1 = new KetQuaTranDau { ThanhPhanTranDauId = tp1.Id, LoaiKetQua = "Thang", Diem = 3, XepHang = 1, KetQuaText = "Thắng 3 - 1" };
                var kqTran_Tp2 = new KetQuaTranDau { ThanhPhanTranDauId = tp2.Id, LoaiKetQua = "Thua", Diem = 1, XepHang = 2, KetQuaText = "Thua 1 - 3" };
                await context.KetQuaTranDaus.AddRangeAsync(kqTran_Tp1, kqTran_Tp2);
                await context.SaveChangesAsync();

                // 19. Trao Huy Chương (HuyChuong)
                var huyChuongVang = new HuyChuong
                {
                    GiaiDauId = giaiHoiThao.Id,
                    GiaiDauMonTheThaoId = gdmBongDa.Id,
                    DangKyThiDauId = dkBk.Id,
                    LoaiHuyChuongId = lhcVang.Id,
                    XepHang = 1,
                    NgayTrao = now,
                    GhiChu = "Nhà vô địch Bóng đá nam SV 2026"
                };
                var huyChuongBac = new HuyChuong
                {
                    GiaiDauId = giaiHoiThao.Id,
                    GiaiDauMonTheThaoId = gdmBongDa.Id,
                    DangKyThiDauId = dkUeh.Id,
                    LoaiHuyChuongId = lhcBac.Id,
                    XepHang = 2,
                    NgayTrao = now,
                    GhiChu = "Á quân Bóng đá nam SV 2026"
                };
                await context.HuyChuongs.AddRangeAsync(huyChuongVang, huyChuongBac);
                await context.SaveChangesAsync();

                // 20. Lịch sử chuyển đội thi đấu (LichSuChuyenDoi)
                var lsChuyenDoi1 = new LichSuChuyenDoi
                {
                    VanDongVienId = vdv2.Id,
                    DoiCuId = doiBk.Id,
                    DoiMoiId = doiUeh.Id,
                    NgayChuyen = now.AddDays(-5),
                    LyDo = "Chuyển đơn vị học tập và giao lưu thi đấu theo thỏa thuận",
                    NguoiXacNhan = "Ban Tổ Chức Giải",
                    GhiChu = "Đã hoàn tất hồ sơ và thủ tục chuyển nhượng hợp lệ"
                };
                await context.LichSuChuyenDois.AddAsync(lsChuyenDoi1);
                await context.SaveChangesAsync();

                // 21. Điều lệ giải đấu (DieuLeGiaiDau)
                var dlGiai1 = new DieuLeGiaiDau
                {
                    GiaiDauId = giaiHoiThao.Id,
                    TieuDe = "Quy định đối tượng và điều kiện tham gia",
                    NoiDung = "Vận động viên phải là sinh viên đang theo học chính quy tại các trường đại học, cao đẳng trên địa bàn TP.HCM, có thẻ sinh viên hợp lệ và đủ điều kiện sức khỏe tham gia thi đấu.",
                    ThuTu = 1,
                    TrangThai = true
                };
                var dlGiai2 = new DieuLeGiaiDau
                {
                    GiaiDauId = giaiHoiThao.Id,
                    TieuDe = "Quy định khen thưởng và kỷ luật",
                    NoiDung = "Ban Tổ chức trao cờ, huy chương Vàng, Bạc, Đồng và tiền thưởng cho các đội đạt thứ hạng Nhất, Nhì, Ba. Vận động viên hoặc đội bóng vi phạm tinh thần thể thao sẽ bị xử lý kỷ luật theo quy định.",
                    ThuTu = 2,
                    TrangThai = true
                };
                await context.DieuLeGiaiDaus.AddRangeAsync(dlGiai1, dlGiai2);
                await context.SaveChangesAsync();

                // 22. Điều lệ môn thể thao (DieuLeMonTheThao)
                var dlMonBongDa = new DieuLeMonTheThao
                {
                    MonTheThaoId = monBongDa.Id,
                    TieuDe = "Luật thi đấu Bóng đá mini 7 người",
                    NoiDung = "Áp dụng Luật thi đấu bóng đá 7 người do Liên đoàn Bóng đá Việt Nam (VFF) ban hành. Mỗi trận gồm 2 hiệp, mỗi hiệp 25 phút, nghỉ giữa hiệp 10 phút. Không áp dụng luật việt vị.",
                    ThuTu = 1,
                    TrangThai = true
                };
                var dlMonCauLong = new DieuLeMonTheThao
                {
                    MonTheThaoId = monCauLong.Id,
                    TieuDe = "Luật thi đấu Cầu lông hiện hành",
                    NoiDung = "Áp dụng theo Luật Cầu lông hiện hành của Liên đoàn Cầu lông Thế giới (BWF). Thi đấu theo thể thức 3 hiệp thắng 2, mỗi hiệp 21 điểm (rallies point scoring system).",
                    ThuTu = 1,
                    TrangThai = true
                };
                await context.DieuLeMonTheThaos.AddRangeAsync(dlMonBongDa, dlMonCauLong);
                await context.SaveChangesAsync();
            }

            // 23. Cấu hình thể thức thi đấu (CauHinhTheThucThiDau) mặc định cho các môn thể thao
            if (!context.CauHinhTheThucThiDaus.Any())
            {
                var mons = context.MonTheThaos.ToList();
                var configs = new List<CauHinhTheThucThiDau>();

                foreach (var mon in mons)
                {
                    switch (mon.Ma)
                    {
                        case "BONG_DA":
                            configs.Add(new CauHinhTheThucThiDau
                            {
                                MonTheThaoId = mon.Id,
                                LoaiTheThuc = "ThoiGianHiep",
                                SoHiepToiDa = 2,
                                ThoiGianHiepChinhPhut = 25,
                                ChoPhepHoaVongBang = true,
                                ChoPhepHoaKnockout = false,
                                CoHiepPhu = false,
                                CoPenalty = true,
                                SoLuotPenaltyMoiDoi = 5,
                                CoThePhat = true,
                                DiemThang = 3,
                                DiemHoa = 1,
                                DiemThua = 0,
                                CachTinhDiemTheoSet = false,
                                TieuChiXepHangJson = "[\"Diem\",\"HieuSo\",\"DiemGhiDuoc\",\"DoiDau\",\"SoTranThang\"]"
                            });
                            break;

                        case "CAU_LONG":
                            configs.Add(new CauHinhTheThucThiDau
                            {
                                MonTheThaoId = mon.Id,
                                LoaiTheThuc = "SetDiem",
                                SoHiepToiDa = 3,
                                SoHiepThangDeThangTran = 2,
                                DiemMoiHiep = 21,
                                DiemHiepQuyetDinh = 21,
                                CachBietDiemToiThieu = 2,
                                DiemToiDaMoiHiep = 30,
                                ChoPhepHoaVongBang = false,
                                ChoPhepHoaKnockout = false,
                                DiemThang = 2,
                                DiemHoa = 0,
                                DiemThua = 0,
                                CachTinhDiemTheoSet = false,
                                TieuChiXepHangJson = "[\"Diem\",\"HieuSo\",\"DoiDau\",\"DiemGhiDuoc\"]"
                            });
                            break;

                        case "BONG_CHUYEN":
                            configs.Add(new CauHinhTheThucThiDau
                            {
                                MonTheThaoId = mon.Id,
                                LoaiTheThuc = "SetDiem",
                                SoHiepToiDa = 5,
                                SoHiepThangDeThangTran = 3,
                                DiemMoiHiep = 25,
                                DiemHiepQuyetDinh = 15,
                                CachBietDiemToiThieu = 2,
                                DiemToiDaMoiHiep = null,
                                ChoPhepHoaVongBang = false,
                                ChoPhepHoaKnockout = false,
                                CachTinhDiemTheoSet = true,
                                DiemThang = 3,
                                DiemHoa = 0,
                                DiemThua = 0,
                                TieuChiXepHangJson = "[\"Diem\",\"HieuSo\",\"DoiDau\",\"SoTranThang\"]"
                            });
                            break;

                        case "BONG_BAN":
                        case "QUAN_VOT":
                        case "PICKLEBALL":
                            configs.Add(new CauHinhTheThucThiDau
                            {
                                MonTheThaoId = mon.Id,
                                LoaiTheThuc = "SetDiem",
                                SoHiepToiDa = 5,
                                SoHiepThangDeThangTran = 3,
                                DiemMoiHiep = 11,
                                DiemHiepQuyetDinh = 11,
                                CachBietDiemToiThieu = 2,
                                ChoPhepHoaVongBang = false,
                                ChoPhepHoaKnockout = false,
                                DiemThang = 2,
                                DiemHoa = 0,
                                DiemThua = 0,
                                TieuChiXepHangJson = "[\"Diem\",\"HieuSo\",\"DoiDau\"]"
                            });
                            break;

                        case "BOI_LOI":
                            configs.Add(new CauHinhTheThucThiDau
                            {
                                MonTheThaoId = mon.Id,
                                LoaiTheThuc = "TinhDiemXepHang",
                                LoaiDoThanhTich = "ThoiGian",
                                DonViThanhTich = "giay",
                                TieuChiXepHangThanhTich = "CangNhoCangTot",
                                SoVdvMoiLuotThi = 8,
                                QuyCachTienVaoChungKet = "TopNToanVong",
                                SoVdvVaoChungKet = 8,
                                KyLucHienTai = 24.50m,
                                KyLucHienTaiText = "24.50s",
                                TieuChiXepHangJson = "[\"ThoiGian\"]"
                            });
                            break;

                        case "CHAY_100M":
                            configs.Add(new CauHinhTheThucThiDau
                            {
                                MonTheThaoId = mon.Id,
                                LoaiTheThuc = "TinhDiemXepHang",
                                LoaiDoThanhTich = "ThoiGian",
                                DonViThanhTich = "giay",
                                TieuChiXepHangThanhTich = "CangNhoCangTot",
                                SoVdvMoiLuotThi = 8,
                                QuyCachTienVaoChungKet = "TopNToanVong",
                                SoVdvVaoChungKet = 8,
                                KyLucHienTai = 10.15m,
                                KyLucHienTaiText = "10.15s",
                                TieuChiXepHangJson = "[\"ThoiGian\"]"
                            });
                            break;

                        default:
                            configs.Add(new CauHinhTheThucThiDau
                            {
                                MonTheThaoId = mon.Id,
                                LoaiTheThuc = "SetDiem",
                                SoHiepToiDa = 3,
                                SoHiepThangDeThangTran = 2,
                                DiemMoiHiep = 21,
                                CachBietDiemToiThieu = 2,
                                ChoPhepHoaVongBang = false,
                                ChoPhepHoaKnockout = false,
                                DiemThang = 2,
                                DiemHoa = 0,
                                DiemThua = 0,
                                TieuChiXepHangJson = "[\"Diem\",\"HieuSo\",\"DoiDau\"]"
                            });
                            break;
                    }
                }

                if (configs.Any())
                {
                    await context.CauHinhTheThucThiDaus.AddRangeAsync(configs);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}


