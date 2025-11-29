using System.Collections.Generic;
using banthietbidientu.Models; // Đảm bảo có dòng này để nhận diện TaiKhoan

namespace banthietbidientu.Models
{
    public class ThanhToanViewModel
    {
        // Danh sách hàng trong giỏ để hiển thị lại lúc thanh toán
        public List<CartItem> GioHangs { get; set; }

        // Thông tin khách hàng (để điền sẵn vào form)
        public TaiKhoan TaiKhoan { get; set; }

        public decimal TongTien { get; set; }

        // Các trường nhận dữ liệu từ Form
        public string DiaChi { get; set; }
        public string NguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string GhiChu { get; set; }

        // --- [QUAN TRỌNG] Các trường mới bắt buộc phải có ---
        public int? StoreId { get; set; }
        public string DeliveryType { get; set; }
        public string TinhThanh { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}