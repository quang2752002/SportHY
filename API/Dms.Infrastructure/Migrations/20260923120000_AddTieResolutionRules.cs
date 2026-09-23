using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddTieResolutionRules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChoPhepDongHangThanhTich",
                table: "CauHinhTheThucThiDau",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "TenTieuChiPhuThanhTich",
                table: "CauHinhTheThucThiDau",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieuChiPhuCangNhoCangTot",
                table: "CauHinhTheThucThiDau",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(
                "UPDATE [CauHinhTheThucThiDau] SET [ChoPhepDongHangThanhTich] = 1 " +
                "WHERE [LoaiTheThuc] = N'TinhDiemXepHang';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenTieuChiPhuThanhTich",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "ChoPhepDongHangThanhTich",
                table: "CauHinhTheThucThiDau");

            migrationBuilder.DropColumn(
                name: "TieuChiPhuCangNhoCangTot",
                table: "CauHinhTheThucThiDau");
        }
    }
}
