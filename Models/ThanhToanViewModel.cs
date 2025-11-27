using System.Collections.Generic;

namespace banthietbidientu.Models
{
    public class ThanhToanViewModel
    {
        // Danh sách hàng trong giỏ để hiển thị lại lúc thanh toán
        public List<CartItem> GioHangs { get; set; }

        // Thông tin khách hàng (để điền sẵn vào form)
        public TaiKhoan TaiKhoan { get; set; }

        public decimal TongTien { get; set; }

        // --- QUAN TRỌNG: Thuộc tính này để hứng địa chỉ mới nhập từ Form ---
        public string DiaChi { get; set; }
        public string NguoiNhan { get; set; }
        public string SoDienThoai { get; set; }
        public string GhiChu { get; set; }
    }
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity; // Thành tiền = Giá * Số lượng
    }
}