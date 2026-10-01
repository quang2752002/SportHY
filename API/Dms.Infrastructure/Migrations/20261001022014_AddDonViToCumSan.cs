using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddDonViToCumSan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonViId",
                table: "CumSan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CumSan_DonViId",
                table: "CumSan",
                column: "DonViId");

            migrationBuilder.AddForeignKey(
                name: "FK_CumSan_DonVi_DonViId",
                table: "CumSan",
                column: "DonViId",
                principalTable: "DonVi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CumSan_DonVi_DonViId",
                table: "CumSan");

            migrationBuilder.DropIndex(
                name: "IX_CumSan_DonViId",
                table: "CumSan");

            migrationBuilder.DropColumn(
                name: "DonViId",
                table: "CumSan");
        }
    }
}
