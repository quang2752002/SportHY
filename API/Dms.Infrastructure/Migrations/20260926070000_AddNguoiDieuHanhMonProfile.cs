using Dms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260926070000_AddNguoiDieuHanhMonProfile")]
    public partial class AddNguoiDieuHanhMonProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NguoiDieuHanhMon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DonViCongTac = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_NguoiDieuHanhMon", x => x.Id));

            migrationBuilder.AddColumn<int>(
                name: "NguoiDieuHanhMonId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NguoiDieuHanhMonId",
                table: "PhanCongDieuHanhMon",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
INSERT INTO [NguoiDieuHanhMon]
    ([Ma], [HoTen], [SoDienThoai], [Email], [TrangThai], [Created], [CreatedBy], [IsDeleted])
SELECT
    N'NDH-' + CONVERT(nvarchar(20), u.[Id]),
    COALESCE(NULLIF(u.[FullName], N''), NULLIF(u.[UserName], N''), N'Người điều hành môn'),
    u.[PhoneNumber],
    u.[Email],
    1,
    SYSUTCDATETIME(),
    N'Migration',
    0
FROM [Users] AS u
WHERE u.[NguoiDieuHanhMonId] IS NULL
  AND (
      EXISTS (
          SELECT 1
          FROM [UserRoles] AS ur
          INNER JOIN [Roles] AS r ON r.[Id] = ur.[RoleId]
          WHERE ur.[UserId] = u.[Id] AND r.[Name] = N'SportCoordinator'
      )
      OR EXISTS (
          SELECT 1 FROM [PhanCongDieuHanhMon] AS pc WHERE pc.[ApplicationUserId] = u.[Id]
      )
  );

UPDATE u
SET u.[NguoiDieuHanhMonId] = p.[Id]
FROM [Users] AS u
INNER JOIN [NguoiDieuHanhMon] AS p
    ON p.[Ma] = N'NDH-' + CONVERT(nvarchar(20), u.[Id]);

UPDATE pc
SET pc.[NguoiDieuHanhMonId] = u.[NguoiDieuHanhMonId]
FROM [PhanCongDieuHanhMon] AS pc
INNER JOIN [Users] AS u ON u.[Id] = pc.[ApplicationUserId];
");

            migrationBuilder.AlterColumn<int>(
                name: "NguoiDieuHanhMonId",
                table: "PhanCongDieuHanhMon",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_PhanCongDieuHanhMon_Users_ApplicationUserId",
                table: "PhanCongDieuHanhMon");

            migrationBuilder.DropIndex(
                name: "IX_PhanCongDieuHanhMon_ApplicationUserId",
                table: "PhanCongDieuHanhMon");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "PhanCongDieuHanhMon");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongDieuHanhMon_NguoiDieuHanhMonId",
                table: "PhanCongDieuHanhMon",
                column: "NguoiDieuHanhMonId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhanCongDieuHanhMon_NguoiDieuHanhMon_NguoiDieuHanhMonId",
                table: "PhanCongDieuHanhMon",
                column: "NguoiDieuHanhMonId",
                principalTable: "NguoiDieuHanhMon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NguoiDieuHanhMonId",
                table: "Users",
                column: "NguoiDieuHanhMonId",
                unique: true,
                filter: "[NguoiDieuHanhMonId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_NguoiDieuHanhMon_NguoiDieuHanhMonId",
                table: "Users",
                column: "NguoiDieuHanhMonId",
                principalTable: "NguoiDieuHanhMon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApplicationUserId",
                table: "PhanCongDieuHanhMon",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE pc
SET pc.[ApplicationUserId] = u.[Id]
FROM [PhanCongDieuHanhMon] AS pc
INNER JOIN [Users] AS u ON u.[NguoiDieuHanhMonId] = pc.[NguoiDieuHanhMonId];
");

            migrationBuilder.AlterColumn<int>(
                name: "ApplicationUserId",
                table: "PhanCongDieuHanhMon",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_PhanCongDieuHanhMon_NguoiDieuHanhMon_NguoiDieuHanhMonId",
                table: "PhanCongDieuHanhMon");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_NguoiDieuHanhMon_NguoiDieuHanhMonId",
                table: "Users");

            migrationBuilder.DropIndex(name: "IX_PhanCongDieuHanhMon_NguoiDieuHanhMonId", table: "PhanCongDieuHanhMon");
            migrationBuilder.DropIndex(name: "IX_Users_NguoiDieuHanhMonId", table: "Users");
            migrationBuilder.DropColumn(name: "NguoiDieuHanhMonId", table: "PhanCongDieuHanhMon");
            migrationBuilder.DropColumn(name: "NguoiDieuHanhMonId", table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_PhanCongDieuHanhMon_ApplicationUserId",
                table: "PhanCongDieuHanhMon",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhanCongDieuHanhMon_Users_ApplicationUserId",
                table: "PhanCongDieuHanhMon",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(name: "NguoiDieuHanhMon");
        }
    }
}
