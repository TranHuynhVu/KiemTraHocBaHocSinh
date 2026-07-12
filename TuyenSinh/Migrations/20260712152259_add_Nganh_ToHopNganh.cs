using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuyenSinh.Migrations
{
    /// <inheritdoc />
    public partial class add_Nganh_ToHopNganh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nganhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNganh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenNganh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HeSoTHPT = table.Column<float>(type: "real", nullable: false),
                    HeSoHB = table.Column<float>(type: "real", nullable: false),
                    ToHopXetTuyen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgungDauVao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nganhs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToHopNganhs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNganhId = table.Column<int>(type: "int", nullable: false),
                    ToHopId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToHopNganhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToHopNganhs_Nganhs_MaNganhId",
                        column: x => x.MaNganhId,
                        principalTable: "Nganhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToHopNganhs_ToHopMons_ToHopId",
                        column: x => x.ToHopId,
                        principalTable: "ToHopMons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToHopNganhs_MaNganhId",
                table: "ToHopNganhs",
                column: "MaNganhId");

            migrationBuilder.CreateIndex(
                name: "IX_ToHopNganhs_ToHopId",
                table: "ToHopNganhs",
                column: "ToHopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToHopNganhs");

            migrationBuilder.DropTable(
                name: "Nganhs");
        }
    }
}
