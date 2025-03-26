using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebADS.Migrations
{
    /// <inheritdoc />
    public partial class addParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "articleparameters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductID = table.Column<string>(type: "text", nullable: false),
                    AWS = table.Column<float>(type: "real", nullable: false),
                    SalesMethod = table.Column<string>(type: "text", nullable: false),
                    MinSalesQty = table.Column<float>(type: "real", nullable: false),
                    MultipackQty = table.Column<float>(type: "real", nullable: false),
                    PalletQty = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articleparameters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_articleparameters_ProductID",
                table: "articleparameters",
                column: "ProductID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "articleparameters");
        }
    }
}
