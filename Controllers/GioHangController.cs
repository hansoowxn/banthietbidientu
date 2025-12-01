using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using banthietbidientu.Data;
using banthietbidientu.Services;
using Microsoft.Data.SqlClient;
using banthietbidientu.Models;
using banthietbidientu.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace banthietbidientu.Controllers
{
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GioHangController> _logger;
        private readonly MemberService _memberService;
        private readonly IConfiguration _configuration;

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

            string referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index", "Home");
        }

        public IActionResult ThemVaoGio(int id, int quantity = 1)
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

        public IActionResult XoaKhoiGio(int id) => Remove(id);

        // --- 6. CẬP NHẬT SỐ LƯỢNG ---
        public IActionResult Update(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                var product = _context.SanPhams.Find(id);
                if (product != null)
                {
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

        // --- 8. XÁC NHẬN THANH TOÁN (COD & VNPAY) ---
        [HttpPost]
        public async Task<IActionResult> XacNhanThanhToan(ThanhToanViewModel model, string paymentMethod)
        {
            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("Index");

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            // --- 1. TÍNH TOÁN GIẢM GIÁ ---
            decimal tongTienHang = cart.Sum(x => x.Total);
            var tier = _memberService.GetUserTier(user.Username);
            decimal giamGia = (tongTienHang * tier.DiscountPercent) / 100;
            decimal tongTienSauGiam = tongTienHang - giamGia;

            // --- 2. XÁC ĐỊNH STORE ID & ĐỊA CHỈ ---
            int? storeId = null;

            if (model.DeliveryType == "Store" && model.StoreId.HasValue)
            {
                storeId = model.StoreId.Value;
            }
            else
            {
                // 1. Ưu tiên lấy Tỉnh từ Dropdown
                string tinh = (model.TinhThanh ?? "").Trim();

                // 2. Nếu Dropdown rỗng (do dùng địa chỉ cũ), lấy từ chuỗi Địa chỉ đầy đủ
                string diaChiFull = (model.DiaChi ?? "").ToLower();

                string[] mienBac = { "hà nội", "hải phòng", "quảng ninh", "bắc ninh", "hải dương", "hưng yên", "nam định", "thái bình", "vĩnh phúc", "phú thọ", "bắc giang", "thái nguyên", "cao bằng", "bắc kạn", "lạng sơn", "tuyên quang", "hà giang", "yên bái", "lào cai", "điện biên", "lai châu", "sơn la", "hòa bình", "hà nam", "ninh bình" };
                string[] mienTrung = { "đà nẵng", "huế", "thừa thiên huế", "quảng nam", "quảng ngãi", "bình định", "phú yên", "khánh hòa", "quảng bình", "quảng trị", "nghệ an", "hà tĩnh", "thanh hóa", "ninh thuận", "bình thuận", "kon tum", "gia lai", "đắk lắk", "đắk nông", "lâm đồng" };

                // Logic kiểm tra kết hợp
                bool isMienTrung = mienTrung.Any(x => tinh.Contains(x, StringComparison.OrdinalIgnoreCase) || diaChiFull.Contains(x));
                bool isMienBac = mienBac.Any(x => tinh.Contains(x, StringComparison.OrdinalIgnoreCase) || diaChiFull.Contains(x));

                if (isMienTrung)
                {
                    storeId = 2; // Kho Đà Nẵng
                }
                else if (isMienBac)
                {
                    storeId = 1; // Kho Hà Nội
                }
                else
                {
                    storeId = 3; // Mặc định còn lại về Kho HCM
                }

                // Gộp địa chỉ nếu chưa có Tỉnh trong chuỗi (chỉ áp dụng khi chọn mới)
                if (!string.IsNullOrEmpty(model.TinhThanh) && !model.DiaChi.Contains(model.TinhThanh))
                {
                    model.DiaChi = $"{model.DiaChi}, {model.TinhThanh}";
                }
            }
            // -------------------------------------

            var tempOrder = new DonHang
            {
                MaDon = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TaiKhoanId = user.Id,
                NgayDat = DateTime.Now,
                TrangThai = 0,
                NguoiNhan = model.NguoiNhan ?? user.FullName ?? string.Empty,
                SDT = model.SoDienThoai ?? user.PhoneNumber ?? string.Empty,
                DiaChi = model.DiaChi ?? user.Address ?? string.Empty,
                PhiShip = 0, // Mặc định FreeShip
                TongTien = tongTienSauGiam, // Lưu giá đã giảm
                StoreId = storeId // Lưu StoreId
            };

            // A. THANH TOÁN VNPAY
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
            // B. THANH TOÁN COD
            else
            {
                await ProcessOrderSuccess(tempOrder, cart, "COD");
                return View("PaymentSuccess");
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
                            order.TrangThai = 1;

                            await ProcessOrderSuccess(order, cart, "VNPAY");
                            HttpContext.Session.Remove("PendingOrder");

                            return View("PaymentSuccess");
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Giao dịch VNPAY thất bại. Mã lỗi: " + vnp_ResponseCode;
                        return RedirectToAction("ThanhToan");
                    }
                }
            }
            TempData["Error"] = "Lỗi xử lý VNPAY (Sai chữ ký).";
            return RedirectToAction("Index");
        }

        // --- 10. HÀM XỬ LÝ CHUNG ---
        private async Task<IActionResult> ProcessOrderSuccess(DonHang order, List<CartItem> cart, string method)
        {
            _context.DonHangs.Add(order);

            var thongBao = new ThongBao
            {
                TieuDe = "Đơn hàng mới #" + order.MaDon,
                NoiDung = $"Khách {order.NguoiNhan} đặt đơn {method} trị giá {order.TongTien:N0}đ",
                NgayTao = DateTime.Now,
                DaDoc = false,
                LoaiThongBao = 0,
                RedirectId = order.MaDon,
                RedirectAction = "QuanLyDonHang",

                // [MỚI] Gán StoreId từ đơn hàng sang thông báo
                StoreId = order.StoreId
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                var sanPham = await _context.SanPhams.FindAsync(item.Id);

                var chiTiet = new ChiTietDonHang
                {
                    MaDon = order.MaDon,
                    SanPhamId = item.Id,
                    SoLuong = item.Quantity,
                    Gia = item.Price,
                    GiaGoc = sanPham != null ? sanPham.GiaNhap : 0
                };
                _context.ChiTietDonHangs.Add(chiTiet);

                if (sanPham != null)
                {
                    sanPham.SoLuong -= item.Quantity;
                }
            }
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("GioHang");
            HttpContext.Session.Remove("Cart");

            return Ok();
        }
    }
}