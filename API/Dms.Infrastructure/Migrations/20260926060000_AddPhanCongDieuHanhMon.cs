using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260926060000_AddPhanCongDieuHanhMon")]
    public partial class AddPhanCongDieuHanhMon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhanCongDieuHanhMon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauId = table.Column<int>(type: "int", nullable: false),
                    DanhMucId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongDieuHanhMon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanCongDieuHanhMon_DanhMucMonTheThao_DanhMucId",
                        column: x => x.DanhMucId,
                        principalTable: "DanhMucMonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongDieuHanhMon_GiaiDau_GiaiDauId",
                        column: x => x.GiaiDauId,
                        principalTable: "GiaiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongDieuHanhMon_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongDieuHanhMon_ApplicationUserId",
                table: "PhanCongDieuHanhMon",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongDieuHanhMon_DanhMucId",
                table: "PhanCongDieuHanhMon",
                column: "DanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongDieuHanhMon_GiaiDauId",
                table: "PhanCongDieuHanhMon",
                column: "GiaiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongDieuHanhMon_GiaiDauId_DanhMucId",
                table: "PhanCongDieuHanhMon",
                columns: new[] { "GiaiDauId", "DanhMucId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PhanCongDieuHanhMon");
        }
    }
}
