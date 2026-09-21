using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class AddTruongBanAndNguoiDieuHanh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NguoiDieuHanhId",
                table: "GiaiDauMonTheThao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TruongBanTrongTaiId",
                table: "GiaiDau",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDauMonTheThao_NguoiDieuHanhId",
                table: "GiaiDauMonTheThao",
                column: "NguoiDieuHanhId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaiDau_TruongBanTrongTaiId",
                table: "GiaiDau",
                column: "TruongBanTrongTaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_GiaiDau_TrongTai_TruongBanTrongTaiId",
                table: "GiaiDau",
                column: "TruongBanTrongTaiId",
                principalTable: "TrongTai",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GiaiDauMonTheThao_TrongTai_NguoiDieuHanhId",
                table: "GiaiDauMonTheThao",
                column: "NguoiDieuHanhId",
                principalTable: "TrongTai",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiaiDau_TrongTai_TruongBanTrongTaiId",
                table: "GiaiDau");

            migrationBuilder.DropForeignKey(
                name: "FK_GiaiDauMonTheThao_TrongTai_NguoiDieuHanhId",
                table: "GiaiDauMonTheThao");

            migrationBuilder.DropIndex(
                name: "IX_GiaiDauMonTheThao_NguoiDieuHanhId",
                table: "GiaiDauMonTheThao");

            migrationBuilder.DropIndex(
                name: "IX_GiaiDau_TruongBanTrongTaiId",
                table: "GiaiDau");

            migrationBuilder.DropColumn(
                name: "NguoiDieuHanhId",
                table: "GiaiDauMonTheThao");

            migrationBuilder.DropColumn(
                name: "TruongBanTrongTaiId",
                table: "GiaiDau");
        }
    }
}
