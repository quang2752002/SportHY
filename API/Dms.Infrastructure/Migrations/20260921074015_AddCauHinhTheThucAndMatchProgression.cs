using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddCauHinhTheThucAndMatchProgression : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiemPenaltyDoi1",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiemPenaltyDoi2",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoiThangDangKyId",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoiThuaDangKyId",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHoa",
                table: "TranDau",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoserNextTranDauId",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoserNextTranDauViTri",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaTranBracket",
                table: "TranDau",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextTranDauId",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextTranDauViTri",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TySoDoi1",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TySoDoi2",
                table: "TranDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HieuSo",
                table: "ThanhVienBang",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SoSetThang",
                table: "ThanhVienBang",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoSetThua",
                table: "ThanhVienBang",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LoaiHiep",
                table: "HiepDau",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenHiep",
                table: "HiepDau",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CauHinhTheThucThiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonTheThaoId = table.Column<int>(type: "int", nullable: false),
                    GiaiDauMonTheThaoId = table.Column<int>(type: "int", nullable: true),
                    LoaiTheThuc = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoHiepToiDa = table.Column<int>(type: "int", nullable: false),
                    SoHiepThangDeThangTran = table.Column<int>(type: "int", nullable: true),
                    DiemMoiHiep = table.Column<int>(type: "int", nullable: true),
                    DiemHiepQuyetDinh = table.Column<int>(type: "int", nullable: true),
                    CachBietDiemToiThieu = table.Column<int>(type: "int", nullable: false),
                    DiemToiDaMoiHiep = table.Column<int>(type: "int", nullable: true),
                    ThoiGianHiepChinhPhut = table.Column<int>(type: "int", nullable: true),
                    ChoPhepHoaVongBang = table.Column<bool>(type: "bit", nullable: false),
                    ChoPhepHoaKnockout = table.Column<bool>(type: "bit", nullable: false),
                    CoHiepPhu = table.Column<bool>(type: "bit", nullable: false),
                    SoHiepPhu = table.Column<int>(type: "int", nullable: true),
                    ThoiGianHiepPhuPhut = table.Column<int>(type: "int", nullable: true),
                    CoPenalty = table.Column<bool>(type: "bit", nullable: false),
                    SoLuotPenaltyMoiDoi = table.Column<int>(type: "int", nullable: true),
                    CoTieBreak = table.Column<bool>(type: "bit", nullable: false),
                    DiemThang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiemHoa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiemThua = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiemThuaBocCuoc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CachTinhDiemTheoSet = table.Column<bool>(type: "bit", nullable: false),
                    TieuChiXepHangJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiDoThanhTich = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DonViThanhTich = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TieuChiXepHangThanhTich = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SoVdvMoiLuotThi = table.Column<int>(type: "int", nullable: false),
                    QuyCachTienVaoChungKet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoVdvVaoChungKet = table.Column<int>(type: "int", nullable: true),
                    KyLucHienTai = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    KyLucHienTaiText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhTheThucThiDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHinhTheThucThiDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                        column: x => x.GiaiDauMonTheThaoId,
                        principalTable: "GiaiDauMonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CauHinhTheThucThiDau_MonTheThao_MonTheThaoId",
                        column: x => x.MonTheThaoId,
                        principalTable: "MonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_DoiThangDangKyId",
                table: "TranDau",
                column: "DoiThangDangKyId");

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_DoiThuaDangKyId",
                table: "TranDau",
                column: "DoiThuaDangKyId");

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_LoserNextTranDauId",
                table: "TranDau",
                column: "LoserNextTranDauId");

            migrationBuilder.CreateIndex(
                name: "IX_TranDau_NextTranDauId",
                table: "TranDau",
                column: "NextTranDauId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhTheThucThiDau_GiaiDauMonTheThaoId",
                table: "CauHinhTheThucThiDau",
                column: "GiaiDauMonTheThaoId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhTheThucThiDau_MonTheThaoId",
                table: "CauHinhTheThucThiDau",
                column: "MonTheThaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_TranDau_DangKyThiDau_DoiThangDangKyId",
                table: "TranDau",
                column: "DoiThangDangKyId",
                principalTable: "DangKyThiDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TranDau_DangKyThiDau_DoiThuaDangKyId",
                table: "TranDau",
                column: "DoiThuaDangKyId",
                principalTable: "DangKyThiDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TranDau_TranDau_LoserNextTranDauId",
                table: "TranDau",
                column: "LoserNextTranDauId",
                principalTable: "TranDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TranDau_TranDau_NextTranDauId",
                table: "TranDau",
                column: "NextTranDauId",
                principalTable: "TranDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TranDau_DangKyThiDau_DoiThangDangKyId",
                table: "TranDau");

            migrationBuilder.DropForeignKey(
                name: "FK_TranDau_DangKyThiDau_DoiThuaDangKyId",
                table: "TranDau");

            migrationBuilder.DropForeignKey(
                name: "FK_TranDau_TranDau_LoserNextTranDauId",
                table: "TranDau");

            migrationBuilder.DropForeignKey(
                name: "FK_TranDau_TranDau_NextTranDauId",
                table: "TranDau");

            migrationBuilder.DropTable(
                name: "CauHinhTheThucThiDau");

            migrationBuilder.DropIndex(
                name: "IX_TranDau_DoiThangDangKyId",
                table: "TranDau");

            migrationBuilder.DropIndex(
                name: "IX_TranDau_DoiThuaDangKyId",
                table: "TranDau");

            migrationBuilder.DropIndex(
                name: "IX_TranDau_LoserNextTranDauId",
                table: "TranDau");

            migrationBuilder.DropIndex(
                name: "IX_TranDau_NextTranDauId",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "DiemPenaltyDoi1",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "DiemPenaltyDoi2",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "DoiThangDangKyId",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "DoiThuaDangKyId",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "IsHoa",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "LoserNextTranDauId",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "LoserNextTranDauViTri",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "MaTranBracket",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "NextTranDauId",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "NextTranDauViTri",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "TySoDoi1",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "TySoDoi2",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "HieuSo",
                table: "ThanhVienBang");

            migrationBuilder.DropColumn(
                name: "SoSetThang",
                table: "ThanhVienBang");

            migrationBuilder.DropColumn(
                name: "SoSetThua",
                table: "ThanhVienBang");

            migrationBuilder.DropColumn(
                name: "LoaiHiep",
                table: "HiepDau");

            migrationBuilder.DropColumn(
                name: "TenHiep",
                table: "HiepDau");
        }
    }
}
