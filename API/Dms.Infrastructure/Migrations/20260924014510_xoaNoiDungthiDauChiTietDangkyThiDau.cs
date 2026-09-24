using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class xoaNoiDungthiDauChiTietDangkyThiDau : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDangKyThiDau");

            migrationBuilder.DropIndex(
                name: "IX_PhanCongThuKy_GiaiDauId",
                table: "PhanCongThuKy");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "TranDau",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'LoaiThiDau' 
                    AND Object_ID = Object_ID(N'MonTheThao')
                )
                BEGIN
                    ALTER TABLE [MonTheThao] ADD [LoaiThiDau] nvarchar(30) NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'LoaiThiDau' 
                    AND Object_ID = Object_ID(N'MonTheThao')
                )
                BEGIN
                    ALTER TABLE [MonTheThao] DROP COLUMN [LoaiThiDau];
                END
            ");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "TranDau",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ChiTietDangKyThiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DangKyThiDauId = table.Column<int>(type: "int", nullable: false),
                    VanDongVienId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoThuTu = table.Column<int>(type: "int", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongThuKy_GiaiDauId",
                table: "PhanCongThuKy",
                column: "GiaiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDangKyThiDau_DangKyThiDauId",
                table: "ChiTietDangKyThiDau",
                column: "DangKyThiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDangKyThiDau_VanDongVienId",
                table: "ChiTietDangKyThiDau",
                column: "VanDongVienId");
        }
    }
}
