using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoreId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "TaiKhoan",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address", "DateOfBirth", "Email", "FullName", "Gender", "Password", "PhoneNumber", "Role", "StoreId", "Username" },
                values: new object[] { "Trụ sở chính - Sky Tower", new DateTime(1980, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "boss@smarttech.com", "Chủ Tịch (CEO)", "Nam", "123", "0999999999", "Boss", null, "boss" });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address", "DateOfBirth", "Email", "FullName", "Gender", "Password", "PhoneNumber", "Role", "StoreId", "Username" },
                values: new object[] { "120 Xuân Thủy, Cầu Giấy, HN", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin.hn@smarttech.com", "Quản Lý Hà Nội", "Nam", "123", "0988111222", "Admin", 1, "admin_hn" });

            migrationBuilder.InsertData(
                table: "TaiKhoan",
                columns: new[] { "Id", "Address", "DateOfBirth", "Email", "FullName", "Gender", "Password", "PhoneNumber", "Role", "StoreId", "Username" },
                values: new object[,]
                {
                    { 3, "78 Bạch Đằng, Hải Châu, ĐN", new DateTime(1992, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin.dn@smarttech.com", "Quản Lý Đà Nẵng", "Nữ", "123", "0988333444", "Admin", 2, "admin_dn" },
                    { 4, "55 Nguyễn Huệ, Quận 1, TP.HCM", new DateTime(1995, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin.sg@smarttech.com", "Quản Lý Sài Gòn", "Nam", "123", "0988555666", "Admin", 3, "admin_sg" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "TaiKhoan");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "TaiKhoan",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "TaiKhoan",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "TaiKhoan",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "TaiKhoan",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "TaiKhoan",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "TaiKhoan",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Address", "DateOfBirth", "Email", "FullName", "Gender", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { "123 Đường Admin, TP.HCM", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@example.com", "Quản Trị Viên", "Male", "password", "0901234567", "Admin", "admin" });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Address", "DateOfBirth", "Email", "FullName", "Gender", "Password", "PhoneNumber", "Role", "Username" },
                values: new object[] { "456 Đường User, Hà Nội", new DateTime(1995, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "user@example.com", "Nguyễn Văn A", "Male", "password", "0909876543", "User", "user" });
        }
    }
}
