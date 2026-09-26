using Dms.Domain.Entities;
using Dms.Domain.Interfaces;
using Dms.Infrastructure.Persistence;

namespace Dms.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IGenericRepository<Menu>? _menus;
        private IGenericRepository<SystemSetting>? _systemSettings;

        // Vietnamese Sport Tournament Repositories
        private IGenericRepository<Khoi>? _khois;
        private IGenericRepository<DonVi>? _donVis;
        private IGenericRepository<GiaiDau>? _giaiDaus;
        private IGenericRepository<GiaiDauKhoi>? _giaiDauKhois;
        private IGenericRepository<DanhMucMonTheThao>? _danhMucMonTheThaos;
        private IGenericRepository<MonTheThao>? _monTheThaos;
        private IGenericRepository<GiaiDauMonTheThao>? _giaiDauMonTheThaos;
        private IGenericRepository<PhanCongDieuHanhMon>? _phanCongDieuHanhMons;
        private IGenericRepository<NguoiDieuHanhMon>? _nguoiDieuHanhMons;
        private IGenericRepository<ApplicationUser>? _applicationUsers;
        private IGenericRepository<VanDongVien>? _vanDongViens;
        private IGenericRepository<Doi>? _dois;
        private IGenericRepository<ThanhVienDoi>? _thanhVienDois;
        private IGenericRepository<DangKyThiDau>? _dangKyThiDaus;
        private IGenericRepository<BangDau>? _bangDaus;
        private IGenericRepository<ThanhVienBang>? _thanhVienBangs;
        private IGenericRepository<VongDau>? _vongDaus;
        private IGenericRepository<CumSan>? _cumSans;
        private IGenericRepository<SanDau>? _sanDaus;
        private IGenericRepository<TrongTai>? _trongTais;
        private IGenericRepository<ThuKy>? _thuKys;
        private IGenericRepository<PhanCongThuKy>? _phanCongThuKys;
        private IGenericRepository<TranDau>? _tranDaus;
        private IGenericRepository<ThanhPhanTranDau>? _thanhPhanTranDaus;
        private IGenericRepository<HiepDau>? _hiepDaus;
        private IGenericRepository<KetQuaHiepDau>? _ketQuaHiepDaus;
        private IGenericRepository<KetQuaTranDau>? _ketQuaTranDaus;
        private IGenericRepository<PhanCongTrongTai>? _phanCongTrongTais;
        private IGenericRepository<LoaiHuyChuong>? _loaiHuyChuongs;
        private IGenericRepository<HuyChuong>? _huyChuongs;
        private IGenericRepository<LichSuChuyenDoi>? _lichSuChuyenDois;
        private IGenericRepository<DieuLeGiaiDau>? _dieuLeGiaiDaus;
        private IGenericRepository<DieuLeMonTheThao>? _dieuLeMonTheThaos;
        private IGenericRepository<CauHinhLichThiDau>? _cauHinhLichThiDaus;
        private IGenericRepository<CauHinhTheThucThiDau>? _cauHinhTheThucThiDaus;
        private IGenericRepository<SuCoDieuHanhMon>? _suCoDieuHanhMons;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Menu> Menus => 
            _menus ??= new GenericRepository<Menu>(_context);

        public IGenericRepository<SystemSetting> SystemSettings => 
            _systemSettings ??= new GenericRepository<SystemSetting>(_context);

        public IGenericRepository<Khoi> Khois => 
            _khois ??= new GenericRepository<Khoi>(_context);

        public IGenericRepository<DonVi> DonVis => 
            _donVis ??= new GenericRepository<DonVi>(_context);

        public IGenericRepository<GiaiDau> GiaiDaus => 
            _giaiDaus ??= new GenericRepository<GiaiDau>(_context);

        public IGenericRepository<GiaiDauKhoi> GiaiDauKhois => 
            _giaiDauKhois ??= new GenericRepository<GiaiDauKhoi>(_context);

        public IGenericRepository<DanhMucMonTheThao> DanhMucMonTheThaos => 
            _danhMucMonTheThaos ??= new GenericRepository<DanhMucMonTheThao>(_context);

        public IGenericRepository<MonTheThao> MonTheThaos => 
            _monTheThaos ??= new GenericRepository<MonTheThao>(_context);

        public IGenericRepository<GiaiDauMonTheThao> GiaiDauMonTheThaos => 
            _giaiDauMonTheThaos ??= new GenericRepository<GiaiDauMonTheThao>(_context);

        public IGenericRepository<PhanCongDieuHanhMon> PhanCongDieuHanhMons =>
            _phanCongDieuHanhMons ??= new GenericRepository<PhanCongDieuHanhMon>(_context);

        public IGenericRepository<NguoiDieuHanhMon> NguoiDieuHanhMons =>
            _nguoiDieuHanhMons ??= new GenericRepository<NguoiDieuHanhMon>(_context);

        public IGenericRepository<ApplicationUser> ApplicationUsers =>
            _applicationUsers ??= new GenericRepository<ApplicationUser>(_context);

      

        public IGenericRepository<VanDongVien> VanDongViens => 
            _vanDongViens ??= new GenericRepository<VanDongVien>(_context);

        public IGenericRepository<Doi> Dois => 
            _dois ??= new GenericRepository<Doi>(_context);

        public IGenericRepository<ThanhVienDoi> ThanhVienDois => 
            _thanhVienDois ??= new GenericRepository<ThanhVienDoi>(_context);

        public IGenericRepository<DangKyThiDau> DangKyThiDaus => 
            _dangKyThiDaus ??= new GenericRepository<DangKyThiDau>(_context);

     

        public IGenericRepository<BangDau> BangDaus => 
            _bangDaus ??= new GenericRepository<BangDau>(_context);

        public IGenericRepository<ThanhVienBang> ThanhVienBangs => 
            _thanhVienBangs ??= new GenericRepository<ThanhVienBang>(_context);

        public IGenericRepository<VongDau> VongDaus => 
            _vongDaus ??= new GenericRepository<VongDau>(_context);

        public IGenericRepository<CumSan> CumSans => 
            _cumSans ??= new GenericRepository<CumSan>(_context);

        public IGenericRepository<SanDau> SanDaus => 
            _sanDaus ??= new GenericRepository<SanDau>(_context);

        public IGenericRepository<TrongTai> TrongTais => 
            _trongTais ??= new GenericRepository<TrongTai>(_context);

        public IGenericRepository<ThuKy> ThuKys => 
            _thuKys ??= new GenericRepository<ThuKy>(_context);

        public IGenericRepository<PhanCongThuKy> PhanCongThuKys =>
            _phanCongThuKys ??= new GenericRepository<PhanCongThuKy>(_context);

        public IGenericRepository<TranDau> TranDaus => 
            _tranDaus ??= new GenericRepository<TranDau>(_context);

        public IGenericRepository<ThanhPhanTranDau> ThanhPhanTranDaus => 
            _thanhPhanTranDaus ??= new GenericRepository<ThanhPhanTranDau>(_context);

        public IGenericRepository<HiepDau> HiepDaus => 
            _hiepDaus ??= new GenericRepository<HiepDau>(_context);

        public IGenericRepository<KetQuaHiepDau> KetQuaHiepDaus => 
            _ketQuaHiepDaus ??= new GenericRepository<KetQuaHiepDau>(_context);

        public IGenericRepository<KetQuaTranDau> KetQuaTranDaus => 
            _ketQuaTranDaus ??= new GenericRepository<KetQuaTranDau>(_context);

        public IGenericRepository<PhanCongTrongTai> PhanCongTrongTais => 
            _phanCongTrongTais ??= new GenericRepository<PhanCongTrongTai>(_context);

        public IGenericRepository<LoaiHuyChuong> LoaiHuyChuongs => 
            _loaiHuyChuongs ??= new GenericRepository<LoaiHuyChuong>(_context);

        public IGenericRepository<HuyChuong> HuyChuongs => 
            _huyChuongs ??= new GenericRepository<HuyChuong>(_context);

        public IGenericRepository<LichSuChuyenDoi> LichSuChuyenDois => 
            _lichSuChuyenDois ??= new GenericRepository<LichSuChuyenDoi>(_context);

        public IGenericRepository<DieuLeGiaiDau> DieuLeGiaiDaus => 
            _dieuLeGiaiDaus ??= new GenericRepository<DieuLeGiaiDau>(_context);

        public IGenericRepository<DieuLeMonTheThao> DieuLeMonTheThaos => 
            _dieuLeMonTheThaos ??= new GenericRepository<DieuLeMonTheThao>(_context);

        public IGenericRepository<CauHinhLichThiDau> CauHinhLichThiDaus => 
            _cauHinhLichThiDaus ??= new GenericRepository<CauHinhLichThiDau>(_context);

        public IGenericRepository<CauHinhTheThucThiDau> CauHinhTheThucThiDaus => 
            _cauHinhTheThucThiDaus ??= new GenericRepository<CauHinhTheThucThiDau>(_context);

        public IGenericRepository<SuCoDieuHanhMon> SuCoDieuHanhMons =>
            _suCoDieuHanhMons ??= new GenericRepository<SuCoDieuHanhMon>(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
