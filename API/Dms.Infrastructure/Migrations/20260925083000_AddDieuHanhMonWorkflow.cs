using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260925083000_AddDieuHanhMonWorkflow")]
    public partial class AddDieuHanhMonWorkflow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrangThaiDuyetKetQua",
                table: "TranDau",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ChoDuyet");

            migrationBuilder.AddColumn<DateTime>(
                name: "ThoiGianDuyetKetQua",
                table: "TranDau",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDuyetKetQua",
                table: "TranDau",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChuDuyetKetQua",
                table: "TranDau",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SuCoDieuHanhMon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GiaiDauMonTheThaoId = table.Column<int>(type: "int", nullable: false),
                    TranDauId = table.Column<int>(type: "int", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MucDo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NguoiPhuTrach = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    GhiChuXuLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianGiaiQuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuCoDieuHanhMon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuCoDieuHanhMon_GiaiDauMonTheThao_GiaiDauMonTheThaoId",
                        column: x => x.GiaiDauMonTheThaoId,
                        principalTable: "GiaiDauMonTheThao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuCoDieuHanhMon_TranDau_TranDauId",
                        column: x => x.TranDauId,
                        principalTable: "TranDau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuCoDieuHanhMon_GiaiDauMonTheThaoId_TrangThai",
                table: "SuCoDieuHanhMon",
                columns: new[] { "GiaiDauMonTheThaoId", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_SuCoDieuHanhMon_TranDauId",
                table: "SuCoDieuHanhMon",
                column: "TranDauId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SuCoDieuHanhMon");

            migrationBuilder.DropColumn(name: "TrangThaiDuyetKetQua", table: "TranDau");
            migrationBuilder.DropColumn(name: "ThoiGianDuyetKetQua", table: "TranDau");
            migrationBuilder.DropColumn(name: "NguoiDuyetKetQua", table: "TranDau");
            migrationBuilder.DropColumn(name: "GhiChuDuyetKetQua", table: "TranDau");
        }
    }
}
