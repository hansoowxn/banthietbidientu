using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonHang_DonHangs_MaDon",
                table: "ChiTietDonHang");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_DonHangs_MaDon",
                table: "DanhGia");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHangs_TaiKhoan_TaiKhoanId",
                table: "DonHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonHangs",
                table: "DonHangs");

            migrationBuilder.RenameTable(
                name: "DonHangs",
                newName: "DonHang");

            migrationBuilder.RenameIndex(
                name: "IX_DonHangs_TaiKhoanId",
                table: "DonHang",
                newName: "IX_DonHang_TaiKhoanId");

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "DonHang",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonHang",
                table: "DonHang",
                column: "MaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonHang_DonHang_MaDon",
                table: "ChiTietDonHang",
                column: "MaDon",
                principalTable: "DonHang",
                principalColumn: "MaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_DonHang_MaDon",
                table: "DanhGia",
                column: "MaDon",
                principalTable: "DonHang",
                principalColumn: "MaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHang_TaiKhoan_TaiKhoanId",
                table: "DonHang",
                column: "TaiKhoanId",
                principalTable: "TaiKhoan",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonHang_DonHang_MaDon",
                table: "ChiTietDonHang");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_DonHang_MaDon",
                table: "DanhGia");

            migrationBuilder.DropForeignKey(
                name: "FK_DonHang_TaiKhoan_TaiKhoanId",
                table: "DonHang");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonHang",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "DonHang");

            migrationBuilder.RenameTable(
                name: "DonHang",
                newName: "DonHangs");

            migrationBuilder.RenameIndex(
                name: "IX_DonHang_TaiKhoanId",
                table: "DonHangs",
                newName: "IX_DonHangs_TaiKhoanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonHangs",
                table: "DonHangs",
                column: "MaDon");

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonHang_DonHangs_MaDon",
                table: "ChiTietDonHang",
                column: "MaDon",
                principalTable: "DonHangs",
                principalColumn: "MaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_DonHangs_MaDon",
                table: "DanhGia",
                column: "MaDon",
                principalTable: "DonHangs",
                principalColumn: "MaDon",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonHangs_TaiKhoan_TaiKhoanId",
                table: "DonHangs",
                column: "TaiKhoanId",
                principalTable: "TaiKhoan",
                principalColumn: "Id");
        }
    }
}
