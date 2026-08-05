using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenSinh.Migrations
{
    /// <inheritdoc />
    public partial class create_quydoinn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BacNgoaiNgus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBac = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenVietTat = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacNgoaiNgus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoaiNgoaiNgus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiNgoaiNgus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuyDoiNNs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BacNgoaiNguId = table.Column<int>(type: "int", nullable: false),
                    LoaiNgoaiNguId = table.Column<int>(type: "int", nullable: false),
                    DiemNN = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiemQuyDoi = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyDoiNNs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyDoiNNs_BacNgoaiNgus_BacNgoaiNguId",
                        column: x => x.BacNgoaiNguId,
                        principalTable: "BacNgoaiNgus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuyDoiNNs_LoaiNgoaiNgus_LoaiNgoaiNguId",
                        column: x => x.LoaiNgoaiNguId,
                        principalTable: "LoaiNgoaiNgus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuyDoiNNs_BacNgoaiNguId",
                table: "QuyDoiNNs",
                column: "BacNgoaiNguId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyDoiNNs_LoaiNgoaiNguId",
                table: "QuyDoiNNs",
                column: "LoaiNgoaiNguId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuyDoiNNs");

            migrationBuilder.DropTable(
                name: "BacNgoaiNgus");

            migrationBuilder.DropTable(
                name: "LoaiNgoaiNgus");
        }
    }
}
