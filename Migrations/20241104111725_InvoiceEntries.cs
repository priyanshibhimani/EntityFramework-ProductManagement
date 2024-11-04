using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagemet.Migrations
{
    public partial class InvoiceEntries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalProducts",
                table: "PartyTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InvoiceEntry",
                columns: table => new
                {
                    InvoiceEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceEntry", x => x.InvoiceEntryId);
                    table.ForeignKey(
                        name: "FK_InvoiceEntry_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "PartyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceEntry_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceEntry_PartyId",
                table: "InvoiceEntry",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceEntry_ProductId",
                table: "InvoiceEntry",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceEntry");

            migrationBuilder.DropColumn(
                name: "TotalProducts",
                table: "PartyTotal");
        }
    }
}
