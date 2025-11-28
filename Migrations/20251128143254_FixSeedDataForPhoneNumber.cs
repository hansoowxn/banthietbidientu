using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataForPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_TaiKhoans_TaiKhoanId",
                table: "DanhGia");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_TaiKhoans_TaiKhoanId",
                table: "DonHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_TaiKhoans_TaiKhoanId",
                table: "GioHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuMuaHangs_TaiKhoans_UserId",
                table: "LichSuMuaHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaiKhoans",
                table: "TaiKhoans");

            migrationBuilder.RenameTable(
                name: "TaiKhoans",
                newName: "TaiKhoan");

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
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "TaiKhoan",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TaiKhoan",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "TaiKhoan",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaiKhoan",
                table: "TaiKhoan",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhoneNumber",
                value: "0901234567");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhoneNumber",
                value: "0909876543");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_TaiKhoan_TaiKhoanId",
                table: "DanhGia",
                column: "TaiKhoanId",
                principalTable: "TaiKhoan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_TaiKhoan_TaiKhoanId",
                table: "DonHangs",
                column: "TaiKhoanId",
                principalTable: "TaiKhoan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_TaiKhoan_TaiKhoanId",
                table: "GioHangs",
                column: "TaiKhoanId",
                principalTable: "TaiKhoan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuMuaHangs_TaiKhoan_UserId",
                table: "LichSuMuaHangs",
                column: "UserId",
                principalTable: "TaiKhoan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_TaiKhoan_TaiKhoanId",
                table: "DanhGia");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_TaiKhoan_TaiKhoanId",
                table: "DonHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_TaiKhoan_TaiKhoanId",
                table: "GioHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuMuaHangs_TaiKhoan_UserId",
                table: "LichSuMuaHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaiKhoan",
                table: "TaiKhoan");

            migrationBuilder.RenameTable(
                name: "TaiKhoan",
                newName: "TaiKhoans");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "TaiKhoans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TaiKhoans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "TaiKhoans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaiKhoans",
                table: "TaiKhoans",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "TaiKhoans",
                keyColumn: "Id",
                keyValue: 1,
                column: "PhoneNumber",
                value: null);

            migrationBuilder.UpdateData(
                table: "TaiKhoans",
                keyColumn: "Id",
                keyValue: 2,
                column: "PhoneNumber",
                value: null);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_TaiKhoans_TaiKhoanId",
                table: "DanhGia",
                column: "TaiKhoanId",
                principalTable: "TaiKhoans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_TaiKhoans_TaiKhoanId",
                table: "DonHangs",
                column: "TaiKhoanId",
                principalTable: "TaiKhoans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_TaiKhoans_TaiKhoanId",
                table: "GioHangs",
                column: "TaiKhoanId",
                principalTable: "TaiKhoans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuMuaHangs_TaiKhoans_UserId",
                table: "LichSuMuaHangs",
                column: "UserId",
                principalTable: "TaiKhoans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
