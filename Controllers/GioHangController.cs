using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using banthietbidientu.Data;
using banthietbidientu.Services; // Dùng MemberService
using Microsoft.Data.SqlClient;
using banthietbidientu.Models;   // Dùng CartItem, DonHang...
using banthietbidientu.Helpers;  // Dùng Session Extensions
using Microsoft.AspNetCore.Http;

namespace banthietbidientu.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GioHangController> _logger;
        private readonly MemberService _memberService;
        // Đã xóa IEmailSender để tránh lỗi crash

        public GioHangController(ApplicationDbContext context, ILogger<GioHangController> logger, MemberService memberService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            _memberService = memberService;
        }

        // --- 1. HÀM LẤY GIỎ HÀNG (Đã sửa về List<CartItem> để khớp View) ---
        private List<CartItem> GetCart()
        {
            var cart = new List<CartItem>();

            try
            {
                // Ưu tiên lấy từ Session trước
                var cartJson = HttpContext.Session.GetString("GioHang");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
                }

                // Nếu User đã đăng nhập, thử đồng bộ từ DB (nếu bạn muốn dùng tính năng lưu DB)
                if (User.Identity?.IsAuthenticated == true)
                {
                    // Logic lấy từ DB ở đây nếu cần, hiện tại dùng Session cho ổn định
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy Giỏ hàng.");
            }

            return cart;
        }

        // --- 2. HÀM LƯU GIỎ HÀNG (Lưu vào Session) ---
        private void SaveCart(List<CartItem> cart)
        {
            try
            {
                // Luôn lưu vào Session
                HttpContext.Session.SetString("GioHang", JsonConvert.SerializeObject(cart));

                // Nếu muốn lưu vào DB cho User, viết logic mapping ở đây
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu Giỏ hàng.");
            }
        }

        // --- 3. TRANG GIỎ HÀNG ---
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // --- 4. THÊM VÀO GIỎ ---
        [HttpPost]
        public IActionResult Add(int id, int quantity = 1, string? capacity = null, string? color = null)
        {
            var product = _context.SanPhams.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            // Xử lý tên biến thể (Màu/Dung lượng)
            decimal finalPrice = product.Price;
            string variantName = product.Name;

            if (!string.IsNullOrEmpty(capacity))
            {
                if (capacity == "128GB") finalPrice += 500000;
                else if (capacity == "256GB") finalPrice += 1500000;
                variantName += $" ({capacity}";
            }

            if (!string.IsNullOrEmpty(color))
            {
                variantName += string.IsNullOrEmpty(capacity) ? $" ({color}" : $" - {color}";
            }

            if (!string.IsNullOrEmpty(capacity) || !string.IsNullOrEmpty(color))
            {
                variantName += ")";
            }

            var cart = GetCart();
            // Tìm xem sản phẩm đã có trong giỏ chưa (so sánh ID và Tên biến thể)
            var item = cart.FirstOrDefault(x => x.Id == id && x.Name == variantName);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Id = product.Id,
                    Name = variantName,
                    Image = product.ImageUrl,
                    Price = finalPrice,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
            TempData["Success"] = $"Đã thêm: {variantName}";

            // Quay lại trang trước đó hoặc trang chủ
            string referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index", "Home");
        }

        // Action hỗ trợ nút thêm nhanh (GET)
        public async Task<IActionResult> ThemVaoGio(int id, int quantity = 1)
        {
            return Add(id, quantity);
        }

        // --- 5. XÓA KHỎI GIỎ ---
        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // Action Alias cho nút xóa
        public IActionResult XoaKhoiGio(int id) => Remove(id);

        // --- 6. CẬP NHẬT SỐ LƯỢNG ---
        public IActionResult GiamSoLuong(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                if (item.Quantity > 1) item.Quantity--;
                else cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        // --- 7. TRANG THANH TOÁN ---
        [HttpGet]
        public IActionResult ThanhToan()
        {
            if (User.Identity?.IsAuthenticated != true) return RedirectToAction("DangNhap", "Login");

            var cart = GetCart();
            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống, không thể thanh toán.";
                return RedirectToAction("Index");
            }

            var username = User.Identity.Name;
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == username);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            // Lấy hạng thành viên
            var tier = _memberService.GetUserTier(user.Username);
            ViewBag.Tier = tier;

            var model = new ThanhToanViewModel
            {
                GioHangs = cart,
                TaiKhoan = user,
                TongTien = cart.Sum(x => x.Total),
                NguoiNhan = user.FullName,
                SoDienThoai = user.PhoneNumber,
                DiaChi = user.Address
            };

            return View(model);
        }

        // --- 8. XÁC NHẬN THANH TOÁN ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(ThanhToanViewModel model)
        {
            // 1. Lấy giỏ hàng
            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("Index");

            // 2. Lấy thông tin khách hàng
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            // 3. Tạo mã đơn hàng
            string maDon = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // --- A. TẠO ĐƠN HÀNG (HEADER) ---
            var donHang = new DonHang
            {
                MaDon = maDon,
                TaiKhoanId = user.Id,
                NgayDat = DateTime.Now,
                TrangThai = 0, // Chờ xử lý
                NguoiNhan = model.NguoiNhan ?? user.FullName,
                SDT = model.SoDienThoai ?? user.PhoneNumber,
                DiaChi = model.DiaChi ?? user.Address,
                PhiShip = 0,
                TongTien = cart.Sum(x => x.Total)
            };
            _context.DonHangs.Add(donHang);

            // Tạo thông báo cho Admin
            var thongBao = new ThongBao
            {
                TieuDe = "Đơn hàng mới",
                NoiDung = $"Khách {user.FullName} vừa đặt đơn hàng #{maDon} trị giá {donHang.TongTien:N0}đ",
                NgayTao = DateTime.Now,
                DaDoc = false,
                LoaiThongBao = 0
            };
            _context.ThongBaos.Add(thongBao);

            // Lưu đợt 1 để tạo Đơn hàng trước
            await _context.SaveChangesAsync();

            // --- B. LƯU CHI TIẾT SẢN PHẨM (CHI TIẾT) ---
            // [LỖI CŨ NẰM Ở ĐÂY: BẠN ĐÃ RETURN TRƯỚC KHI CHẠY VÒNG LẶP NÀY]
            foreach (var item in cart)
            {
                var chiTiet = new ChiTietDonHang
                {
                    MaDon = maDon,
                    SanPhamId = item.Id,
                    SoLuong = item.Quantity,
                    Gia = item.Price
                };
                _context.ChiTietDonHangs.Add(chiTiet);

                // Trừ tồn kho
                var sanPham = await _context.SanPhams.FindAsync(item.Id);
                if (sanPham != null)
                {
                    sanPham.SoLuong -= item.Quantity;
                }
            }

            // Lưu đợt 2: Lưu các chi tiết đơn hàng và cập nhật kho
            await _context.SaveChangesAsync();

            // --- C. DỌN DẸP VÀ CHUYỂN HƯỚNG ---
            HttpContext.Session.Remove("GioHang"); // Xóa giỏ hàng sau khi mua xong
            TempData["Success"] = "Đặt hàng thành công! Mã đơn: " + maDon;

            return RedirectToAction("LichSuMuaHang", "Login");
        }

        // Hàm tạo nội dung email (Giữ lại nhưng chưa dùng)
        private string GetEmailContent(TaiKhoan khach, List<CartItem> gioHangs, decimal total, string orderId)
        {
            string productsHtml = "";
            foreach (var item in gioHangs)
            {
                productsHtml += $@"
                <tr>
                    <td style='padding:8px;border-bottom:1px solid #ddd;'>{item.Name}</td>
                    <td style='padding:8px;border-bottom:1px solid #ddd;text-align:center;'>{item.Quantity}</td>
                    <td style='padding:8px;border-bottom:1px solid #ddd;text-align:right;'>{item.Price:N0} ₫</td>
                </tr>";
            }
            return productsHtml; // Rút gọn
        }
    }
}