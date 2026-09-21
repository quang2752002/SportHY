# DB
/* ============================================================
   DATABASE: QUAN LY GIAI DAU THE THAO
   SQL SERVER
   ============================================================ */

IF DB_ID(N'QuanLyGiaiDauTheThao') IS NULL
BEGIN
    CREATE DATABASE QuanLyGiaiDauTheThao;
END
GO

USE QuanLyGiaiDauTheThao;
GO

/* ============================================================
   1. KHOI
   Ví d?:
   - Giáo d?c
   - Y t?
   - V?n hóa
   - Công an
   - Quân ??i
   ============================================================ */

IF OBJECT_ID('dbo.Khoi', 'U') IS NOT NULL
    DROP TABLE dbo.Khoi;
GO

CREATE TABLE Khoi
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(200) NOT NULL,
    MoTa            NVARCHAR(1000) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao        DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat    DATETIME2 NULL,

    CONSTRAINT UQ_Khoi_Ma UNIQUE (Ma)
);
GO


/* ============================================================
   2. DON VI
   Có th? phân c?p:
   
   Kh?i Giáo d?c
      ??? S? GD
      ??? Tr??ng A
      ??? Tr??ng B

   ??n v? có th? có ??n v? cha.
   ============================================================ */

IF OBJECT_ID('dbo.DonVi', 'U') IS NOT NULL
    DROP TABLE dbo.DonVi;
GO

CREATE TABLE DonVi
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(300) NOT NULL,

    KhoiId          BIGINT NULL,
    DonViChaId      BIGINT NULL,

    LoaiDonVi       VARCHAR(30) NULL,

    DiaChi          NVARCHAR(500) NULL,
    NguoiDaiDien    NVARCHAR(200) NULL,
    SoDienThoai     VARCHAR(30) NULL,
    Email           VARCHAR(200) NULL,

    MoTa            NVARCHAR(1000) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_DonVi_Ma UNIQUE (Ma),

    CONSTRAINT FK_DonVi_Khoi
        FOREIGN KEY (KhoiId)
        REFERENCES Khoi(Id),

    CONSTRAINT FK_DonVi_DonViCha
        FOREIGN KEY (DonViChaId)
        REFERENCES DonVi(Id),

    CONSTRAINT CK_DonVi_Loai
        CHECK
        (
            LoaiDonVi IS NULL
            OR LoaiDonVi IN
            (
                'So',
                'Tinh',
                'Huyen',
                'Xa',
                'Truong',
                'BenhVien',
                'DoanhNghiep',
                'DonViTrucThuoc',
                'Khac'
            )
        )
);
GO


/* ============================================================
   3. GIAI DAU
   PhamVi:
   - TatCa
   - TheoKhoi

   N?u TheoKhoi thì s? d?ng b?ng GiaiDauKhoi.
   ============================================================ */

IF OBJECT_ID('dbo.GiaiDau', 'U') IS NOT NULL
    DROP TABLE dbo.GiaiDau;
GO

CREATE TABLE GiaiDau
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(300) NOT NULL,

    MoTa            NVARCHAR(2000) NULL,

    NgayBatDau      DATE NOT NULL,
    NgayKetThuc     DATE NOT NULL,

    DiaDiem         NVARCHAR(500) NULL,

    PhamVi          VARCHAR(20) NOT NULL DEFAULT 'TatCa',

    TrangThai       VARCHAR(30) NOT NULL DEFAULT 'Nhap',

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_GiaiDau_Ma UNIQUE (Ma),

    CONSTRAINT CK_GiaiDau_Ngay
        CHECK (NgayKetThuc >= NgayBatDau),

    CONSTRAINT CK_GiaiDau_PhamVi
        CHECK (PhamVi IN ('TatCa', 'TheoKhoi')),

    CONSTRAINT CK_GiaiDau_TrangThai
        CHECK
        (
            TrangThai IN
            (
                'Nhap',
                'SapDienRa',
                'DangDienRa',
                'KetThuc',
                'Huy'
            )
        )
);
GO


/* ============================================================
   4. GIAI DAU - KHOI
   Dùng khi gi?i ch? dành cho m?t s? kh?i.
   ============================================================ */

IF OBJECT_ID('dbo.GiaiDauKhoi', 'U') IS NOT NULL
    DROP TABLE dbo.GiaiDauKhoi;
GO

CREATE TABLE GiaiDauKhoi
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    GiaiDauId       BIGINT NOT NULL,
    KhoiId          BIGINT NOT NULL,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_GiaiDauKhoi_GiaiDau
        FOREIGN KEY (GiaiDauId)
        REFERENCES GiaiDau(Id),

    CONSTRAINT FK_GiaiDauKhoi_Khoi
        FOREIGN KEY (KhoiId)
        REFERENCES Khoi(Id),

    CONSTRAINT UQ_GiaiDauKhoi
        UNIQUE (GiaiDauId, KhoiId)
);
GO


/* ============================================================
   5. DANH MUC MON THE THAO
   Ví d?:
   - Bóng
   - ?i?n kinh
   - B?i
   - Võ thu?t
   ============================================================ */

IF OBJECT_ID('dbo.DanhMucMonTheThao', 'U') IS NOT NULL
    DROP TABLE dbo.DanhMucMonTheThao;
GO

CREATE TABLE DanhMucMonTheThao
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(200) NOT NULL,

    MoTa            NVARCHAR(1000) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_DanhMucMonTheThao_Ma UNIQUE (Ma)
);
GO


/* ============================================================
   6. MON THE THAO
   Ví d?:
   - Bóng ?á
   - Bóng chuy?n
   - C?u lông
   - ?i?n kinh
   ============================================================ */

IF OBJECT_ID('dbo.MonTheThao', 'U') IS NOT NULL
    DROP TABLE dbo.MonTheThao;
GO

CREATE TABLE MonTheThao
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    DanhMucId       BIGINT NOT NULL,

    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(200) NOT NULL,

    MoTa            NVARCHAR(1000) NULL,

    LaMonDongDoi    BIT NOT NULL DEFAULT 0,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_MonTheThao_Ma UNIQUE (Ma),

    CONSTRAINT FK_MonTheThao_DanhMuc
        FOREIGN KEY (DanhMucId)
        REFERENCES DanhMucMonTheThao(Id)
);
GO


/* ============================================================
   7. GIAI DAU - MON THE THAO
   M?t gi?i có nhi?u môn.
   ============================================================ */

IF OBJECT_ID('dbo.GiaiDauMonTheThao', 'U') IS NOT NULL
    DROP TABLE dbo.GiaiDauMonTheThao;
GO

CREATE TABLE GiaiDauMonTheThao
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    GiaiDauId       BIGINT NOT NULL,
    MonTheThaoId    BIGINT NOT NULL,

    MoTa            NVARCHAR(1000) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT FK_GiaiDauMonTheThao_GiaiDau
        FOREIGN KEY (GiaiDauId)
        REFERENCES GiaiDau(Id),

    CONSTRAINT FK_GiaiDauMonTheThao_Mon
        FOREIGN KEY (MonTheThaoId)
        REFERENCES MonTheThao(Id),

    CONSTRAINT UQ_GiaiDauMonTheThao
        UNIQUE (GiaiDauId, MonTheThaoId)
);
GO


/* ============================================================
   8. NOI DUNG THI DAU
   Ví d?:

   ?i?n kinh
      - 100m Nam
      - 100m N?
      - Nh?y xa Nam

   C?u lông
      - ??n nam
      - ??n n?
      - ?ôi nam
      - ?ôi n?

   LoaiThiDau:
      CaNhan
      Doi
      DongDoi
   ============================================================ */

IF OBJECT_ID('dbo.NoiDungThiDau', 'U') IS NOT NULL
    DROP TABLE dbo.NoiDungThiDau;
GO

CREATE TABLE NoiDungThiDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    GiaiDauMonTheThaoId BIGINT NOT NULL,

    Ma                  VARCHAR(50) NOT NULL,
    Ten                 NVARCHAR(200) NOT NULL,

    GioiTinh            VARCHAR(20) NOT NULL DEFAULT 'HonHop',

    LoaiThiDau          VARCHAR(30) NOT NULL DEFAULT 'CaNhan',

    SoLuongToiThieu     INT NULL,
    SoLuongToiDa        INT NULL,

    MoTa                NVARCHAR(1000) NULL,

    TrangThai           BIT NOT NULL DEFAULT 1,

    NgayTao             DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat         DATETIME2 NULL,

    CONSTRAINT FK_NoiDungThiDau_GiaiDauMon
        FOREIGN KEY (GiaiDauMonTheThaoId)
        REFERENCES GiaiDauMonTheThao(Id),

    CONSTRAINT UQ_NoiDungThiDau
        UNIQUE (GiaiDauMonTheThaoId, Ma),

    CONSTRAINT CK_NoiDung_GioiTinh
        CHECK
        (
            GioiTinh IN
            (
                'Nam',
                'Nu',
                'HonHop'
            )
        ),

    CONSTRAINT CK_NoiDung_LoaiThiDau
        CHECK
        (
            LoaiThiDau IN
            (
                'CaNhan',
                'Doi',
                'DongDoi'
            )
        ),

    CONSTRAINT CK_NoiDung_SoLuong
        CHECK
        (
            SoLuongToiThieu IS NULL
            OR SoLuongToiDa IS NULL
            OR SoLuongToiDa >= SoLuongToiThieu
        )
);
GO


/* ============================================================
   9. VAN DONG VIEN
   M?t V?V có th? tham gia nhi?u môn / nhi?u n?i dung.
   ============================================================ */

IF OBJECT_ID('dbo.VanDongVien', 'U') IS NOT NULL
    DROP TABLE dbo.VanDongVien;
GO

CREATE TABLE VanDongVien
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(50) NOT NULL,
    HoTen           NVARCHAR(200) NOT NULL,

    DonViId         BIGINT NULL,

    NgaySinh        DATE NULL,

    GioiTinh        VARCHAR(20) NOT NULL,

    SoDienThoai     VARCHAR(30) NULL,
    Email           VARCHAR(200) NULL,

    SoCCCD          VARCHAR(50) NULL,

    DiaChi          NVARCHAR(500) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_VanDongVien_Ma
        UNIQUE (Ma),

    CONSTRAINT FK_VanDongVien_DonVi
        FOREIGN KEY (DonViId)
        REFERENCES DonVi(Id),

    CONSTRAINT CK_VanDongVien_GioiTinh
        CHECK (GioiTinh IN ('Nam', 'Nu'))
);
GO


/* ============================================================
   10. DOI
   Dùng cho:
   - Bóng ?á
   - Bóng chuy?n
   - C?u lông ?ôi
   - Các n?i dung ??ng ??i
   ============================================================ */

IF OBJECT_ID('dbo.Doi', 'U') IS NOT NULL
    DROP TABLE dbo.Doi;
GO

CREATE TABLE Doi
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(200) NOT NULL,

    DonViId         BIGINT NULL,

    NguoiQuanLy     NVARCHAR(200) NULL,
    SoDienThoai     VARCHAR(30) NULL,
    Email           VARCHAR(200) NULL,

    MoTa            NVARCHAR(1000) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_Doi_Ma
        UNIQUE (Ma),

    CONSTRAINT FK_Doi_DonVi
        FOREIGN KEY (DonViId)
        REFERENCES DonVi(Id)
);
GO


/* ============================================================
   11. THANH VIEN DOI
   ============================================================ */

IF OBJECT_ID('dbo.ThanhVienDoi', 'U') IS NOT NULL
    DROP TABLE dbo.ThanhVienDoi;
GO

CREATE TABLE ThanhVienDoi
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    DoiId           BIGINT NOT NULL,
    VanDongVienId   BIGINT NOT NULL,

    SoAo            VARCHAR(20) NULL,
    ViTri           NVARCHAR(100) NULL,

    LaDoiTruong     BIT NOT NULL DEFAULT 0,

    NgayThamGia     DATE NULL,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ThanhVienDoi_Doi
        FOREIGN KEY (DoiId)
        REFERENCES Doi(Id),

    CONSTRAINT FK_ThanhVienDoi_VDV
        FOREIGN KEY (VanDongVienId)
        REFERENCES VanDongVien(Id),

    CONSTRAINT UQ_ThanhVienDoi
        UNIQUE (DoiId, VanDongVienId)
);
GO


/* ============================================================
   12. DANG KY THI DAU
   M?t ??ng ký có th? là:

   Cá nhân:
       1 V?V

   ?ôi:
       2 V?V

   ??ng ??i:
       nhi?u V?V

   ??i:
       DoiId khác NULL
   ============================================================ */

IF OBJECT_ID('dbo.DangKyThiDau', 'U') IS NOT NULL
    DROP TABLE dbo.DangKyThiDau;
GO

CREATE TABLE DangKyThiDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    NoiDungThiDauId    BIGINT NOT NULL,

    DoiId               BIGINT NULL,

    SoDangKy            VARCHAR(50) NOT NULL,

    TenDangKy           NVARCHAR(300) NULL,

    TrangThai           VARCHAR(30) NOT NULL DEFAULT 'ChoDuyet',

    NgayDangKy          DATETIME2 NOT NULL DEFAULT GETDATE(),

    GhiChu              NVARCHAR(1000) NULL,

    CONSTRAINT FK_DangKyThiDau_NoiDung
        FOREIGN KEY (NoiDungThiDauId)
        REFERENCES NoiDungThiDau(Id),

    CONSTRAINT FK_DangKyThiDau_Doi
        FOREIGN KEY (DoiId)
        REFERENCES Doi(Id),

    CONSTRAINT UQ_DangKyThiDau
        UNIQUE (NoiDungThiDauId, SoDangKy),

    CONSTRAINT CK_DangKy_TrangThai
        CHECK
        (
            TrangThai IN
            (
                'ChoDuyet',
                'DaDuyet',
                'TuChoi',
                'Huy'
            )
        )
);
GO


/* ============================================================
   13. CHI TIET DANG KY
   ?ây là b?ng r?t quan tr?ng.

   Ví d?:

   ??n nam:
       DangKy #1
          ??? VDV A

   ?ôi nam:
       DangKy #2
          ??? VDV A
          ??? VDV B

   ??ng ??i:
       DangKy #3
          ??? VDV A
          ??? VDV B
          ??? VDV C
          ??? VDV D
   ============================================================ */

IF OBJECT_ID('dbo.ChiTietDangKyThiDau', 'U') IS NOT NULL
    DROP TABLE dbo.ChiTietDangKyThiDau;
GO

CREATE TABLE ChiTietDangKyThiDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    DangKyThiDauId      BIGINT NOT NULL,
    VanDongVienId       BIGINT NOT NULL,

    SoThuTu             INT NULL,
    VaiTro              NVARCHAR(100) NULL,

    NgayTao             DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ChiTietDangKy_DangKy
        FOREIGN KEY (DangKyThiDauId)
        REFERENCES DangKyThiDau(Id),

    CONSTRAINT FK_ChiTietDangKy_VDV
        FOREIGN KEY (VanDongVienId)
        REFERENCES VanDongVien(Id),

    CONSTRAINT UQ_ChiTietDangKy
        UNIQUE (DangKyThiDauId, VanDongVienId)
);
GO


/* ============================================================
   14. BANG DAU
   Không b?t bu?c.

   Bóng ?á:
      B?ng A
      B?ng B
      B?ng C

   ?i?n kinh:
      Có th? không dùng.
   ============================================================ */

IF OBJECT_ID('dbo.BangDau', 'U') IS NOT NULL
    DROP TABLE dbo.BangDau;
GO

CREATE TABLE BangDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    NoiDungThiDauId    BIGINT NOT NULL,

    Ma                  VARCHAR(50) NOT NULL,
    Ten                 NVARCHAR(200) NOT NULL,

    ThuTu               INT NOT NULL DEFAULT 1,

    NgayTao             DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_BangDau_NoiDung
        FOREIGN KEY (NoiDungThiDauId)
        REFERENCES NoiDungThiDau(Id),

    CONSTRAINT UQ_BangDau
        UNIQUE (NoiDungThiDauId, Ma)
);
GO


/* ============================================================
   15. THANH VIEN BANG
   ============================================================ */

IF OBJECT_ID('dbo.ThanhVienBang', 'U') IS NOT NULL
    DROP TABLE dbo.ThanhVienBang;
GO

CREATE TABLE ThanhVienBang
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    BangDauId           BIGINT NOT NULL,
    DangKyThiDauId      BIGINT NOT NULL,

    HatGiong            INT NULL,

    SoTran              INT NOT NULL DEFAULT 0,
    SoThang             INT NOT NULL DEFAULT 0,
    SoHoa               INT NOT NULL DEFAULT 0,
    SoThua              INT NOT NULL DEFAULT 0,

    DiemGhiDuoc         DECIMAL(18,3) NOT NULL DEFAULT 0,
    DiemBiGhi           DECIMAL(18,3) NOT NULL DEFAULT 0,

    Diem                DECIMAL(18,3) NOT NULL DEFAULT 0,

    XepHang             INT NULL,

    CONSTRAINT FK_ThanhVienBang_Bang
        FOREIGN KEY (BangDauId)
        REFERENCES BangDau(Id),

    CONSTRAINT FK_ThanhVienBang_DangKy
        FOREIGN KEY (DangKyThiDauId)
        REFERENCES DangKyThiDau(Id),

    CONSTRAINT UQ_ThanhVienBang
        UNIQUE (BangDauId, DangKyThiDauId)
);
GO


/* ============================================================
   16. VONG DAU

   Ví d?:
   - Vòng b?ng
   - Vòng lo?i
   - Vòng 16
   - T? k?t
   - Bán k?t
   - Tranh h?ng 3
   - Chung k?t
   ============================================================ */

IF OBJECT_ID('dbo.VongDau', 'U') IS NOT NULL
    DROP TABLE dbo.VongDau;
GO

CREATE TABLE VongDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    NoiDungThiDauId    BIGINT NOT NULL,

    Ten                 NVARCHAR(200) NOT NULL,

    LoaiVong            VARCHAR(30) NOT NULL,

    ThuTu               INT NOT NULL,

    MoTa                NVARCHAR(1000) NULL,

    NgayTao             DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_VongDau_NoiDung
        FOREIGN KEY (NoiDungThiDauId)
        REFERENCES NoiDungThiDau(Id),

    CONSTRAINT UQ_VongDau
        UNIQUE (NoiDungThiDauId, ThuTu),

    CONSTRAINT CK_VongDau_Loai
        CHECK
        (
            LoaiVong IN
            (
                'VongBang',
                'VongLoai',
                'Vong16',
                'TuKet',
                'BanKet',
                'TranhHangBa',
                'ChungKet'
            )
        )
);
GO


/* ============================================================
   17. CUM SAN
   Ví d?:
      Nhà thi ??u A
      ??? Sân 1
      ??? Sân 2
      ??? Sân 3

      Khu liên h?p B
      ??? Sân bóng ?á
      ??? Sân tennis
      ??? H? b?i
   ============================================================ */

IF OBJECT_ID('dbo.CumSan', 'U') IS NOT NULL
    DROP TABLE dbo.CumSan;
GO

CREATE TABLE CumSan
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(300) NOT NULL,

    DiaChi          NVARCHAR(500) NULL,

    SoLuongSan      INT NULL,

    MoTa            NVARCHAR(1000) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_CumSan_Ma
        UNIQUE (Ma)
);
GO


/* ============================================================
   18. SAN DAU
   ============================================================ */

IF OBJECT_ID('dbo.SanDau', 'U') IS NOT NULL
    DROP TABLE dbo.SanDau;
GO

CREATE TABLE SanDau
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    CumSanId        BIGINT NOT NULL,

    Ma              VARCHAR(50) NOT NULL,
    Ten             NVARCHAR(200) NOT NULL,

    LoaiSan         NVARCHAR(100) NULL,

    SoSan           INT NULL,

    SucChua         INT NULL,

    MoTa            NVARCHAR(1000) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat     DATETIME2 NULL,

    CONSTRAINT UQ_SanDau_Ma
        UNIQUE (Ma),

    CONSTRAINT FK_SanDau_CumSan
        FOREIGN KEY (CumSanId)
        REFERENCES CumSan(Id),

    CONSTRAINT UQ_SanDau_CumSan_SoSan
        UNIQUE (CumSanId, SoSan)
);
GO


/* ============================================================
   19. TRONG TAI
   ============================================================ */

IF OBJECT_ID('dbo.TrongTai', 'U') IS NOT NULL
    DROP TABLE dbo.TrongTai;
GO

CREATE TABLE TrongTai
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(50) NOT NULL,
    HoTen           NVARCHAR(200) NOT NULL,

    GioiTinh        VARCHAR(20) NULL,

    SoDienThoai     VARCHAR(30) NULL,
    Email           VARCHAR(200) NULL,

    CapBac          NVARCHAR(100) NULL,

    TrangThai       BIT NOT NULL DEFAULT 1,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT UQ_TrongTai_Ma
        UNIQUE (Ma)
);
GO


/* ============================================================
   20. TRAN DAU
   Không l?u Doi1 / Doi2.

   Vì:
   - Bóng ?á: 2 ??i
   - ?i?n kinh: 8 V?V
   - B?i: nhi?u V?V
   - Nh?y xa: nhi?u V?V
   - Các môn khác có th? có N ng??i.

   Thành ph?n tr?n ??u n?m ? ThanhPhanTranDau.
   ============================================================ */

IF OBJECT_ID('dbo.TranDau', 'U') IS NOT NULL
    DROP TABLE dbo.TranDau;
GO

CREATE TABLE TranDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    NoiDungThiDauId    BIGINT NOT NULL,
    VongDauId          BIGINT NOT NULL,

    BangDauId           BIGINT NULL,

    SanDauId            BIGINT NULL,

    SoTran              INT NOT NULL,

    TenTran             NVARCHAR(300) NULL,

    ThoiGianDuKien      DATETIME2 NULL,

    ThoiGianBatDau      DATETIME2 NULL,
    ThoiGianKetThuc     DATETIME2 NULL,

    TrangThai           VARCHAR(30) NOT NULL DEFAULT 'ChuaDau',

    GhiChu              NVARCHAR(1000) NULL,

    NgayTao             DATETIME2 NOT NULL DEFAULT GETDATE(),
    NgayCapNhat         DATETIME2 NULL,

    CONSTRAINT FK_TranDau_NoiDung
        FOREIGN KEY (NoiDungThiDauId)
        REFERENCES NoiDungThiDau(Id),

    CONSTRAINT FK_TranDau_Vong
        FOREIGN KEY (VongDauId)
        REFERENCES VongDau(Id),

    CONSTRAINT FK_TranDau_Bang
        FOREIGN KEY (BangDauId)
        REFERENCES BangDau(Id),

    CONSTRAINT FK_TranDau_San
        FOREIGN KEY (SanDauId)
        REFERENCES SanDau(Id),

    CONSTRAINT UQ_TranDau
        UNIQUE (NoiDungThiDauId, SoTran),

    CONSTRAINT CK_TranDau_TrangThai
        CHECK
        (
            TrangThai IN
            (
                'ChuaDau',
                'DangDau',
                'DaDau',
                'Hoan',
                'Huy'
            )
        )
);
GO


/* ============================================================
   21. THANH PHAN TRAN DAU
   ?ây là b?ng giúp TranDau h? tr? N ng??i / N ??i.

   Ví d? 100m:
      Tran 1
      ??? VDV A - Lane 1
      ??? VDV B - Lane 2
      ??? VDV C - Lane 3
      ??? VDV D - Lane 4

   Bóng ?á:
      Tran 1
      ??? ??i A
      ??? ??i B
   ============================================================ */

IF OBJECT_ID('dbo.ThanhPhanTranDau', 'U') IS NOT NULL
    DROP TABLE dbo.ThanhPhanTranDau;
GO

CREATE TABLE ThanhPhanTranDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    TranDauId           BIGINT NOT NULL,
    DangKyThiDauId      BIGINT NOT NULL,

    SoLane              INT NULL,

    ThuTuThiDau         INT NULL,

    ViTri               INT NULL,

    TrangThai           VARCHAR(30) NOT NULL DEFAULT 'ThamGia',

    GhiChu              NVARCHAR(500) NULL,

    NgayTao             DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ThanhPhanTranDau_Tran
        FOREIGN KEY (TranDauId)
        REFERENCES TranDau(Id),

    CONSTRAINT FK_ThanhPhanTranDau_DangKy
        FOREIGN KEY (DangKyThiDauId)
        REFERENCES DangKyThiDau(Id),

    CONSTRAINT UQ_ThanhPhanTranDau
        UNIQUE (TranDauId, DangKyThiDauId),

    CONSTRAINT CK_ThanhPhanTranDau_TrangThai
        CHECK
        (
            TrangThai IN
            (
                'ThamGia',
                'KhongThamGia',
                'BoCuoc',
                'BiLoai',
                'HoanThanh'
            )
        )
);
GO


/* ============================================================
   22. HIEP DAU
   Dùng cho:
   - C?u lông
   - Bóng chuy?n
   - Tennis
   - Các môn có set/hi?p
   ============================================================ */

IF OBJECT_ID('dbo.HiepDau', 'U') IS NOT NULL
    DROP TABLE dbo.HiepDau;
GO

CREATE TABLE HiepDau
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    TranDauId           BIGINT NOT NULL,

    SoHiep              INT NOT NULL,

    ThoiGianBatDau      DATETIME2 NULL,
    ThoiGianKetThuc     DATETIME2 NULL,

    TrangThai           VARCHAR(30) NOT NULL DEFAULT 'ChuaDau',

    GhiChu              NVARCHAR(500) NULL,

    CONSTRAINT FK_HiepDau_Tran
        FOREIGN KEY (TranDauId)
        REFERENCES TranDau(Id),

    CONSTRAINT UQ_HiepDau
        UNIQUE (TranDauId, SoHiep)
);
GO


/* ============================================================
   23. KET QUA HIEP DAU
   ============================================================ */

IF OBJECT_ID('dbo.KetQuaHiepDau', 'U') IS NOT NULL
    DROP TABLE dbo.KetQuaHiepDau;
GO

CREATE TABLE KetQuaHiepDau
(
    Id                      BIGINT IDENTITY(1,1) PRIMARY KEY,

    HiepDauId               BIGINT NOT NULL,

    ThanhPhanTranDauId      BIGINT NOT NULL,

    Diem                    DECIMAL(18,3) NULL,

    GhiChu                  NVARCHAR(500) NULL,

    CONSTRAINT FK_KetQuaHiepDau_Hiep
        FOREIGN KEY (HiepDauId)
        REFERENCES HiepDau(Id),

    CONSTRAINT FK_KetQuaHiepDau_ThanhPhan
        FOREIGN KEY (ThanhPhanTranDauId)
        REFERENCES ThanhPhanTranDau(Id),

    CONSTRAINT UQ_KetQuaHiepDau
        UNIQUE (HiepDauId, ThanhPhanTranDauId)
);
GO


/* ============================================================
   24. KET QUA TRAN DAU
   H? tr? nhi?u ki?u k?t qu?:

   Diem
   ThoiGian
   CuLy
   BanThang
   ThanhTich
   Khac

   Ví d?:

   100m:
      10.52 giây

   Nh?y xa:
      7.32 mét

   Bóng ?á:
      3 bàn th?ng
   ============================================================ */

IF OBJECT_ID('dbo.KetQuaTranDau', 'U') IS NOT NULL
    DROP TABLE dbo.KetQuaTranDau;
GO

CREATE TABLE KetQuaTranDau
(
    Id                      BIGINT IDENTITY(1,1) PRIMARY KEY,

    ThanhPhanTranDauId      BIGINT NOT NULL,

    LoaiKetQua              VARCHAR(30) NOT NULL,

    GiaTri                  DECIMAL(18,4) NULL,

    DonVi                   VARCHAR(30) NULL,

    Diem                    DECIMAL(18,3) NULL,

    XepHang                 INT NULL,

    KyLuc                   BIT NOT NULL DEFAULT 0,

    KetQuaText              NVARCHAR(500) NULL,

    GhiChu                  NVARCHAR(1000) NULL,

    NgayTao                 DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_KetQuaTranDau_ThanhPhan
        FOREIGN KEY (ThanhPhanTranDauId)
        REFERENCES ThanhPhanTranDau(Id),

    CONSTRAINT CK_KetQuaTranDau_Loai
        CHECK
        (
            LoaiKetQua IN
            (
                'Diem',
                'ThoiGian',
                'CuLy',
                'BanThang',
                'ThanhTich',
                'Khac'
            )
        )
);
GO


/* ============================================================
   25. PHAN CONG TRONG TAI
   M?t tr?n có th? có nhi?u tr?ng tài.
   ============================================================ */

IF OBJECT_ID('dbo.PhanCongTrongTai', 'U') IS NOT NULL
    DROP TABLE dbo.PhanCongTrongTai;
GO

CREATE TABLE PhanCongTrongTai
(
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,

    TranDauId       BIGINT NOT NULL,
    TrongTaiId      BIGINT NOT NULL,

    VaiTro          NVARCHAR(100) NULL,

    GhiChu          NVARCHAR(500) NULL,

    NgayTao         DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_PhanCongTrongTai_Tran
        FOREIGN KEY (TranDauId)
        REFERENCES TranDau(Id),

    CONSTRAINT FK_PhanCongTrongTai_TrongTai
        FOREIGN KEY (TrongTaiId)
        REFERENCES TrongTai(Id),

    CONSTRAINT UQ_PhanCongTrongTai
        UNIQUE (TranDauId, TrongTaiId)
);
GO


/* ============================================================
   26. LOAI HUY CHUONG
   ============================================================ */

IF OBJECT_ID('dbo.LoaiHuyChuong', 'U') IS NOT NULL
    DROP TABLE dbo.LoaiHuyChuong;
GO

CREATE TABLE LoaiHuyChuong
(
    Id              INT IDENTITY(1,1) PRIMARY KEY,

    Ma              VARCHAR(30) NOT NULL,
    Ten             NVARCHAR(100) NOT NULL,

    ThuTu           INT NOT NULL,

    CONSTRAINT UQ_LoaiHuyChuong_Ma
        UNIQUE (Ma)
);
GO


/* ============================================================
   27. HUY CHUONG
   Huy ch??ng g?n v?i ??NG KÝ THI ??U.

   Vì v?y h? tr?:

   Cá nhân:
       V?V A -> Vàng

   ?ôi:
       V?V A + V?V B -> Vàng

   ??ng ??i:
       ??i A -> Vàng
   ============================================================ */

IF OBJECT_ID('dbo.HuyChuong', 'U') IS NOT NULL
    DROP TABLE dbo.HuyChuong;
GO

CREATE TABLE HuyChuong
(
    Id                  BIGINT IDENTITY(1,1) PRIMARY KEY,

    GiaiDauId           BIGINT NOT NULL,

    NoiDungThiDauId    BIGINT NOT NULL,

    DangKyThiDauId      BIGINT NOT NULL,

    LoaiHuyChuongId     INT NOT NULL,

    XepHang             INT NOT NULL,

    NgayTrao            DATETIME2 NULL,

    GhiChu              NVARCHAR(1000) NULL,

    NgayTao             DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_HuyChuong_GiaiDau
        FOREIGN KEY (GiaiDauId)
        REFERENCES GiaiDau(Id),

    CONSTRAINT FK_HuyChuong_NoiDung
        FOREIGN KEY (NoiDungThiDauId)
        REFERENCES NoiDungThiDau(Id),

    CONSTRAINT FK_HuyChuong_DangKy
        FOREIGN KEY (DangKyThiDauId)
        REFERENCES DangKyThiDau(Id),

    CONSTRAINT FK_HuyChuong_Loai
        FOREIGN KEY (LoaiHuyChuongId)
        REFERENCES LoaiHuyChuong(Id),

    CONSTRAINT UQ_HuyChuong_DangKy
        UNIQUE (DangKyThiDauId),

    CONSTRAINT CK_HuyChuong_XepHang
        CHECK (XepHang > 0)
);
GO


/* ============================================================
   28. INDEX
   ============================================================ */

CREATE INDEX IX_DonVi_KhoiId
ON DonVi(KhoiId);
GO

CREATE INDEX IX_DonVi_DonViChaId
ON DonVi(DonViChaId);
GO

CREATE INDEX IX_GiaiDauKhoi_GiaiDauId
ON GiaiDauKhoi(GiaiDauId);
GO

CREATE INDEX IX_GiaiDauKhoi_KhoiId
ON GiaiDauKhoi(KhoiId);
GO

CREATE INDEX IX_GiaiDauMonTheThao_GiaiDauId
ON GiaiDauMonTheThao(GiaiDauId);
GO

CREATE INDEX IX_GiaiDauMonTheThao_MonTheThaoId
ON GiaiDauMonTheThao(MonTheThaoId);
GO

CREATE INDEX IX_NoiDungThiDau_GiaiDauMonTheThaoId
ON NoiDungThiDau(GiaiDauMonTheThaoId);
GO

CREATE INDEX IX_VanDongVien_DonViId
ON VanDongVien(DonViId);
GO

CREATE INDEX IX_Doi_DonViId
ON Doi(DonViId);
GO

CREATE INDEX IX_ThanhVienDoi_DoiId
ON ThanhVienDoi(DoiId);
GO

CREATE INDEX IX_ThanhVienDoi_VanDongVienId
ON ThanhVienDoi(VanDongVienId);
GO

CREATE INDEX IX_DangKyThiDau_NoiDungThiDauId
ON DangKyThiDau(NoiDungThiDauId);
GO

CREATE INDEX IX_DangKyThiDau_DoiId
ON DangKyThiDau(DoiId);
GO

CREATE INDEX IX_ChiTietDangKy_DangKyThiDauId
ON ChiTietDangKyThiDau(DangKyThiDauId);
GO

CREATE INDEX IX_ChiTietDangKy_VanDongVienId
ON ChiTietDangKyThiDau(VanDongVienId);
GO

CREATE INDEX IX_BangDau_NoiDungThiDauId
ON BangDau(NoiDungThiDauId);
GO

CREATE INDEX IX_ThanhVienBang_DangKyThiDauId
ON ThanhVienBang(DangKyThiDauId);
GO

CREATE INDEX IX_VongDau_NoiDungThiDauId
ON VongDau(NoiDungThiDauId);
GO

CREATE INDEX IX_SanDau_CumSanId
ON SanDau(CumSanId);
GO

CREATE INDEX IX_TranDau_NoiDungThiDauId
ON TranDau(NoiDungThiDauId);
GO

CREATE INDEX IX_TranDau_VongDauId
ON TranDau(VongDauId);
GO

CREATE INDEX IX_TranDau_SanDauId
ON TranDau(SanDauId);
GO

CREATE INDEX IX_ThanhPhanTranDau_TranDauId
ON ThanhPhanTranDau(TranDauId);
GO

CREATE INDEX IX_ThanhPhanTranDau_DangKyThiDauId
ON ThanhPhanTranDau(DangKyThiDauId);
GO

CREATE INDEX IX_HiepDau_TranDauId
ON HiepDau(TranDauId);
GO

CREATE INDEX IX_KetQuaHiepDau_ThanhPhanTranDauId
ON KetQuaHiepDau(ThanhPhanTranDauId);
GO

CREATE INDEX IX_KetQuaTranDau_ThanhPhanTranDauId
ON KetQuaTranDau(ThanhPhanTranDauId);
GO

CREATE INDEX IX_PhanCongTrongTai_TrongTaiId
ON PhanCongTrongTai(TrongTaiId);
GO

CREATE INDEX IX_HuyChuong_GiaiDauId
ON HuyChuong(GiaiDauId);
GO

CREATE INDEX IX_HuyChuong_NoiDungThiDauId
ON HuyChuong(NoiDungThiDauId);
GO

CREATE INDEX IX_HuyChuong_LoaiHuyChuongId
ON HuyChuong(LoaiHuyChuongId);
GO


/* ============================================================
   29. D? LI?U LO?I HUY CH??NG
   ============================================================ */

IF NOT EXISTS
(
    SELECT 1
    FROM LoaiHuyChuong
    WHERE Ma = 'VANG'
)
BEGIN
    INSERT INTO LoaiHuyChuong
    (
        Ma,
        Ten,
        ThuTu
    )
    VALUES
    (
        'VANG',
        N'Huy ch??ng Vàng',
        1
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM LoaiHuyChuong
    WHERE Ma = 'BAC'
)
BEGIN
    INSERT INTO LoaiHuyChuong
    (
        Ma,
        Ten,
        ThuTu
    )
    VALUES
    (
        'BAC',
        N'Huy ch??ng B?c',
        2
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM LoaiHuyChuong
    WHERE Ma = 'DONG'
)
BEGIN
    INSERT INTO LoaiHuyChuong
    (
        Ma,
        Ten,
        ThuTu
    )
    VALUES
    (
        'DONG',
        N'Huy ch??ng ??ng',
        3
    );
END
GO


/* ============================================================
   30. D? LI?U KH?I M?U
   ============================================================ */

IF NOT EXISTS
(
    SELECT 1
    FROM Khoi
    WHERE Ma = 'GIAO_DUC'
)
BEGIN
    INSERT INTO Khoi
    (
        Ma,
        Ten
    )
    VALUES
    (
        'GIAO_DUC',
        N'Kh?i Giáo d?c'
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM Khoi
    WHERE Ma = 'Y_TE'
)
BEGIN
    INSERT INTO Khoi
    (
        Ma,
        Ten
    )
    VALUES
    (
        'Y_TE',
        N'Kh?i Y t?'
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM Khoi
    WHERE Ma = 'VAN_HOA'
)
BEGIN
    INSERT INTO Khoi
    (
        Ma,
        Ten
    )
    VALUES
    (
        'VAN_HOA',
        N'Kh?i V?n hóa'
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM Khoi
    WHERE Ma = 'CONG_AN'
)
BEGIN
    INSERT INTO Khoi
    (
        Ma,
        Ten
    )
    VALUES
    (
        'CONG_AN',
        N'Kh?i Công an'
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM Khoi
    WHERE Ma = 'QUAN_DOI'
)
BEGIN
    INSERT INTO Khoi
    (
        Ma,
        Ten
    )
    VALUES
    (
        'QUAN_DOI',
        N'Kh?i Quân ??i'
    );
END
GO


/* ============================================================
   31. D? LI?U DANH M?C MÔN TH? THAO
   ============================================================ */

IF NOT EXISTS
(
    SELECT 1
    FROM DanhMucMonTheThao
    WHERE Ma = 'MON_BONG'
)
BEGIN
    INSERT INTO DanhMucMonTheThao
    (
        Ma,
        Ten
    )
    VALUES
    (
        'MON_BONG',
        N'Môn bóng'
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM DanhMucMonTheThao
    WHERE Ma = 'DIEN_KINH'
)
BEGIN
    INSERT INTO DanhMucMonTheThao
    (
        Ma,
        Ten
    )
    VALUES
    (
        'DIEN_KINH',
        N'?i?n kinh'
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM DanhMucMonTheThao
    WHERE Ma = 'DUOI_NUOC'
)
BEGIN
    INSERT INTO DanhMucMonTheThao
    (
        Ma,
        Ten
    )
    VALUES
    (
        'DUOI_NUOC',
        N'Môn d??i n??c'
    );
END
GO


IF NOT EXISTS
(
    SELECT 1
    FROM DanhMucMonTheThao
    WHERE Ma = 'VO_THUAT'
)
BEGIN
    INSERT INTO DanhMucMonTheThao
    (
        Ma,
        Ten
    )
    VALUES
    (
        'VO_THUAT',
        N'Võ thu?t'
    );
END
GO


/* ============================================================
   32. KI?M TRA C?U TRÚC
   ============================================================ */

SELECT
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO
