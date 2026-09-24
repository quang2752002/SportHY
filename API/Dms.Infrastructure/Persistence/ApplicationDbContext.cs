using Dms.Domain.Common;
using Dms.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dms.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

        // New Vietnamese Sport Tournament Schema Entities
        public DbSet<Khoi> Khois => Set<Khoi>();
        public DbSet<DonVi> DonVis => Set<DonVi>();
        public DbSet<GiaiDau> GiaiDaus => Set<GiaiDau>();
        public DbSet<GiaiDauKhoi> GiaiDauKhois => Set<GiaiDauKhoi>();
        public DbSet<DanhMucMonTheThao> DanhMucMonTheThaos => Set<DanhMucMonTheThao>();
        public DbSet<MonTheThao> MonTheThaos => Set<MonTheThao>();
        public DbSet<GiaiDauMonTheThao> GiaiDauMonTheThaos => Set<GiaiDauMonTheThao>();
        public DbSet<VanDongVien> VanDongViens => Set<VanDongVien>();
        public DbSet<Doi> Dois => Set<Doi>();
        public DbSet<ThanhVienDoi> ThanhVienDois => Set<ThanhVienDoi>();
        public DbSet<DangKyThiDau> DangKyThiDaus => Set<DangKyThiDau>();
        public DbSet<BangDau> BangDaus => Set<BangDau>();
        public DbSet<ThanhVienBang> ThanhVienBangs => Set<ThanhVienBang>();
        public DbSet<VongDau> VongDaus => Set<VongDau>();
        public DbSet<CumSan> CumSans => Set<CumSan>();
        public DbSet<SanDau> SanDaus => Set<SanDau>();
        public DbSet<TrongTai> TrongTais => Set<TrongTai>();
        public DbSet<ThuKy> ThuKys => Set<ThuKy>();
        public DbSet<PhanCongThuKy> PhanCongThuKys => Set<PhanCongThuKy>();
        public DbSet<TranDau> TranDaus => Set<TranDau>();
        public DbSet<ThanhPhanTranDau> ThanhPhanTranDaus => Set<ThanhPhanTranDau>();
        public DbSet<HiepDau> HiepDaus => Set<HiepDau>();
        public DbSet<KetQuaHiepDau> KetQuaHiepDaus => Set<KetQuaHiepDau>();
        public DbSet<KetQuaTranDau> KetQuaTranDaus => Set<KetQuaTranDau>();
        public DbSet<PhanCongTrongTai> PhanCongTrongTais => Set<PhanCongTrongTai>();
        public DbSet<LoaiHuyChuong> LoaiHuyChuongs => Set<LoaiHuyChuong>();
        public DbSet<HuyChuong> HuyChuongs => Set<HuyChuong>();
        public DbSet<LichSuChuyenDoi> LichSuChuyenDois => Set<LichSuChuyenDoi>();
        public DbSet<DieuLeGiaiDau> DieuLeGiaiDaus => Set<DieuLeGiaiDau>();
        public DbSet<DieuLeMonTheThao> DieuLeMonTheThaos => Set<DieuLeMonTheThao>();
        public DbSet<CauHinhLichThiDau> CauHinhLichThiDaus => Set<CauHinhLichThiDau>();
        public DbSet<CauHinhTheThucThiDau> CauHinhTheThucThiDaus => Set<CauHinhTheThucThiDau>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           

            // Đổi tên các bảng Identity thành tên thân thiện hơn
            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole<int>>().ToTable("Roles");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<int>>().ToTable("UserTokens");

            // Cấu hình lưu Enum của GiaiDau dưới dạng chuỗi (VARCHAR) trong DB để tương thích SQL Server
            modelBuilder.Entity<GiaiDau>()
                .Property(g => g.PhamVi)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<GiaiDau>()
                .Property(g => g.TrangThai)
                .HasConversion<string>()
                .HasMaxLength(30);

            modelBuilder.Entity<GiaiDau>()
                .HasIndex(g => g.Slug);

            modelBuilder.Entity<PhanCongThuKy>()
                .HasIndex(p => new { p.GiaiDauId, p.ThuKyId })
                .IsUnique();

            // Cấu hình lưu Enum của MonTheThao dưới dạng chuỗi (VARCHAR) trong DB
            modelBuilder.Entity<MonTheThao>()
                .Property(m => m.HinhThucThiDau)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Tắt Cascade Delete cho toàn bộ Foreign Keys để tránh lỗi chu trình SQL Server (Error 1785: multiple cascade paths)
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = DateTime.UtcNow;
                        if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                        {
                            entry.Entity.CreatedBy = "System";
                        }
                        if (!entry.Entity.IsDeleted.HasValue)
                        {
                            entry.Entity.IsDeleted = false;
                        }
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModified = DateTime.UtcNow;
                        if (string.IsNullOrEmpty(entry.Entity.LastModifiedBy))
                        {
                            entry.Entity.LastModifiedBy = "System";
                        }
                        break;
                    case EntityState.Deleted:
                        // Tự động chặn xóa cứng (Hard Delete), chuyển thành xóa mềm (Soft Delete)
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.LastModified = DateTime.UtcNow;
                        break;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
