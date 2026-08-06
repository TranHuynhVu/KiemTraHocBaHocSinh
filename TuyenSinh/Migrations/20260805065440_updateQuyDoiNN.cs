using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenSinh.Migrations
{
    /// <inheritdoc />
    public partial class updateQuyDoiNN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemNNDen",
                table: "QuyDoiNNs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiemNNDen",
                table: "QuyDoiNNs",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
