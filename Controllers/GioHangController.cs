using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using banthietbidientu.Data;
using banthietbidientu.Services; // Dùng MemberService, VnPayLibrary
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
        private readonly IConfiguration _configuration; // Thêm Config để đọc VNPAY settings

        public GioHangController(ApplicationDbContext context, ILogger<GioHangController> logger, MemberService memberService, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            _memberService = memberService;
            _configuration = configuration;
        }

        // --- 1. HÀM LẤY GIỎ HÀNG ---
        private List<CartItem> GetCart()
        {
            var cart = new List<CartItem>();
            try
            {
                var cartJson = HttpContext.Session.GetString("GioHang");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy Giỏ hàng.");
            }
            return cart;
        }

        // --- 2. HÀM LƯU GIỎ HÀNG ---
        private void SaveCart(List<CartItem> cart)
        {
            try
            {
                HttpContext.Session.SetString("GioHang", JsonConvert.SerializeObject(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu Giỏ hàng.");
            }
        }

        // ... (Giữ nguyên các hàm Index, Add, Remove, GiamSoLuong, ThanhToan) ...
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

        // --- 8. XÁC NHẬN THANH TOÁN (ĐÃ SỬA ĐỂ HỖ TRỢ VNPAY) ---
        [HttpPost]
        public async Task<IActionResult> XacNhanThanhToan(ThanhToanViewModel model, string paymentMethod)
        {
            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("Index");

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            // 1. Tạo dữ liệu đơn hàng (Chưa lưu DB ngay nếu là VNPAY)
            // Lưu tạm thông tin vào Session để dùng lại sau khi thanh toán VNPAY xong
            var tempOrder = new DonHang
            {
                MaDon = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TaiKhoanId = user.Id,
                NgayDat = DateTime.Now,
                TrangThai = 0, // Chờ xử lý
                NguoiNhan = model.NguoiNhan ?? user.FullName,
                SDT = model.SoDienThoai ?? user.PhoneNumber,
                DiaChi = model.DiaChi ?? user.Address,
                PhiShip = 0,
                TongTien = cart.Sum(x => x.Total)
            };

            if (paymentMethod == "VNPAY")
            {
                // --- LOGIC GỬI YÊU CẦU SANG VNPAY ---
                var vnpay = new VnPayLibrary();

                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
                vnpay.AddRequestData("vnp_Amount", ((long)tempOrder.TongTien * 100).ToString()); // Số tiền * 100
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(HttpContext));
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang " + tempOrder.MaDon);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", Url.Action("PaymentCallback", "GioHang", null, Request.Scheme)); // URL Callback
                vnpay.AddRequestData("vnp_TxnRef", tempOrder.MaDon); // Mã đơn hàng

                string paymentUrl = vnpay.CreateRequestUrl(_configuration["VnPay:BaseUrl"], _configuration["VnPay:HashSecret"]);

                // Lưu tạm thông tin đơn hàng vào Session để dùng ở Callback
                HttpContext.Session.SetString("PendingOrder", JsonConvert.SerializeObject(tempOrder));

                return Redirect(paymentUrl);
            }
            else
            {
                // --- LOGIC COD (THANH TOÁN KHI NHẬN HÀNG) ---
                return await ProcessOrderSuccess(tempOrder, cart, "COD");
            }
        }

        // --- 9. CALLBACK TỪ VNPAY ---
        [HttpGet]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _configuration.GetSection("VnPay");
            if (Request.Query.Count > 0)
            {
                string vnp_HashSecret = response["HashSecret"];
                var vnpayData = Request.Query;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (var s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s.Key, s.Value);
                    }
                }

                // Lấy thông tin
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
                string vnp_SecureHash = vnpayData["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00") // Thanh toán thành công
                    {
                        // Lấy lại thông tin đơn hàng từ Session
                        var pendingOrderJson = HttpContext.Session.GetString("PendingOrder");
                        if (!string.IsNullOrEmpty(pendingOrderJson))
                        {
                            var order = JsonConvert.DeserializeObject<DonHang>(pendingOrderJson);
                            var cart = GetCart(); // Lấy lại giỏ hàng (vẫn còn trong session)

                            // Xử lý lưu đơn hàng (Trạng thái đã thanh toán nếu cần, ở đây để 0 chờ xử lý hoặc 1 đã xác nhận)
                            // Nếu muốn đánh dấu đã thanh toán, bạn có thể thêm cột PaymentStatus vào bảng DonHang
                            order.TrangThai = 1; // Đã xác nhận (Vì đã trả tiền rồi)

                            // Gọi hàm xử lý chung
                            await ProcessOrderSuccess(order, cart, "VNPAY");

                            HttpContext.Session.Remove("PendingOrder");
                            return RedirectToAction("LichSuMuaHang", "Login");
                        }
                    }
                    else
                    {
                        // Thanh toán thất bại / Hủy bỏ
                        TempData["Error"] = "Giao dịch VNPAY thất bại hoặc bị hủy. Mã lỗi: " + vnp_ResponseCode;
                        return RedirectToAction("ThanhToan");
                    }
                }
            }
            TempData["Error"] = "Có lỗi xảy ra trong quá trình xử lý VNPAY.";
            return RedirectToAction("Index");
        }

        // --- 10. HÀM XỬ LÝ CHUNG: LƯU ĐƠN HÀNG VÀO DB ---
        private async Task<IActionResult> ProcessOrderSuccess(DonHang order, List<CartItem> cart, string method)
        {
            // A. Lưu Đơn Hàng Header
            _context.DonHangs.Add(order);

            // Tạo thông báo
            var thongBao = new ThongBao
            {
                TieuDe = "Đơn hàng mới #" + order.MaDon,
                NoiDung = $"Khách {order.NguoiNhan} đặt đơn {method} trị giá {order.TongTien:N0}đ",
                NgayTao = DateTime.Now,
                DaDoc = false,
                LoaiThongBao = 0
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            // B. Lưu Chi Tiết & Trừ Kho
            foreach (var item in cart)
            {
                var chiTiet = new ChiTietDonHang
                {
                    MaDon = order.MaDon,
                    SanPhamId = item.Id,
                    SoLuong = item.Quantity,
                    Gia = item.Price
                };
                _context.ChiTietDonHangs.Add(chiTiet);

                var sanPham = await _context.SanPhams.FindAsync(item.Id);
                if (sanPham != null)
                {
                    sanPham.SoLuong -= item.Quantity;
                }
            }
            await _context.SaveChangesAsync();

            // C. Dọn dẹp
            HttpContext.Session.Remove("GioHang");
            HttpContext.Session.Remove("Cart");

            TempData["Success"] = $"Đặt hàng thành công ({method})! Mã đơn: {order.MaDon}";
            return RedirectToAction("LichSuMuaHang", "Login");
        }
    }

    // Helper để lấy IP (Cần cho VNPAY)
    public static class Utils
    {
        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    }
                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();
                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return "Invalid IP:" + ex.Message;
            }
            return "127.0.0.1";
        }
    }
}