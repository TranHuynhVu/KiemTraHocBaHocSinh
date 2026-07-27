using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenSinh.Migrations
{
    /// <inheritdoc />
    public partial class updateNganh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgungDauVao",
                table: "Nganhs");

            migrationBuilder.AlterColumn<string>(
                name: "ToHopXetTuyen",
                table: "Nganhs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DXT",
                table: "Nganhs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiemSanToan",
                table: "Nganhs",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NguongDauVao",
                table: "Nganhs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DXT",
                table: "Nganhs");

            migrationBuilder.DropColumn(
                name: "DiemSanToan",
                table: "Nganhs");

            migrationBuilder.DropColumn(
                name: "NguongDauVao",
                table: "Nganhs");

            migrationBuilder.AlterColumn<string>(
                name: "ToHopXetTuyen",
                table: "Nganhs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "NgungDauVao",
                table: "Nganhs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
