using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddSchedulingAndRefereeUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonViId",
                table: "TrongTai",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLichCoDinh",
                table: "TranDau",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaTranHienThi",
                table: "TranDau",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoNgayNghiSauVongBang",
                table: "CauHinhLichThiDau",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThoiGianDemHiepPhuLuonLuuPhut",
                table: "CauHinhLichThiDau",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrongTai_DonViId",
                table: "TrongTai",
                column: "DonViId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrongTai_DonVi_DonViId",
                table: "TrongTai",
                column: "DonViId",
                principalTable: "DonVi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrongTai_DonVi_DonViId",
                table: "TrongTai");

            migrationBuilder.DropIndex(
                name: "IX_TrongTai_DonViId",
                table: "TrongTai");

            migrationBuilder.DropColumn(
                name: "DonViId",
                table: "TrongTai");

            migrationBuilder.DropColumn(
                name: "IsLichCoDinh",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "MaTranHienThi",
                table: "TranDau");

            migrationBuilder.DropColumn(
                name: "SoNgayNghiSauVongBang",
                table: "CauHinhLichThiDau");

            migrationBuilder.DropColumn(
                name: "ThoiGianDemHiepPhuLuonLuuPhut",
                table: "CauHinhLichThiDau");
        }
    }
}
