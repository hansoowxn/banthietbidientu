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

        // --- 6. CẬP NHẬT SỐ LƯỢNG (GiamSoLuong - Logic cũ) ---
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

        // --- 11. CẬP NHẬT SỐ LƯỢNG (UPDATE - MỚI THÊM ĐỂ FIX LỖI 404) ---
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                // Kiểm tra tồn kho (Optional)
                var product = _context.SanPhams.Find(id);
                if (product != null)
                {
                    // Nếu số lượng mua lớn hơn tồn kho
                    if (quantity > product.SoLuong)
                    {
                        TempData["Error"] = $"Kho chỉ còn {product.SoLuong} sản phẩm!";
                        item.Quantity = product.SoLuong;
                    }
                    else if (quantity > 0)
                    {
                        item.Quantity = quantity;
                    }
                    else
                    {
                        cart.Remove(item);
                    }
                }
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
        public async Task<IActionResult> XacNhanThanhToan(ThanhToanViewModel model, string paymentMethod)
        {
            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("Index");

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            var tempOrder = new DonHang
            {
                MaDon = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TaiKhoanId = user.Id,
                NgayDat = DateTime.Now,
                TrangThai = 0, // Chờ xử lý

                // Sửa lỗi: Thêm ?? string.Empty để ngăn chặn giá trị NULL
                NguoiNhan = model.NguoiNhan ?? user.FullName ?? string.Empty,
                SDT = model.SoDienThoai ?? user.PhoneNumber ?? string.Empty, // <-- Đã sửa lỗi ở đây
                DiaChi = model.DiaChi ?? user.Address ?? string.Empty,

                PhiShip = 0,
                TongTien = cart.Sum(x => x.Total)
            };

            if (paymentMethod == "VNPAY")
            {
                var vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
                vnpay.AddRequestData("vnp_Amount", ((long)tempOrder.TongTien * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(HttpContext));
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang " + tempOrder.MaDon);
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", Url.Action("PaymentCallback", "GioHang", null, Request.Scheme));
                vnpay.AddRequestData("vnp_TxnRef", tempOrder.MaDon);

                string paymentUrl = vnpay.CreateRequestUrl(_configuration["VnPay:BaseUrl"], _configuration["VnPay:HashSecret"]);

                HttpContext.Session.SetString("PendingOrder", JsonConvert.SerializeObject(tempOrder));
                return Redirect(paymentUrl);
            }
            else
            {
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

                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_SecureHash = vnpayData["vnp_SecureHash"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        var pendingOrderJson = HttpContext.Session.GetString("PendingOrder");
                        if (!string.IsNullOrEmpty(pendingOrderJson))
                        {
                            var order = JsonConvert.DeserializeObject<DonHang>(pendingOrderJson);
                            var cart = GetCart();
                            order.TrangThai = 1; // Đã xác nhận (đã thanh toán)

                            await ProcessOrderSuccess(order, cart, "VNPAY");
                            HttpContext.Session.Remove("PendingOrder");
                            return RedirectToAction("LichSuMuaHang", "Login");
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Giao dịch VNPAY thất bại. Mã lỗi: " + vnp_ResponseCode;
                        return RedirectToAction("ThanhToan");
                    }
                }
            }
            TempData["Error"] = "Lỗi xử lý VNPAY.";
            return RedirectToAction("Index");
        }

        // --- 10. HÀM XỬ LÝ CHUNG ---
        private async Task<IActionResult> ProcessOrderSuccess(DonHang order, List<CartItem> cart, string method)
        {
            _context.DonHangs.Add(order);

            // CẬP NHẬT: Thêm RedirectId và RedirectAction
            var thongBao = new ThongBao
            {
                TieuDe = "Đơn hàng mới #" + order.MaDon,
                NoiDung = $"Khách {order.NguoiNhan} đặt đơn {method} trị giá {order.TongTien:N0}đ",
                NgayTao = DateTime.Now,
                DaDoc = false,
                LoaiThongBao = 0, // 0 = Đơn hàng
                RedirectId = order.MaDon,           // Mã đơn hàng
                RedirectAction = "QuanLyDonHang"    // Dẫn về trang quản lý đơn
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            // ... (Phần còn lại giữ nguyên) ...
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

            HttpContext.Session.Remove("GioHang");
            HttpContext.Session.Remove("Cart");

            TempData["Success"] = $"Đặt hàng thành công ({method})! Mã đơn: {order.MaDon}";
            return RedirectToAction("LichSuMuaHang", "Login");
        }
    }
}
