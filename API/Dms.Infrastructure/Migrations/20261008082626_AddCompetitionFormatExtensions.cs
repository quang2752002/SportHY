using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddCompetitionFormatExtensions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoDeoBIB",
                table: "ThanhPhanTranDau",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChiTietKetQuaJson",
                table: "KetQuaTranDau",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CachTinhKetQuaLuotThi",
                table: "CauHinhTheThucThiDau",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CoDiemTruBieuDien",
                table: "CauHinhTheThucThiDau",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HinhThucXuatPhat",
                table: "CauHinhTheThucThiDau",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuotThucHien",
                table: "CauHinhTheThucThiDau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThangDiemToiDa",
                table: "CauHinhTheThucThiDau",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoDeoBIB",
                table: "ThanhPhanTranDau");

            migrationBuilder.DropColumn(
                name: "ChiTietKetQuaJson",
                table: "KetQuaTranDau");

            migrationBuilder.DropColumn(
                name: "CachTinhKetQuaLuotThi",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "CoDiemTruBieuDien",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "HinhThucXuatPhat",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "SoLuotThucHien",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "ThangDiemToiDa",
                table: "CauHinhTheThucThiDau");
        }
    }
}
