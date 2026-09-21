using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class delete_table_noidungthidau : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BangDau_NoiDungThiDau_NoiDungThiDauId",
                table: "BangDau");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyThiDau_NoiDungThiDau_NoiDungThiDauId",
                table: "DangKyThiDau");

            migrationBuilder.DropForeignKey(
                name: "FK_HuyChuong_NoiDungThiDau_NoiDungThiDauId",
                table: "HuyChuong");

            migrationBuilder.DropForeignKey(
                name: "FK_TranDau_NoiDungThiDau_NoiDungThiDauId",
                table: "TranDau");

            migrationBuilder.DropForeignKey(
                name: "FK_VongDau_NoiDungThiDau_NoiDungThiDauId",
                table: "VongDau");

            // Cập nhật dữ liệu từ NoiDungThiDau sang GiaiDauMonTheThao trước khi drop bảng
            migrationBuilder.Sql(@"
                UPDATE bd
                SET bd.NoiDungThiDauId = nd.GiaiDauMonTheThaoId
                FROM BangDau bd
                INNER JOIN NoiDungThiDau nd ON bd.NoiDungThiDauId = nd.Id;

                UPDATE dk
                SET dk.NoiDungThiDauId = nd.GiaiDauMonTheThaoId
                FROM DangKyThiDau dk
                INNER JOIN NoiDungThiDau nd ON dk.NoiDungThiDauId = nd.Id;

                UPDATE hc
                SET hc.NoiDungThiDauId = nd.GiaiDauMonTheThaoId
                FROM HuyChuong hc
                INNER JOIN NoiDungThiDau nd ON hc.NoiDungThiDauId = nd.Id;

                UPDATE td
                SET td.NoiDungThiDauId = nd.GiaiDauMonTheThaoId
                FROM TranDau td
                INNER JOIN NoiDungThiDau nd ON td.NoiDungThiDauId = nd.Id;

                UPDATE vd
                SET vd.NoiDungThiDauId = nd.GiaiDauMonTheThaoId
                FROM VongDau vd
                INNER JOIN NoiDungThiDau nd ON vd.NoiDungThiDauId = nd.Id;
            ");

            migrationBuilder.DropTable(
                name: "NoiDungThiDau");

            migrationBuilder.RenameColumn(
                name: "NoiDungThiDauId",
                table: "VongDau",
                newName: "GiaiDauMonTheThaoId");

            migrationBuilder.RenameIndex(
                name: "IX_VongDau_NoiDungThiDauId",
                table: "VongDau",
                newName: "IX_VongDau_GiaiDauMonTheThaoId");

            migrationBuilder.RenameColumn(
                name: "NoiDungThiDauId",
                table: "TranDau",
                newName: "GiaiDauMonTheThaoId");

            migrationBuilder.RenameIndex(
                name: "IX_TranDau_NoiDungThiDauId",
                table: "TranDau",
                newName: "IX_TranDau_GiaiDauMonTheThaoId");

            migrationBuilder.RenameColumn(
                name: "NoiDungThiDauId",
                table: "HuyChuong",
                newName: "GiaiDauMonTheThaoId");

            migrationBuilder.RenameIndex(
                name: "IX_HuyChuong_NoiDungThiDauId",
                table: "HuyChuong",
                newName: "IX_HuyChuong_GiaiDauMonTheThaoId");

            migrationBuilder.RenameColumn(
                name: "NoiDungThiDauId",
                table: "DangKyThiDau",
                newName: "GiaiDauMonTheThaoId");

            migrationBuilder.RenameIndex(
                name: "IX_DangKyThiDau_NoiDungThiDauId",
                table: "DangKyThiDau",
                newName: "IX_DangKyThiDau_GiaiDauMonTheThaoId");

            migrationBuilder.RenameColumn(
                name: "NoiDungThiDauId",
                table: "BangDau",
                newName: "GiaiDauMonTheThaoId");

            migrationBuilder.RenameIndex(
                name: "IX_BangDau_NoiDungThiDauId",
                table: "BangDau",
                newName: "IX_BangDau_GiaiDauMonTheThaoId");

            migrationBuilder.AddColumn<int>(
                name: "SoDoiToiDa",
                table: "MonTheThao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongVanDongVienToiDa",
                table: "MonTheThao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongVanDongVienToiThieu",
                table: "MonTheThao",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BangDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "BangDau",
                column: "GiaiDauMonTheThaoId",
                principalTable: "GiaiDauMonTheThao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyThiDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "DangKyThiDau",
                column: "GiaiDauMonTheThaoId",
                principalTable: "GiaiDauMonTheThao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HuyChuong_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "HuyChuong",
                column: "GiaiDauMonTheThaoId",
                principalTable: "GiaiDauMonTheThao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TranDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "TranDau",
                column: "GiaiDauMonTheThaoId",
                principalTable: "GiaiDauMonTheThao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VongDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "VongDau",
                column: "GiaiDauMonTheThaoId",
                principalTable: "GiaiDauMonTheThao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BangDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "BangDau");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyThiDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "DangKyThiDau");

            migrationBuilder.DropForeignKey(
                name: "FK_HuyChuong_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "HuyChuong");

            migrationBuilder.DropForeignKey(
                name: "FK_TranDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "TranDau");

            migrationBuilder.DropForeignKey(
                name: "FK_VongDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                table: "VongDau");

            migrationBuilder.DropColumn(
                name: "SoDoiToiDa",
                table: "MonTheThao");

            migrationBuilder.DropColumn(
                name: "SoLuongVanDongVienToiDa",
                table: "MonTheThao");

            migrationBuilder.DropColumn(
                name: "SoLuongVanDongVienToiThieu",
                table: "MonTheThao");

            migrationBuilder.RenameColumn(
                name: "GiaiDauMonTheThaoId",
                table: "VongDau",
                newName: "NoiDungThiDauId");

            migrationBuilder.RenameIndex(
                name: "IX_VongDau_GiaiDauMonTheThaoId",
                table: "VongDau",
                newName: "IX_VongDau_NoiDungThiDauId");

            migrationBuilder.RenameColumn(
                name: "GiaiDauMonTheThaoId",
                table: "TranDau",
                newName: "NoiDungThiDauId");

            migrationBuilder.RenameIndex(
                name: "IX_TranDau_GiaiDauMonTheThaoId",
                table: "TranDau",
                newName: "IX_TranDau_NoiDungThiDauId");

            migrationBuilder.RenameColumn(
                name: "GiaiDauMonTheThaoId",
                table: "HuyChuong",
                newName: "NoiDungThiDauId");

            migrationBuilder.RenameIndex(
                name: "IX_HuyChuong_GiaiDauMonTheThaoId",
                table: "HuyChuong",
                newName: "IX_HuyChuong_NoiDungThiDauId");

            migrationBuilder.RenameColumn(
                name: "GiaiDauMonTheThaoId",
                table: "DangKyThiDau",
                newName: "NoiDungThiDauId");

            migrationBuilder.RenameIndex(
                name: "IX_DangKyThiDau_GiaiDauMonTheThaoId",
                table: "DangKyThiDau",
                newName: "IX_DangKyThiDau_NoiDungThiDauId");

            migrationBuilder.RenameColumn(
                name: "GiaiDauMonTheThaoId",
                table: "BangDau",
                newName: "NoiDungThiDauId");

            migrationBuilder.RenameIndex(
                name: "IX_BangDau_GiaiDauMonTheThaoId",
                table: "BangDau",
                newName: "IX_BangDau_NoiDungThiDauId");

            migrationBuilder.CreateTable(
                name: "NoiDungThiDau",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauMonTheThaoId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HinhThucThiDau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiThiDau = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SoLuongToiDa = table.Column<int>(type: "int", nullable: true),
                    SoLuongToiThieu = table.Column<int>(type: "int", nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoiDungThiDau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoiDungThiDau_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                        column: x => x.GiaiDauMonTheThaoId,
                        principalTable: "GiaiDauMonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoiDungThiDau_GiaiDauMonTheThaoId",
                table: "NoiDungThiDau",
                column: "GiaiDauMonTheThaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_BangDau_NoiDungThiDau_NoiDungThiDauId",
                table: "BangDau",
                column: "NoiDungThiDauId",
                principalTable: "NoiDungThiDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyThiDau_NoiDungThiDau_NoiDungThiDauId",
                table: "DangKyThiDau",
                column: "NoiDungThiDauId",
                principalTable: "NoiDungThiDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HuyChuong_NoiDungThiDau_NoiDungThiDauId",
                table: "HuyChuong",
                column: "NoiDungThiDauId",
                principalTable: "NoiDungThiDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TranDau_NoiDungThiDau_NoiDungThiDauId",
                table: "TranDau",
                column: "NoiDungThiDauId",
                principalTable: "NoiDungThiDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VongDau_NoiDungThiDau_NoiDungThiDauId",
                table: "VongDau",
                column: "NoiDungThiDauId",
                principalTable: "NoiDungThiDau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
