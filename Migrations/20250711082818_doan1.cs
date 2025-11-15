using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class doan1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GioHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GioHangs_SanPhams_ProductId",
                        column: x => x.ProductId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GioHangs_TaiKhoans_UserId",
                        column: x => x.UserId,
                        principalTable: "TaiKhoans",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "SanPhams",
                columns: new[] { "Id", "ImageUrl", "Name", "Price", "SoLuong" },
                values: new object[,]
                {
                    { 1, "https://via.placeholder.com/300x200?text=Galaxy+X", "Smartphone Galaxy X", 699.99m, 1000 },
                    { 2, "https://via.placeholder.com/300x200?text=Laptop+Pro", "Laptop Pro 15", 1299.99m, 1000 },
                    { 3, "https://via.placeholder.com/300x200?text=Headphones", "Wireless Headphones Elite", 199.99m, 1000 },
                    { 4, "https://via.placeholder.com/300x200?text=Smartwatch", "Smartwatch Series 7", 399.99m, 1000 },
                    { 5, "https://via.placeholder.com/300x200?text=Smart+TV", "4K Smart TV 55\"", 799.99m, 1000 },
                    { 6, "https://via.placeholder.com/300x200?text=Speaker", "Bluetooth Speaker", 149.99m, 1000 },
                    { 7, "https://via.placeholder.com/300x200?text=Console", "Gaming Console X", 499.99m, 1000 },
                    { 8, "https://via.placeholder.com/300x200?text=Mouse", "Wireless Mouse", 49.99m, 1000 },
                    { 9, "https://via.placeholder.com/300x200?text=Charger", "Portable Charger 10000mAh", 29.99m, 1000 },
                    { 10, "https://via.placeholder.com/300x200?text=Tablet", "Tablet Air 10", 499.99m, 1000 },
                    { 11, "https://via.placeholder.com/300x200?text=Keyboard", "Wireless Keyboard", 79.99m, 1000 },
                    { 12, "https://via.placeholder.com/300x200?text=Camera", "Action Camera 4K", 249.99m, 1000 }
                });

            migrationBuilder.InsertData(
                table: "TaiKhoans",
                columns: new[] { "Id", "Address", "DateOfBirth", "Email", "FullName", "Gender", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "123 Đường Admin, TP.HCM", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@example.com", "Quản Trị Viên", "Male", "password", "Admin", "admin" },
                    { 2, "456 Đường User, Hà Nội", new DateTime(1995, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "user@example.com", "Nguyễn Văn A", "Male", "password", "User", "user" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_ProductId",
                table: "GioHangs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_UserId",
                table: "GioHangs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GioHangs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "TaiKhoans");
        }
    }
}
