using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhanCongThuKy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhanCongThuKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    GiaiDauId = table.Column<int>(type: "int", nullable: false),
                    ThuKyId = table.Column<int>(type: "int", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanCongThuKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanCongThuKy_GiaiDau_GiaiDauId",
                        column: x => x.GiaiDauId,
                        principalTable: "GiaiDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanCongThuKy_ThuKy_ThuKyId",
                        column: x => x.ThuKyId,
                        principalTable: "ThuKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongThuKy_GiaiDauId",
                table: "PhanCongThuKy",
                column: "GiaiDauId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongThuKy_ThuKyId",
                table: "PhanCongThuKy",
                column: "ThuKyId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongThuKy_GiaiDauId_ThuKyId",
                table: "PhanCongThuKy",
                columns: new[] { "GiaiDauId", "ThuKyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhanCongThuKy");
        }
    }
}
