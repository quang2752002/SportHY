using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddMonTheThaoToSanDau : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonTheThaoId",
                table: "SanDau",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SanDau_MonTheThaoId",
                table: "SanDau",
                column: "MonTheThaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_SanDau_MonTheThao_MonTheThaoId",
                table: "SanDau",
                column: "MonTheThaoId",
                principalTable: "MonTheThao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SanDau_MonTheThao_MonTheThaoId",
                table: "SanDau");

            migrationBuilder.DropIndex(
                name: "IX_SanDau_MonTheThaoId",
                table: "SanDau");

            migrationBuilder.DropColumn(
                name: "MonTheThaoId",
                table: "SanDau");
        }
    }
}
