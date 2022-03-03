using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Final_Project_Backend.Migrations
{
    public partial class ProductDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    FileType1 = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    File1 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileType2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File2 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileType3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File3 = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
