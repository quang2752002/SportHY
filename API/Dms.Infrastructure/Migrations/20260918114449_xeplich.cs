using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dms.Infrastructure.Migrations
{
    public partial class xeplich : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID('GiaiDau') AND name = 'HanDangKy'
                )
                BEGIN
                    ALTER TABLE [GiaiDau] ADD [HanDangKy] datetime2 NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HanDangKy",
                table: "GiaiDau");
        }
    }
}
