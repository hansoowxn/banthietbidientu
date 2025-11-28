using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class AddTaiKhoanToThuMua : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaiKhoanId",
                table: "YeuCauThuMua",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauThuMua_TaiKhoanId",
                table: "YeuCauThuMua",
                column: "TaiKhoanId");

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauThuMua_TaiKhoan_TaiKhoanId",
                table: "YeuCauThuMua",
                column: "TaiKhoanId",
                principalTable: "TaiKhoan",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauThuMua_TaiKhoan_TaiKhoanId",
                table: "YeuCauThuMua");

            migrationBuilder.DropIndex(
                name: "IX_YeuCauThuMua_TaiKhoanId",
                table: "YeuCauThuMua");

            migrationBuilder.DropColumn(
                name: "TaiKhoanId",
                table: "YeuCauThuMua");
        }
    }
}
