using System;
using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260928075000_AddCauHinhTheThucSchedulingColumns")]
    public partial class AddCauHinhTheThucSchedulingColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThoiLuongTranPhut",
                table: "CauHinhTheThucThiDau",
                type: "int",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "NghiGiuaTranPhut",
                table: "CauHinhTheThucThiDau",
                type: "int",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<int>(
                name: "SoBang",
                table: "CauHinhTheThucThiDau",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoDoiMoiBang",
                table: "CauHinhTheThucThiDau",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "SoDoiMoiBangVaoVongTrong",
                table: "CauHinhTheThucThiDau",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "SoVongThi",
                table: "CauHinhTheThucThiDau",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "PhuongThucPhanNhom",
                table: "CauHinhTheThucThiDau",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "random");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThoiLuongTranPhut",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "NghiGiuaTranPhut",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "SoBang",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "SoDoiMoiBang",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "SoDoiMoiBangVaoVongTrong",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "SoVongThi",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "PhuongThucPhanNhom",
                table: "CauHinhTheThucThiDau");
        }
    }
}
