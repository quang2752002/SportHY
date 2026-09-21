using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddCauHinhLichThiDau : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHinhLichThiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonTheThaoId = table.Column<int>(type: "int", nullable: false),
                    MoiVongMotNgay = table.Column<bool>(type: "bit", nullable: false),
                    KhoangCachGiuaCacVongGio = table.Column<int>(type: "int", nullable: false),
                    UuTienChungKetNgayCuoi = table.Column<bool>(type: "bit", nullable: false),
                    SoTranToiDaMoiDoiMoiNgay = table.Column<int>(type: "int", nullable: false),
                    NghiToiThieuGiua2TranPhut = table.Column<int>(type: "int", nullable: false),
                    ChiaCaThiDau = table.Column<bool>(type: "bit", nullable: false),
                    CaSangBatDau = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CaSangKetThuc = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CaChieuBatDau = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CaChieuKetThuc = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    CaToBatDau = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CaToKetThuc = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ThoiGianDemDonSanPhut = table.Column<int>(type: "int", nullable: false),
                    SoTranToiDaMoiTrongTaiMoiNgay = table.Column<int>(type: "int", nullable: false),
                    NghiToiThieuTrongTaiPhut = table.Column<int>(type: "int", nullable: false),
                    ThoiGianDemDiChuyenPhut = table.Column<int>(type: "int", nullable: false),
                    ThoiLuongTranMacDinhPhut = table.Column<int>(type: "int", nullable: false),
                    SoHiepDauMacDinh = table.Column<int>(type: "int", nullable: false),
                    ThoiGianMoiHiepPhut = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhLichThiDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHinhLichThiDau_MonTheThao_MonTheThaoId",
                        column: x => x.MonTheThaoId,
                        principalTable: "MonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhLichThiDau_MonTheThaoId",
                table: "CauHinhLichThiDau",
                column: "MonTheThaoId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinhLichThiDau");
        }
    }
}
