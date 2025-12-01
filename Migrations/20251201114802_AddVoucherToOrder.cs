using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestDoAn.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PhiShip",
                table: "DonHang",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiamGia",
                table: "DonHang",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DonHang",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MaVoucher",
                table: "DonHang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayGiao",
                table: "DonHang",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 3,
                column: "MoTa",
                value: "Mô tả...");

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Đồng hồ thông minh.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "TV 4K sắc nét.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Loa Bluetooth.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Máy chơi game.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Chuột không dây.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Sạc dự phòng.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Máy tính bảng.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Bàn phím.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Camera hành trình.", "Mô tả..." });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 1,
                column: "Address",
                value: "Trụ sở chính");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 2,
                column: "Address",
                value: "Hà Nội");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 3,
                column: "Address",
                value: "Đà Nẵng");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 4,
                column: "Address",
                value: "TP.HCM");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiamGia",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "MaVoucher",
                table: "DonHang");

            migrationBuilder.DropColumn(
                name: "NgayGiao",
                table: "DonHang");

            migrationBuilder.AlterColumn<decimal>(
                name: "PhiShip",
                table: "DonHang",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 3,
                column: "MoTa",
                value: "Trải nghiệm âm thanh đỉnh cao với Wireless Headphones Elite...");

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Đồng hồ thông minh theo dõi sức khỏe toàn diện.", "Smartwatch Series 7 hỗ trợ đo nhịp tim, SpO2..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "TV 4K sắc nét, trải nghiệm điện ảnh tại gia.", "Màn hình 55 inch độ phân giải 4K HDR..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Loa Bluetooth di động âm bass mạnh mẽ.", "Bluetooth Speaker nhỏ gọn, pin trâu..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Máy chơi game thế hệ mới, đồ họa đỉnh cao.", "Gaming Console X hỗ trợ chơi game 4K 120fps..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Chuột không dây tiện lợi, độ nhạy cao.", "Thiết kế Ergonomic giúp cầm nắm thoải mái..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Sạc dự phòng dung lượng lớn, sạc nhanh.", "Dung lượng 10000mAh sạc được nhiều lần..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Máy tính bảng mỏng nhẹ, hiệu năng tốt.", "Màn hình Retina sắc nét, chip xử lý mạnh mẽ..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Bàn phím không dây gõ êm, kết nối ổn định.", "Tương thích nhiều thiết bị, hành trình phím tốt..." });

            migrationBuilder.UpdateData(
                table: "SanPham",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Description", "MoTa" },
                values: new object[] { "Camera hành trình quay 4K chống rung.", "Quay video 4K 60fps, chống nước..." });

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 1,
                column: "Address",
                value: "Trụ sở chính - Sky Tower");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 2,
                column: "Address",
                value: "120 Xuân Thủy, Cầu Giấy, HN");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 3,
                column: "Address",
                value: "78 Bạch Đằng, Hải Châu, ĐN");

            migrationBuilder.UpdateData(
                table: "TaiKhoan",
                keyColumn: "Id",
                keyValue: 4,
                column: "Address",
                value: "55 Nguyễn Huệ, Quận 1, TP.HCM");
        }
    }
}
