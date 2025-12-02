using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "PhieuChuyenKho",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MaPhieu",
                table: "PhieuChuyenKho",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "PhieuChuyenKho",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "BangLuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    DoanhSoDatDuoc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LuongCung = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TienHoaHong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongThucNhan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayChot = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiChot = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BangLuong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BangLuong_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhLuong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaiKhoanId = table.Column<int>(type: "int", nullable: false),
                    LuongCung = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhanTramHoaHong = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhLuong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHinhLuong_TaiKhoan_TaiKhoanId",
                        column: x => x.TaiKhoanId,
                        principalTable: "TaiKhoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BangLuong_TaiKhoanId",
                table: "BangLuong",
                column: "TaiKhoanId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhLuong_TaiKhoanId",
                table: "CauHinhLuong",
                column: "TaiKhoanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BangLuong");

            migrationBuilder.DropTable(
                name: "CauHinhLuong");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiTao",
                table: "PhieuChuyenKho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaPhieu",
                table: "PhieuChuyenKho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GhiChu",
                table: "PhieuChuyenKho",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
