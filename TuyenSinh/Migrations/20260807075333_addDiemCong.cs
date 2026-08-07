using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenSinh.Migrations
{
    /// <inheritdoc />
    public partial class addDiemCong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiemCong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DDCN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaXetTuyen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaPTXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaToHop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiDiemCong = table.Column<int>(type: "int", nullable: false),
                    Diem = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NamHoc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemCong", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiemCong");
        }
    }
}
