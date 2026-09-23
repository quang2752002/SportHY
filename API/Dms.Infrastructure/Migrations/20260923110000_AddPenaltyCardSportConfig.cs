using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddPenaltyCardSportConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CoThePhat",
                table: "CauHinhTheThucThiDau",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE cfg SET CoThePhat = 1 FROM CauHinhTheThucThiDau AS cfg " +
                "INNER JOIN MonTheThao AS mon ON mon.Id = cfg.MonTheThaoId WHERE mon.Ma = N'BONG_DA';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoThePhat",
                table: "CauHinhTheThucThiDau");
        }
    }
}
