using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CumSan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoLuongSan = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CumSan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucMonTheThao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucMonTheThao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GiaiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiaDiem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhamVi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaiDau", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Khoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Khoi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoaiHuyChuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiHuyChuong", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menus_Menus_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThuKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DonViCongTac = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThuKy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrongTai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CapBac = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrongTai", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SanDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CumSanId = table.Column<int>(type: "int", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LoaiSan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoSan = table.Column<int>(type: "int", nullable: true),
                    SucChua = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SanDau_CumSan_CumSanId",
                        column: x => x.CumSanId,
                        principalTable: "CumSan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonTheThao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DanhMucId = table.Column<int>(type: "int", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LaMonDongDoi = table.Column<bool>(type: "bit", nullable: false),
                    HinhThucThiDau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonTheThao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonTheThao_DanhMucMonTheThao_DanhMucId",
                        column: x => x.DanhMucId,
                        principalTable: "DanhMucMonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DieuLeGiaiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauId = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TepDinhKem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DieuLeGiaiDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DieuLeGiaiDau_GiaiDau_GiaiDauId",
                        column: x => x.GiaiDauId,
                        principalTable: "GiaiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonVi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    KhoiId = table.Column<int>(type: "int", nullable: true),
                    DonViChaId = table.Column<int>(type: "int", nullable: true),
                    LoaiDonVi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NguoiDaiDien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonVi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonVi_DonVi_DonViChaId",
                        column: x => x.DonViChaId,
                        principalTable: "DonVi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonVi_Khoi_KhoiId",
                        column: x => x.KhoiId,
                        principalTable: "Khoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiaiDauKhoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauId = table.Column<int>(type: "int", nullable: false),
                    KhoiId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaiDauKhoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiaiDauKhoi_GiaiDau_GiaiDauId",
                        column: x => x.GiaiDauId,
                        principalTable: "GiaiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaiDauKhoi_Khoi_KhoiId",
                        column: x => x.KhoiId,
                        principalTable: "Khoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DieuLeMonTheThao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonTheThaoId = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TepDinhKem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DieuLeMonTheThao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DieuLeMonTheThao_MonTheThao_MonTheThaoId",
                        column: x => x.MonTheThaoId,
                        principalTable: "MonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GiaiDauMonTheThao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauId = table.Column<int>(type: "int", nullable: false),
                    MonTheThaoId = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaiDauMonTheThao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiaiDauMonTheThao_GiaiDau_GiaiDauId",
                        column: x => x.GiaiDauId,
                        principalTable: "GiaiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaiDauMonTheThao_MonTheThao_MonTheThaoId",
                        column: x => x.MonTheThaoId,
                        principalTable: "MonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Doi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DonViId = table.Column<int>(type: "int", nullable: true),
                    NguoiQuanLy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doi_DonVi_DonViId",
                        column: x => x.DonViId,
                        principalTable: "DonVi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DonViId = table.Column<int>(type: "int", nullable: true),
                    TrongTaiId = table.Column<int>(type: "int", nullable: true),
                    ThuKyId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_DonVi_DonViId",
                        column: x => x.DonViId,
                        principalTable: "DonVi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_ThuKy_ThuKyId",
                        column: x => x.ThuKyId,
                        principalTable: "ThuKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_TrongTai_TrongTaiId",
                        column: x => x.TrongTaiId,
                        principalTable: "TrongTai",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VanDongVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DonViId = table.Column<int>(type: "int", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoCCCD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanDongVien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VanDongVien_DonVi_DonViId",
                        column: x => x.DonViId,
                        principalTable: "DonVi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NoiDungThiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauMonTheThaoId = table.Column<int>(type: "int", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LoaiThiDau = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    HinhThucThiDau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoLuongToiThieu = table.Column<int>(type: "int", nullable: true),
                    SoLuongToiDa = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoiDungThiDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoiDungThiDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                        column: x => x.GiaiDauMonTheThaoId,
                        principalTable: "GiaiDauMonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revoked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuChuyenDoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VanDongVienId = table.Column<int>(type: "int", nullable: false),
                    DoiCuId = table.Column<int>(type: "int", nullable: true),
                    DoiMoiId = table.Column<int>(type: "int", nullable: false),
                    NgayChuyen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NguoiXacNhan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuChuyenDoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuChuyenDoi_Doi_DoiCuId",
                        column: x => x.DoiCuId,
                        principalTable: "Doi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuChuyenDoi_Doi_DoiMoiId",
                        column: x => x.DoiMoiId,
                        principalTable: "Doi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuChuyenDoi_VanDongVien_VanDongVienId",
                        column: x => x.VanDongVienId,
                        principalTable: "VanDongVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienDoi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoiId = table.Column<int>(type: "int", nullable: false),
                    VanDongVienId = table.Column<int>(type: "int", nullable: false),
                    SoAo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ViTri = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LaDoiTruong = table.Column<bool>(type: "bit", nullable: false),
                    NgayThamGia = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienDoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhVienDoi_Doi_DoiId",
                        column: x => x.DoiId,
                        principalTable: "Doi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhVienDoi_VanDongVien_VanDongVienId",
                        column: x => x.VanDongVienId,
                        principalTable: "VanDongVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BangDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDungThiDauId = table.Column<int>(type: "int", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangDau_NoiDungThiDau_NoiDungThiDauId",
                        column: x => x.NoiDungThiDauId,
                        principalTable: "NoiDungThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKyThiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDungThiDauId = table.Column<int>(type: "int", nullable: false),
                    DoiId = table.Column<int>(type: "int", nullable: true),
                    SoDangKy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenDangKy = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyThiDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangKyThiDau_Doi_DoiId",
                        column: x => x.DoiId,
                        principalTable: "Doi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyThiDau_NoiDungThiDau_NoiDungThiDauId",
                        column: x => x.NoiDungThiDauId,
                        principalTable: "NoiDungThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VongDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDungThiDauId = table.Column<int>(type: "int", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LoaiVong = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VongDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VongDau_NoiDungThiDau_NoiDungThiDauId",
                        column: x => x.NoiDungThiDauId,
                        principalTable: "NoiDungThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDangKyThiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DangKyThiDauId = table.Column<int>(type: "int", nullable: false),
                    VanDongVienId = table.Column<int>(type: "int", nullable: false),
                    SoThuTu = table.Column<int>(type: "int", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDangKyThiDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDangKyThiDau_DangKyThiDau_DangKyThiDauId",
                        column: x => x.DangKyThiDauId,
                        principalTable: "DangKyThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietDangKyThiDau_VanDongVien_VanDongVienId",
                        column: x => x.VanDongVienId,
                        principalTable: "VanDongVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HuyChuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauId = table.Column<int>(type: "int", nullable: false),
                    NoiDungThiDauId = table.Column<int>(type: "int", nullable: false),
                    DangKyThiDauId = table.Column<int>(type: "int", nullable: false),
                    LoaiHuyChuongId = table.Column<int>(type: "int", nullable: false),
                    XepHang = table.Column<int>(type: "int", nullable: false),
                    NgayTrao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HuyChuong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HuyChuong_DangKyThiDau_DangKyThiDauId",
                        column: x => x.DangKyThiDauId,
                        principalTable: "DangKyThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HuyChuong_GiaiDau_GiaiDauId",
                        column: x => x.GiaiDauId,
                        principalTable: "GiaiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HuyChuong_LoaiHuyChuong_LoaiHuyChuongId",
                        column: x => x.LoaiHuyChuongId,
                        principalTable: "LoaiHuyChuong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HuyChuong_NoiDungThiDau_NoiDungThiDauId",
                        column: x => x.NoiDungThiDauId,
                        principalTable: "NoiDungThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhVienBang",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BangDauId = table.Column<int>(type: "int", nullable: false),
                    DangKyThiDauId = table.Column<int>(type: "int", nullable: false),
                    HatGiong = table.Column<int>(type: "int", nullable: true),
                    SoTran = table.Column<int>(type: "int", nullable: false),
                    SoThang = table.Column<int>(type: "int", nullable: false),
                    SoHoa = table.Column<int>(type: "int", nullable: false),
                    SoThua = table.Column<int>(type: "int", nullable: false),
                    DiemGhiDuoc = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DiemBiGhi = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Diem = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    XepHang = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhVienBang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhVienBang_BangDau_BangDauId",
                        column: x => x.BangDauId,
                        principalTable: "BangDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhVienBang_DangKyThiDau_DangKyThiDauId",
                        column: x => x.DangKyThiDauId,
                        principalTable: "DangKyThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TranDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoiDungThiDauId = table.Column<int>(type: "int", nullable: false),
                    VongDauId = table.Column<int>(type: "int", nullable: false),
                    BangDauId = table.Column<int>(type: "int", nullable: true),
                    SanDauId = table.Column<int>(type: "int", nullable: true),
                    SoTran = table.Column<int>(type: "int", nullable: false),
                    TenTran = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ThoiGianDuKien = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TranDau_BangDau_BangDauId",
                        column: x => x.BangDauId,
                        principalTable: "BangDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TranDau_NoiDungThiDau_NoiDungThiDauId",
                        column: x => x.NoiDungThiDauId,
                        principalTable: "NoiDungThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TranDau_SanDau_SanDauId",
                        column: x => x.SanDauId,
                        principalTable: "SanDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TranDau_VongDau_VongDauId",
                        column: x => x.VongDauId,
                        principalTable: "VongDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HiepDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranDauId = table.Column<int>(type: "int", nullable: false),
                    SoHiep = table.Column<int>(type: "int", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiepDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiepDau_TranDau_TranDauId",
                        column: x => x.TranDauId,
                        principalTable: "TranDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanCongTrongTai",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranDauId = table.Column<int>(type: "int", nullable: false),
                    TrongTaiId = table.Column<int>(type: "int", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongTrongTai", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanCongTrongTai_TranDau_TranDauId",
                        column: x => x.TranDauId,
                        principalTable: "TranDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongTrongTai_TrongTai_TrongTaiId",
                        column: x => x.TrongTaiId,
                        principalTable: "TrongTai",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhPhanTranDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TranDauId = table.Column<int>(type: "int", nullable: false),
                    DangKyThiDauId = table.Column<int>(type: "int", nullable: false),
                    SoLane = table.Column<int>(type: "int", nullable: true),
                    ThuTuThiDau = table.Column<int>(type: "int", nullable: true),
                    ViTri = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhPhanTranDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhPhanTranDau_DangKyThiDau_DangKyThiDauId",
                        column: x => x.DangKyThiDauId,
                        principalTable: "DangKyThiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhPhanTranDau_TranDau_TranDauId",
                        column: x => x.TranDauId,
                        principalTable: "TranDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaHiepDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HiepDauId = table.Column<int>(type: "int", nullable: false),
                    ThanhPhanTranDauId = table.Column<int>(type: "int", nullable: false),
                    Diem = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaHiepDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KetQuaHiepDau_HiepDau_HiepDauId",
                        column: x => x.HiepDauId,
                        principalTable: "HiepDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KetQuaHiepDau_ThanhPhanTranDau_ThanhPhanTranDauId",
                        column: x => x.ThanhPhanTranDauId,
                        principalTable: "ThanhPhanTranDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KetQuaTranDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThanhPhanTranDauId = table.Column<int>(type: "int", nullable: false),
                    LoaiKetQua = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DonVi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Diem = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    XepHang = table.Column<int>(type: "int", nullable: true),
                    KyLuc = table.Column<bool>(type: "bit", nullable: false),
                    KetQuaText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KetQuaTranDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KetQuaTranDau_ThanhPhanTranDau_ThanhPhanTranDauId",
                        column: x => x.ThanhPhanTranDauId,
                        principalTable: "ThanhPhanTranDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangDau_NoiDungThiDauId",
                table: "BangDau",
                column: "NoiDungThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDangKyThiDau_DangKyThiDauId",
                table: "ChiTietDangKyThiDau",
                column: "DangKyThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDangKyThiDau_VanDongVienId",
                table: "ChiTietDangKyThiDau",
                column: "VanDongVienId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyThiDau_DoiId",
                table: "DangKyThiDau",
                column: "DoiId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyThiDau_NoiDungThiDauId",
                table: "DangKyThiDau",
                column: "NoiDungThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuLeGiaiDau_GiaiDauId",
                table: "DieuLeGiaiDau",
                column: "GiaiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuLeMonTheThao_MonTheThaoId",
                table: "DieuLeMonTheThao",
                column: "MonTheThaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Doi_DonViId",
                table: "Doi",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_DonViChaId",
                table: "DonVi",
                column: "DonViChaId");

            migrationBuilder.CreateIndex(
                name: "IX_DonVi_KhoiId",
                table: "DonVi",
                column: "KhoiId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDau_Slug",
                table: "GiaiDau",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDauKhoi_GiaiDauId",
                table: "GiaiDauKhoi",
                column: "GiaiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDauKhoi_KhoiId",
                table: "GiaiDauKhoi",
                column: "KhoiId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDauMonTheThao_GiaiDauId",
                table: "GiaiDauMonTheThao",
                column: "GiaiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDauMonTheThao_MonTheThaoId",
                table: "GiaiDauMonTheThao",
                column: "MonTheThaoId");

            migrationBuilder.CreateIndex(
                name: "IX_HiepDau_TranDauId",
                table: "HiepDau",
                column: "TranDauId");

            migrationBuilder.CreateIndex(
                name: "IX_HuyChuong_DangKyThiDauId",
                table: "HuyChuong",
                column: "DangKyThiDauId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HuyChuong_GiaiDauId",
                table: "HuyChuong",
                column: "GiaiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_HuyChuong_LoaiHuyChuongId",
                table: "HuyChuong",
                column: "LoaiHuyChuongId");

            migrationBuilder.CreateIndex(
                name: "IX_HuyChuong_NoiDungThiDauId",
                table: "HuyChuong",
                column: "NoiDungThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaHiepDau_HiepDauId",
                table: "KetQuaHiepDau",
                column: "HiepDauId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaHiepDau_ThanhPhanTranDauId",
                table: "KetQuaHiepDau",
                column: "ThanhPhanTranDauId");

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTranDau_ThanhPhanTranDauId",
                table: "KetQuaTranDau",
                column: "ThanhPhanTranDauId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuChuyenDoi_DoiCuId",
                table: "LichSuChuyenDoi",
                column: "DoiCuId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuChuyenDoi_DoiMoiId",
                table: "LichSuChuyenDoi",
                column: "DoiMoiId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuChuyenDoi_VanDongVienId",
                table: "LichSuChuyenDoi",
                column: "VanDongVienId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ParentId",
                table: "Menus",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_MonTheThao_DanhMucId",
                table: "MonTheThao",
                column: "DanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungThiDau_GiaiDauMonTheThaoId",
                table: "NoiDungThiDau",
                column: "GiaiDauMonTheThaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongTrongTai_TranDauId",
                table: "PhanCongTrongTai",
                column: "TranDauId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongTrongTai_TrongTaiId",
                table: "PhanCongTrongTai",
                column: "TrongTaiId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SanDau_CumSanId",
                table: "SanDau",
                column: "CumSanId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhPhanTranDau_DangKyThiDauId",
                table: "ThanhPhanTranDau",
                column: "DangKyThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhPhanTranDau_TranDauId",
                table: "ThanhPhanTranDau",
                column: "TranDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienBang_BangDauId",
                table: "ThanhVienBang",
                column: "BangDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienBang_DangKyThiDauId",
                table: "ThanhVienBang",
                column: "DangKyThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienDoi_DoiId",
                table: "ThanhVienDoi",
                column: "DoiId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhVienDoi_VanDongVienId",
                table: "ThanhVienDoi",
                column: "VanDongVienId");

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_BangDauId",
                table: "TranDau",
                column: "BangDauId");

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_NoiDungThiDauId",
                table: "TranDau",
                column: "NoiDungThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_SanDauId",
                table: "TranDau",
                column: "SanDauId");

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_VongDauId",
                table: "TranDau",
                column: "VongDauId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DonViId",
                table: "Users",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ThuKyId",
                table: "Users",
                column: "ThuKyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TrongTaiId",
                table: "Users",
                column: "TrongTaiId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VanDongVien_DonViId",
                table: "VanDongVien",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_VongDau_NoiDungThiDauId",
                table: "VongDau",
                column: "NoiDungThiDauId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDangKyThiDau");

            migrationBuilder.DropTable(
                name: "DieuLeGiaiDau");

            migrationBuilder.DropTable(
                name: "DieuLeMonTheThao");

            migrationBuilder.DropTable(
                name: "GiaiDauKhoi");

            migrationBuilder.DropTable(
                name: "HuyChuong");

            migrationBuilder.DropTable(
                name: "KetQuaHiepDau");

            migrationBuilder.DropTable(
                name: "KetQuaTranDau");

            migrationBuilder.DropTable(
                name: "LichSuChuyenDoi");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "PhanCongTrongTai");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "ThanhVienBang");

            migrationBuilder.DropTable(
                name: "ThanhVienDoi");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "LoaiHuyChuong");

            migrationBuilder.DropTable(
                name: "HiepDau");

            migrationBuilder.DropTable(
                name: "ThanhPhanTranDau");

            migrationBuilder.DropTable(
                name: "VanDongVien");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DangKyThiDau");

            migrationBuilder.DropTable(
                name: "TranDau");

            migrationBuilder.DropTable(
                name: "ThuKy");

            migrationBuilder.DropTable(
                name: "TrongTai");

            migrationBuilder.DropTable(
                name: "Doi");

            migrationBuilder.DropTable(
                name: "BangDau");

            migrationBuilder.DropTable(
                name: "SanDau");

            migrationBuilder.DropTable(
                name: "VongDau");

            migrationBuilder.DropTable(
                name: "DonVi");

            migrationBuilder.DropTable(
                name: "CumSan");

            migrationBuilder.DropTable(
                name: "NoiDungThiDau");

            migrationBuilder.DropTable(
                name: "Khoi");

            migrationBuilder.DropTable(
                name: "GiaiDauMonTheThao");

            migrationBuilder.DropTable(
                name: "GiaiDau");

            migrationBuilder.DropTable(
                name: "MonTheThao");

            migrationBuilder.DropTable(
                name: "DanhMucMonTheThao");
        }
    }
}
