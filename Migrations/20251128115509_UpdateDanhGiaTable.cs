using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDanhGiaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaDuyet",
                table: "DanhGia",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "DanhGia",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTraLoi",
                table: "DanhGia",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraLoi",
                table: "DanhGia",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaDuyet",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "NgayTraLoi",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "TraLoi",
                table: "DanhGia");
        }
    }
}
