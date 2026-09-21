using Dms.Domain.Entities;

namespace Dms.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Menu> Menus { get; }
        IGenericRepository<SystemSetting> SystemSettings { get; }

        // Vietnamese Sport Tournament Repositories
        IGenericRepository<Khoi> Khois { get; }
        IGenericRepository<DonVi> DonVis { get; }
        IGenericRepository<GiaiDau> GiaiDaus { get; }
        IGenericRepository<GiaiDauKhoi> GiaiDauKhois { get; }
        IGenericRepository<DanhMucMonTheThao> DanhMucMonTheThaos { get; }
        IGenericRepository<MonTheThao> MonTheThaos { get; }
        IGenericRepository<GiaiDauMonTheThao> GiaiDauMonTheThaos { get; }
        IGenericRepository<NoiDungThiDau> NoiDungThiDaus { get; }
        IGenericRepository<VanDongVien> VanDongViens { get; }
        IGenericRepository<Doi> Dois { get; }
        IGenericRepository<ThanhVienDoi> ThanhVienDois { get; }
        IGenericRepository<DangKyThiDau> DangKyThiDaus { get; }
        IGenericRepository<ChiTietDangKyThiDau> ChiTietDangKyThiDaus { get; }
        IGenericRepository<BangDau> BangDaus { get; }
        IGenericRepository<ThanhVienBang> ThanhVienBangs { get; }
        IGenericRepository<VongDau> VongDaus { get; }
        IGenericRepository<CumSan> CumSans { get; }
        IGenericRepository<SanDau> SanDaus { get; }
        IGenericRepository<TrongTai> TrongTais { get; }
        IGenericRepository<ThuKy> ThuKys { get; }
        IGenericRepository<TranDau> TranDaus { get; }
        IGenericRepository<ThanhPhanTranDau> ThanhPhanTranDaus { get; }
        IGenericRepository<HiepDau> HiepDaus { get; }
        IGenericRepository<KetQuaHiepDau> KetQuaHiepDaus { get; }
        IGenericRepository<KetQuaTranDau> KetQuaTranDaus { get; }
        IGenericRepository<PhanCongTrongTai> PhanCongTrongTais { get; }
        IGenericRepository<LoaiHuyChuong> LoaiHuyChuongs { get; }
        IGenericRepository<HuyChuong> HuyChuongs { get; }
        IGenericRepository<LichSuChuyenDoi> LichSuChuyenDois { get; }
        IGenericRepository<DieuLeGiaiDau> DieuLeGiaiDaus { get; }
        IGenericRepository<DieuLeMonTheThao> DieuLeMonTheThaos { get; }
        IGenericRepository<CauHinhLichThiDau> CauHinhLichThiDaus { get; }
        IGenericRepository<CauHinhTheThucThiDau> CauHinhTheThucThiDaus { get; }

        Task<int> CompleteAsync();
    }
}
