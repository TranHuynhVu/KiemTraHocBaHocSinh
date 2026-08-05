using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenSinh.Migrations
{
    /// <inheritdoc />
    public partial class add_DiemNNDen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiemNNDen",
                table: "QuyDoiNNs",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiemNNDen",
                table: "QuyDoiNNs");
        }
    }
}
