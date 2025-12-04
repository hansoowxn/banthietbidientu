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
        private readonly IEmailSender _emailSender; // [MỚI]

        // [CẬP NHẬT] Thêm IEmailSender vào Constructor
        public GioHangController(ApplicationDbContext context, ILogger<GioHangController> logger, MemberService memberService, IConfiguration configuration, IEmailSender emailSender)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            _memberService = memberService;
            _configuration = configuration;
            _emailSender = emailSender;
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

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

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

        public IActionResult ThemVaoGio(int id, int quantity = 1) => Add(id, quantity);

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

            string maVoucher = HttpContext.Session.GetString("VoucherCode");
            string strVoucherGiam = HttpContext.Session.GetString("VoucherGiam");
            decimal giamGiaVoucher = 0;

            if (!string.IsNullOrEmpty(strVoucherGiam))
            {
                decimal.TryParse(strVoucherGiam, out giamGiaVoucher);
            }

            if (tier.DiscountPercent > 0)
            {
                giamGiaVoucher = 0;
                maVoucher = null;
            }

            decimal tongTienHang = cart.Sum(x => x.Total);
            decimal giamGiaHang = (tongTienHang * tier.DiscountPercent) / 100;
            decimal tongGiam = giamGiaHang + giamGiaVoucher;

            decimal tienTruocThue = tongTienHang - tongGiam;
            if (tienTruocThue < 0) tienTruocThue = 0;

            decimal thueVAT = tienTruocThue * 0.10m;

            ViewBag.MaVoucher = maVoucher;
            ViewBag.GiamGiaVoucher = giamGiaVoucher;
            ViewBag.ThueVAT = thueVAT;

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

        [HttpGet]
        public IActionResult GetVoucherList()
        {
            var today = DateTime.Now;
            var vouchers = _context.Vouchers
                .Where(v => v.IsActive && v.NgayBatDau <= today && v.NgayKetThuc >= today && v.DaDung < v.SoLuong)
                .OrderBy(v => v.DonToiThieu)
                .Select(v => new { v.MaVoucher, v.TenVoucher, v.LoaiGiamGia, v.GiaTri, v.GiamToiDa, v.DonToiThieu, HanSuDung = v.NgayKetThuc.ToString("dd/MM/yyyy") })
                .ToList();
            return Json(new { success = true, data = vouchers });
        }

        [HttpPost]
        public IActionResult ApDungVoucher(string maVoucher)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để dùng mã!" });
            }

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            var tier = _memberService.GetUserTier(user.Username);

            if (tier.Name.Contains("KIM CƯƠNG") || tier.Name.Contains("BẠCH KIM"))
            {
                return Json(new { success = false, message = $"Hạng {tier.Name} đã có ưu đãi riêng, không thể dùng thêm Voucher!" });
            }

            var voucher = _context.Vouchers.FirstOrDefault(v => v.MaVoucher == maVoucher && v.IsActive);

            if (voucher == null) return Json(new { success = false, message = "Mã không tồn tại hoặc đã bị khóa!" });

            if (DateTime.Now < voucher.NgayBatDau || DateTime.Now > voucher.NgayKetThuc)
                return Json(new { success = false, message = "Mã này chưa bắt đầu hoặc đã hết hạn!" });

            if (voucher.DaDung >= voucher.SoLuong)
                return Json(new { success = false, message = "Mã này đã hết lượt sử dụng!" });

            var cart = GetCart();
            decimal tongTienHang = cart.Sum(x => x.Total);

            if (tongTienHang < voucher.DonToiThieu)
                return Json(new { success = false, message = $"Đơn hàng phải từ {voucher.DonToiThieu:N0}đ mới được dùng mã này!" });

            decimal soTienGiam = 0;
            if (voucher.LoaiGiamGia == 0) soTienGiam = voucher.GiaTri;
            else
            {
                soTienGiam = (tongTienHang * voucher.GiaTri) / 100;
                if (voucher.GiamToiDa > 0 && soTienGiam > voucher.GiamToiDa) soTienGiam = voucher.GiamToiDa;
            }

            if (soTienGiam > tongTienHang) soTienGiam = tongTienHang;

            HttpContext.Session.SetString("VoucherCode", voucher.MaVoucher);
            HttpContext.Session.SetString("VoucherGiam", soTienGiam.ToString());

            return Json(new { success = true, discount = soTienGiam, message = "Áp dụng mã thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanThanhToan(ThanhToanViewModel model, string paymentMethod)
        {
            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("Index");

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            decimal tongTienHang = cart.Sum(x => x.Total);
            var tier = _memberService.GetUserTier(user.Username);
            decimal giamGiaHang = (tongTienHang * tier.DiscountPercent) / 100;

            decimal giamGiaVoucher = 0;
            string maVoucher = HttpContext.Session.GetString("VoucherCode");
            string strVoucherGiam = HttpContext.Session.GetString("VoucherGiam");
            if (!string.IsNullOrEmpty(maVoucher) && !string.IsNullOrEmpty(strVoucherGiam))
            {
                decimal.TryParse(strVoucherGiam, out giamGiaVoucher);
            }

            if (tier.DiscountPercent > 0)
            {
                giamGiaVoucher = 0;
                maVoucher = null;
            }

            decimal tongGiamGia = giamGiaHang + giamGiaVoucher;
            decimal tienTruocThue = tongTienHang - tongGiamGia;
            if (tienTruocThue < 0) tienTruocThue = 0;

            decimal thueVAT = tienTruocThue * 0.10m;
            decimal tongTienSauThue = tienTruocThue + thueVAT;

            int? storeId = null;
            if (model.DeliveryType == "Store" && model.StoreId.HasValue)
            {
                storeId = model.StoreId.Value;
            }
            else
            {
                string tinh = (model.TinhThanh ?? "").Trim();
                string diaChiFull = (model.DiaChi ?? "").ToLower();
                string[] mienBac = { "hà nội", "hải phòng", "quảng ninh", "bắc ninh", "hải dương", "hưng yên", "nam định", "thái bình", "vĩnh phúc", "phú thọ", "bắc giang", "thái nguyên", "cao bằng", "bắc kạn", "lạng sơn", "tuyên quang", "hà giang", "yên bái", "lào cai", "điện biên", "lai châu", "sơn la", "hòa bình", "hà nam", "ninh bình" };
                string[] mienTrung = { "đà nẵng", "huế", "thừa thiên huế", "quảng nam", "quảng ngãi", "bình định", "phú yên", "khánh hòa", "quảng bình", "quảng trị", "nghệ an", "hà tĩnh", "thanh hóa", "ninh thuận", "bình thuận", "kon tum", "gia lai", "đắk lắk", "đắk nông", "lâm đồng" };

                bool isMienTrung = false;
                bool isMienBac = false;

                if (!string.IsNullOrEmpty(tinh))
                {
                    isMienTrung = mienTrung.Any(x => tinh.Contains(x, StringComparison.OrdinalIgnoreCase));
                    isMienBac = mienBac.Any(x => tinh.Contains(x, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    isMienTrung = mienTrung.Any(x => diaChiFull.Contains(x));
                    isMienBac = mienBac.Any(x => diaChiFull.Contains(x));
                }

                if (isMienTrung) storeId = 2;
                else if (isMienBac) storeId = 1;
                else storeId = 3;

                if (!string.IsNullOrEmpty(model.TinhThanh) && !model.DiaChi.Contains(model.TinhThanh))
                {
                    model.DiaChi = $"{model.DiaChi}, {model.TinhThanh}";
                }
            }

            var tempOrder = new DonHang
            {
                MaDon = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TaiKhoanId = user.Id,
                NgayDat = DateTime.Now,
                TrangThai = 0,
                NguoiNhan = model.NguoiNhan ?? user.FullName ?? string.Empty,
                SDT = model.SoDienThoai ?? user.PhoneNumber ?? string.Empty,
                DiaChi = model.DiaChi ?? user.Address ?? string.Empty,
                PhiShip = 0,
                TienThue = thueVAT,
                TongTien = tongTienSauThue,
                StoreId = storeId,
                MaVoucher = maVoucher,
                GiamGia = tongGiamGia
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
                await ProcessOrderSuccess(tempOrder, cart, "COD");
                return View("PaymentSuccess");
            }
        }

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

        // --- 11. HÀM XỬ LÝ CHUNG (ĐÃ THÊM GỬI EMAIL) ---
        private async Task<IActionResult> ProcessOrderSuccess(DonHang order, List<CartItem> cart, string method)
        {
            _context.DonHangs.Add(order);

            if (!string.IsNullOrEmpty(order.MaVoucher))
            {
                var v = _context.Vouchers.FirstOrDefault(x => x.MaVoucher == order.MaVoucher);
                if (v != null) { v.DaDung++; }
            }

            var thongBao = new ThongBao
            {
                TieuDe = "Đơn hàng mới #" + order.MaDon,
                NoiDung = $"Khách {order.NguoiNhan} đặt đơn {method} trị giá {order.TongTien:N0}đ",
                NgayTao = DateTime.Now,
                DaDoc = false,
                LoaiThongBao = 0,
                RedirectId = order.MaDon,
                RedirectAction = "QuanLyDonHang",
                StoreId = order.StoreId
            };
            _context.ThongBaos.Add(thongBao);

            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                // 1. Tìm sản phẩm
                var sanPham = await _context.SanPhams.FindAsync(item.Id);

                if (sanPham != null)
                {
                    // A. Trừ tổng số lượng (để hiển thị nhanh)
                    sanPham.SoLuong -= item.Quantity;

                    // B. [LOGIC MỚI] Trừ kho chi tiết trong bảng KhoHang
                    if (order.StoreId.HasValue)
                    {
                        var khoHang = await _context.KhoHangs
                            .FirstOrDefaultAsync(k => k.SanPhamId == sanPham.Id && k.StoreId == order.StoreId);

                        if (khoHang != null)
                        {
                            khoHang.SoLuong -= item.Quantity;
                            if (khoHang.SoLuong < 0) khoHang.SoLuong = 0; // Chặn âm kho
                        }
                        else
                        {
                            // Trường hợp lỗi dữ liệu (không tìm thấy kho): 
                            // Có thể tạo mới hoặc Log lỗi. Ở đây ta tạm bỏ qua, chỉ trừ tổng.
                            _logger.LogWarning($"Không tìm thấy bản ghi KhoHang cho SP {sanPham.Id} tại Store {order.StoreId}");
                        }
                    }
                }

                // ... (Đoạn sau giữ nguyên: Tạo ChiTietDonHang...)

                var chiTiet = new ChiTietDonHang
                {
                    MaDon = order.MaDon,
                    SanPhamId = item.Id,
                    SoLuong = item.Quantity,
                    Gia = item.Price,
                    GiaGoc = sanPham != null ? sanPham.GiaNhap : 0
                };
                _context.ChiTietDonHangs.Add(chiTiet);
            }
            await _context.SaveChangesAsync();

            // --- [MỚI] GỬI EMAIL XÁC NHẬN ---
            try
            {
                var userEmail = _context.TaiKhoans.Where(u => u.Id == order.TaiKhoanId).Select(u => u.Email).FirstOrDefault();
                if (!string.IsNullOrEmpty(userEmail))
                {
                    string subject = $"[SmartTech] Xác nhận đơn hàng #{order.MaDon}";
                    string body = $@"
                        <h3>Cảm ơn bạn đã đặt hàng tại SmartTech!</h3>
                        <p>Mã đơn hàng: <strong>{order.MaDon}</strong></p>
                        <p>Tổng thanh toán: <strong>{order.TongTien:N0} đ</strong></p>
                        <p>Trạng thái: Đang xử lý</p>
                        <p>Chúng tôi sẽ sớm liên hệ để giao hàng.</p>
                        <hr>
                        <small>Đây là email tự động, vui lòng không trả lời.</small>
                    ";
                    await _emailSender.SendEmailAsync(userEmail, subject, body);
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi gửi mail nhưng không chặn quy trình đặt hàng
                _logger.LogError(ex, "Lỗi gửi email xác nhận đơn hàng.");
            }
            // --------------------------------

            HttpContext.Session.Remove("GioHang");
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("VoucherCode");
            HttpContext.Session.Remove("VoucherGiam");

            return Ok();
        }
    }
}