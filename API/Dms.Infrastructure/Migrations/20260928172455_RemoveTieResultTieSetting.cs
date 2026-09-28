using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260928172455_RemoveTieResultTieSetting")]
    public partial class RemoveTieResultTieSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChoPhepDongHangThanhTich",
                table: "CauHinhTheThucThiDau");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChoPhepDongHangThanhTich",
                table: "CauHinhTheThucThiDau",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
