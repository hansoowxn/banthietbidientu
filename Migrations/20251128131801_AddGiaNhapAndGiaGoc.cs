using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class AddGiaNhapAndGiaGoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonHang_SanPhams_SanPhamId",
                table: "ChiTietDonHang");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuNhaps_SanPhams_SanPhamId",
                table: "ChiTietPhieuNhaps");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_SanPhams_SanPhamId",
                table: "DanhGia");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_SanPhams_ProductId",
                table: "GioHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuMuaHangs_SanPhams_ProductId",
                table: "LichSuMuaHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuNhapHangs_SanPhams_SanPhamId",
                table: "LichSuNhapHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SanPhams",
                table: "SanPhams");

            migrationBuilder.RenameTable(
                name: "SanPhams",
                newName: "SanPham");

            migrationBuilder.AddColumn<decimal>(
                name: "GiaGoc",
                table: "ChiTietDonHang",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SanPham",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SanPham",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SanPham",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SanPham",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GiaNhap",
                table: "SanPham",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SanPham",
                table: "SanPham",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Điện thoại", "Điện thoại thông minh cao cấp với màn hình tràn viền.", 500.00m, "Smartphone Galaxy X sở hữu thiết kế đột phá..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Laptop", "Laptop hiệu năng cao cho công việc chuyên nghiệp.", 1000.00m, "Laptop Pro 15 được trang bị chip xử lý mới nhất..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Phụ kiện", "Tai nghe không dây chống ồn chủ động.", 150.00m, "Trải nghiệm âm thanh đỉnh cao với Wireless Headphones Elite..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Đồng hồ", "Đồng hồ thông minh theo dõi sức khỏe toàn diện.", 300.00m, "Smartwatch Series 7 hỗ trợ đo nhịp tim, SpO2..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "TV", "TV 4K sắc nét, trải nghiệm điện ảnh tại gia.", 600.00m, "Màn hình 55 inch độ phân giải 4K HDR..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Phụ kiện", "Loa Bluetooth di động âm bass mạnh mẽ.", 100.00m, "Bluetooth Speaker nhỏ gọn, pin trâu..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Console", "Máy chơi game thế hệ mới, đồ họa đỉnh cao.", 400.00m, "Gaming Console X hỗ trợ chơi game 4K 120fps..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Phụ kiện", "Chuột không dây tiện lợi, độ nhạy cao.", 30.00m, "Thiết kế Ergonomic giúp cầm nắm thoải mái..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Phụ kiện", "Sạc dự phòng dung lượng lớn, sạc nhanh.", 20.00m, "Dung lượng 10000mAh sạc được nhiều lần..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Tablet", "Máy tính bảng mỏng nhẹ, hiệu năng tốt.", 350.00m, "Màn hình Retina sắc nét, chip xử lý mạnh mẽ..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Phụ kiện", "Bàn phím không dây gõ êm, kết nối ổn định.", 50.00m, "Tương thích nhiều thiết bị, hành trình phím tốt..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "Description", "GiaNhap", "MoTa" },
                values: new object[] { "Camera", "Camera hành trình quay 4K chống rung.", 200.00m, "Quay video 4K 60fps, chống nước..." });

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonHang_SanPham_SanPhamId",
                table: "ChiTietDonHang",
                column: "SanPhamId",
                principalTable: "SanPham",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuNhaps_SanPham_SanPhamId",
                table: "ChiTietPhieuNhaps",
                column: "SanPhamId",
                principalTable: "SanPham",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_SanPham_SanPhamId",
                table: "DanhGia",
                column: "SanPhamId",
                principalTable: "SanPham",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_SanPham_ProductId",
                table: "GioHangs",
                column: "ProductId",
                principalTable: "SanPham",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuMuaHangs_SanPham_ProductId",
                table: "LichSuMuaHangs",
                column: "ProductId",
                principalTable: "SanPham",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuNhapHangs_SanPham_SanPhamId",
                table: "LichSuNhapHangs",
                column: "SanPhamId",
                principalTable: "SanPham",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietDonHang_SanPham_SanPhamId",
                table: "ChiTietDonHang");

            migrationBuilder.DropForeignKey(
                name: "FK_ChiTietPhieuNhaps_SanPham_SanPhamId",
                table: "ChiTietPhieuNhaps");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_SanPham_SanPhamId",
                table: "DanhGia");

            migrationBuilder.DropForeignKey(
                name: "FK_GioHangs_SanPham_ProductId",
                table: "GioHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuMuaHangs_SanPham_ProductId",
                table: "LichSuMuaHangs");

            migrationBuilder.DropForeignKey(
                name: "FK_LichSuNhapHangs_SanPham_SanPhamId",
                table: "LichSuNhapHangs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SanPham",
                table: "SanPham");

            migrationBuilder.DropColumn(
                name: "GiaGoc",
                table: "ChiTietDonHang");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SanPham");

            migrationBuilder.DropColumn(
                name: "GiaNhap",
                table: "SanPham");

            migrationBuilder.RenameTable(
                name: "SanPham",
                newName: "SanPhams");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "SanPhams",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SanPhams",
                table: "SanPhams",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "SanPhams",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Category", "MoTa" },
                values: new object[] { null, null });

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietDonHang_SanPhams_SanPhamId",
                table: "ChiTietDonHang",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChiTietPhieuNhaps_SanPhams_SanPhamId",
                table: "ChiTietPhieuNhaps",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_SanPhams_SanPhamId",
                table: "DanhGia",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GioHangs_SanPhams_ProductId",
                table: "GioHangs",
                column: "ProductId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuMuaHangs_SanPhams_ProductId",
                table: "LichSuMuaHangs",
                column: "ProductId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuNhapHangs_SanPhams_SanPhamId",
                table: "LichSuNhapHangs",
                column: "SanPhamId",
                principalTable: "SanPhams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
