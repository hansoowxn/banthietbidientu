using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class UpdateThongBao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_TaiKhoans_UserId",
                table: "GioHangs");

            migrationBuilder.DropIndex(
                name: "IX_GioHangs_UserId",
                table: "GioHangs");

            migrationBuilder.AddColumn<string>(
                name: "TieuDe",
                table: "ThongBao",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TaiKhoanId",
                table: "GioHangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_TaiKhoanId",
                table: "GioHangs",
                column: "TaiKhoanId");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_TaiKhoans_TaiKhoanId",
                table: "GioHangs",
                column: "TaiKhoanId",
                principalTable: "TaiKhoans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_TaiKhoans_TaiKhoanId",
                table: "GioHangs");

            migrationBuilder.DropIndex(
                name: "IX_GioHangs_TaiKhoanId",
                table: "GioHangs");

            migrationBuilder.DropColumn(
                name: "TieuDe",
                table: "ThongBao");

            migrationBuilder.DropColumn(
                name: "TaiKhoanId",
                table: "GioHangs");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangs_UserId",
                table: "GioHangs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_TaiKhoans_UserId",
                table: "GioHangs",
                column: "UserId",
                principalTable: "TaiKhoans",
                principalColumn: "Id");
        }
    }
}
