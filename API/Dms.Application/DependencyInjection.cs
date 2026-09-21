using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dms.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddScoped<Dms.Application.Interfaces.IMenuService, Dms.Application.Services.MenuService>();
            services.AddScoped<Dms.Application.Interfaces.IGiaiDauService, Dms.Application.Services.GiaiDauService>();
            services.AddScoped<Dms.Application.Interfaces.IKhoiService, Dms.Application.Services.KhoiService>();
            services.AddScoped<Dms.Application.Interfaces.IDonViService, Dms.Application.Services.DonViService>();
            services.AddScoped<Dms.Application.Interfaces.IDanhMucMonTheThaoService, Dms.Application.Services.DanhMucMonTheThaoService>();
            services.AddScoped<Dms.Application.Interfaces.IMonTheThaoService, Dms.Application.Services.MonTheThaoService>();
            services.AddScoped<Dms.Application.Interfaces.ITrongTaiService, Dms.Application.Services.TrongTaiService>();
            services.AddScoped<Dms.Application.Interfaces.IThuKyService, Dms.Application.Services.ThuKyService>();
            services.AddScoped<Dms.Application.Interfaces.ICumSanService, Dms.Application.Services.CumSanService>();
            services.AddScoped<Dms.Application.Interfaces.ISanDauService, Dms.Application.Services.SanDauService>();
            services.AddScoped<Dms.Application.Interfaces.ILoaiHuyChuongService, Dms.Application.Services.LoaiHuyChuongService>();
            services.AddScoped<Dms.Application.Interfaces.IVanDongVienService, Dms.Application.Services.VanDongVienService>();
            services.AddScoped<Dms.Application.Interfaces.IDangKyThiDauService, Dms.Application.Services.DangKyThiDauService>();
            services.AddScoped<Dms.Application.Interfaces.ITranDauService, Dms.Application.Services.TranDauService>();
            services.AddScoped<Dms.Application.Interfaces.IBangDauService, Dms.Application.Services.BangDauService>();
            services.AddScoped<Dms.Application.Interfaces.IVongDauService, Dms.Application.Services.VongDauService>();
            services.AddScoped<Dms.Application.Interfaces.ICauHinhLichThiDauService, Dms.Application.Services.CauHinhLichThiDauService>();
            services.AddScoped<Dms.Application.Interfaces.ITruongBanTrongTaiService, Dms.Application.Services.TruongBanTrongTaiService>();
            services.AddScoped<Dms.Application.Interfaces.IDieuHanhMonService, Dms.Application.Services.DieuHanhMonService>();
            services.AddScoped<Dms.Application.Interfaces.IThuKyGiaiService, Dms.Application.Services.ThuKyGiaiService>();
            services.AddScoped<Dms.Application.Interfaces.ICauHinhTheThucService, Dms.Application.Services.CauHinhTheThucService>();
            services.AddScoped<Dms.Application.Interfaces.IMatchScoringEngine, Dms.Application.Services.MatchScoringEngine>();
            services.AddScoped<Dms.Application.Interfaces.IGroupStandingsEngine, Dms.Application.Services.GroupStandingsEngine>();
            services.AddScoped<Dms.Application.Interfaces.IKnockoutProgressionEngine, Dms.Application.Services.KnockoutProgressionEngine>();
            services.AddScoped<Dms.Application.Interfaces.IAthleticsProgressionEngine, Dms.Application.Services.AthleticsProgressionEngine>();
            
            return services;
        }
    }
}
