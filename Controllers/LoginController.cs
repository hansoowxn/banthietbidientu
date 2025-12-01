using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using banthietbidientu.Data;
using banthietbidientu.Models;
using banthietbidientu.Services;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;

namespace banthietbidientu.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public LoginController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult DangNhap()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(TaiKhoan model)
        {
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role ?? "User"),
                    new Claim("FullName", user.FullName ?? ""),
                    new Claim("Email", user.Email ?? "")
                };

                if (user.StoreId.HasValue)
                {
                    claims.Add(new Claim("StoreId", user.StoreId.Value.ToString()));
                }

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (user.Role == "Admin" || user.Role == "Boss")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        // [CẬP NHẬT] Đăng ký gửi OTP thay vì lưu luôn
        [HttpPost]
        public async Task<IActionResult> DangKy(TaiKhoan model)
        {
            ModelState.Remove("DonHangs");
            ModelState.Remove("DanhGias");
            ModelState.Remove("YeuCauThuMuas");

            if (ModelState.IsValid)
            {
                if (_context.TaiKhoans.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                if (_context.TaiKhoans.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View(model);
                }

                // 1. Tạo thông tin User tạm (Chưa có ID)
                var newUser = new TaiKhoan
                {
                    Username = model.Username,
                    Password = model.Password,
                    Role = "User",
                    StoreId = null,
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                    Address = model.Address,
                    Gender = model.Gender,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                // 2. Tạo OTP
                Random rand = new Random();
                string otp = rand.Next(100000, 999999).ToString();

                // 3. Lưu User tạm và OTP vào Session
                HttpContext.Session.SetString("RegisterUser", JsonConvert.SerializeObject(newUser));
                HttpContext.Session.SetString("RegisterOTP", otp);

                // 4. Gửi Email
                string subject = "[SmartTech] Xác thực đăng ký tài khoản";
                string body = $@"
                    <div style='font-family:Arial,sans-serif; padding:20px; border:1px solid #eee; border-radius:5px;'>
                        <h3 style='color:#0d6efd'>Chào mừng bạn đến với SmartTech!</h3>
                        <p>Cảm ơn bạn đã đăng ký. Để hoàn tất, vui lòng nhập mã xác thực sau:</p>
                        <h1 style='background:#f8f9fa; padding:10px; border-radius:5px; display:inline-block; letter-spacing:5px;'>{otp}</h1>
                        <p>Mã này có hiệu lực trong 5 phút.</p>
                    </div>";

                try
                {
                    await _emailSender.SendEmailAsync(model.Email, subject, body);
                    return RedirectToAction("VerifyRegisterOtp");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi gửi email: " + ex.Message);
                    return View(model);
                }
            }

            return View(model);
        }

        // --- [MỚI] TRANG XÁC THỰC ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult VerifyRegisterOtp()
        {
            if (HttpContext.Session.GetString("RegisterUser") == null)
            {
                return RedirectToAction("DangKy");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyRegisterOtp(string otp1, string otp2, string otp3, string otp4, string otp5, string otp6)
        {
            string inputOtp = (otp1 + otp2 + otp3 + otp4 + otp5 + otp6)?.Trim();
            string sessionOtp = HttpContext.Session.GetString("RegisterOTP");
            string userJson = HttpContext.Session.GetString("RegisterUser");

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(userJson))
            {
                ViewBag.Error = "Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại!";
                return View();
            }

            if (inputOtp == sessionOtp)
            {
                // OTP Đúng -> Lưu vào Database thật
                var newUser = JsonConvert.DeserializeObject<TaiKhoan>(userJson);

                _context.TaiKhoans.Add(newUser);
                _context.SaveChanges();

                // Xóa Session
                HttpContext.Session.Remove("RegisterUser");
                HttpContext.Session.Remove("RegisterOTP");

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("DangNhap");
            }

            ViewBag.Error = "Mã OTP không chính xác!";
            return View();
        }

        // --- CÁC CHỨC NĂNG QUÊN MẬT KHẨU (GIỮ NGUYÊN) ---

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email!";
                return View();
            }

            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email này chưa được đăng ký trong hệ thống!";
                return View();
            }

            Random rand = new Random();
            string otp = rand.Next(100000, 999999).ToString();

            HttpContext.Session.SetString("ResetOTP", otp);
            HttpContext.Session.SetString("ResetEmail", email);

            string subject = "[SmartTech] Mã xác thực OTP Quên Mật Khẩu";
            string body = $@"
                <div style='font-family:Helvetica,Arial,sans-serif;min-width:1000px;overflow:auto;line-height:2'>
                    <div style='margin:50px auto;width:70%;padding:20px 0'>
                        <div style='border-bottom:1px solid #eee'>
                            <a href='' style='font-size:1.4em;color: #00466a;text-decoration:none;font-weight:600'>SmartTech Store</a>
                        </div>
                        <p style='font-size:1.1em'>Xin chào,</p>
                        <p>Bạn vừa yêu cầu đặt lại mật khẩu. Vui lòng nhập mã OTP sau để tiếp tục. Mã có hiệu lực trong 5 phút.</p>
                        <h2 style='background: #00466a;margin: 0 auto;width: max-content;padding: 0 10px;color: #fff;border-radius: 4px;'>{otp}</h2>
                        <p style='font-size:0.9em;'>Xin cảm ơn,<br />Đội ngũ SmartTech</p>
                    </div>
                </div>";

            try
            {
                await _emailSender.SendEmailAsync(email, subject, body);
                return RedirectToAction("VerifyOtp");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi gửi mail: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (HttpContext.Session.GetString("ResetEmail") == null)
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otp1, string otp2, string otp3, string otp4, string otp5, string otp6)
        {
            string inputOtp = (otp1 + otp2 + otp3 + otp4 + otp5 + otp6)?.Trim();
            string sessionOtp = HttpContext.Session.GetString("ResetOTP");

            if (string.IsNullOrEmpty(sessionOtp))
            {
                ViewBag.Error = "Phiên làm việc đã hết hạn. Vui lòng gửi lại mã!";
                return View();
            }

            if (inputOtp == sessionOtp)
            {
                HttpContext.Session.SetString("CanResetPassword", "true");
                return RedirectToAction("ResetPassword");
            }

            ViewBag.Error = $"Mã OTP không chính xác!";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (HttpContext.Session.GetString("CanResetPassword") != "true")
            {
                return RedirectToAction("ForgotPassword");
            }

            string email = HttpContext.Session.GetString("ResetEmail");
            if (!string.IsNullOrEmpty(email))
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    ViewBag.Username = user.Username;
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            string email = HttpContext.Session.GetString("ResetEmail");
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                user.Password = newPassword;
                _context.SaveChanges();

                HttpContext.Session.Remove("ResetOTP");
                HttpContext.Session.Remove("ResetEmail");
                HttpContext.Session.Remove("CanResetPassword");

                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                return RedirectToAction("DangNhap");
            }

            return RedirectToAction("ForgotPassword");
        }

        // --- CÁC CHỨC NĂNG KHÁC (PROFILE, EDIT...) ---
        [HttpGet]
        public IActionResult Profile()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("DangNhap");

            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId == null) return RedirectToAction("DangNhap");

            if (int.TryParse(claimId.Value, out int userId))
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
                if (user == null) return RedirectToAction("DangNhap");
                return View(user);
            }
            return RedirectToAction("DangNhap");
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("DangNhap");

            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId == null) return RedirectToAction("DangNhap");

            if (int.TryParse(claimId.Value, out int userId))
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
                if (user == null) return RedirectToAction("DangNhap");

                var model = new ChinhSuaTaiKhoan
                {
                    FullName = user.FullName,
                    DateOfBirth = user.DateOfBirth ?? DateTime.Now,
                    Address = user.Address,
                    Gender = user.Gender,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };
                return View(model);
            }
            return RedirectToAction("DangNhap");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(ChinhSuaTaiKhoan model)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("DangNhap");

            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId == null) return RedirectToAction("DangNhap");

            if (int.TryParse(claimId.Value, out int userId))
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
                if (user == null) return RedirectToAction("DangNhap");

                if (ModelState.IsValid)
                {
                    if (_context.TaiKhoans.Any(u => u.Id != userId && u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại với tài khoản khác.");
                        return View(model);
                    }

                    user.FullName = model.FullName;
                    user.DateOfBirth = model.DateOfBirth;
                    user.Address = model.Address;
                    user.Gender = model.Gender;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;

                    try
                    {
                        _context.SaveChanges();
                        TempData["Success"] = "Thông tin tài khoản đã được cập nhật thành công!";
                        return RedirectToAction("Profile");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi khi lưu thông tin: " + ex.Message);
                        return View(model);
                    }
                }
                return View(model);
            }
            return RedirectToAction("DangNhap");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("DangNhap");
            return View(new DoiMatKhau());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(DoiMatKhau model)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("DangNhap");

            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId == null) return RedirectToAction("DangNhap");

            if (int.TryParse(claimId.Value, out int userId))
            {
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
                if (user == null) return RedirectToAction("DangNhap");

                if (!ModelState.IsValid) return View(model);

                if (user.Password != model.CurrentPassword)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                user.Password = model.NewPassword;
                try
                {
                    _context.SaveChanges();
                    TempData["Success"] = "Mật khẩu đã được thay đổi thành công!";
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu mật khẩu: " + ex.Message);
                    return View(model);
                }
            }
            return RedirectToAction("DangNhap");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult LichSuMuaHang()
        {
            var username = User.Identity.Name;
            if (username == null) return RedirectToAction("DangNhap");

            var listDonHang = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .ThenInclude(ct => ct.SanPham)
                .Where(d => d.TaiKhoan.Username == username)
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new HistoryViewModel
                {
                    MaDon = d.MaDon,
                    NgayDat = d.NgayDat ?? DateTime.Now,
                    TrangThai = d.TrangThai == 1 ? "Đã xác nhận" :
                                d.TrangThai == 2 ? "Đang giao" :
                                d.TrangThai == 3 ? "Hoàn thành" :
                                d.TrangThai == -1 ? "Đã hủy" : "Chờ xử lý",
                    TongTien = d.TongTien,
                    PaymentMethod = "COD",
                    SanPhams = d.ChiTietDonHangs.ToList()
                })
                .ToList();

            return View(listDonHang);
        }

        public async Task<IActionResult> ChiTietDonHang(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("LichSuMuaHang");

            var currentUser = User.Identity.Name;
            if (currentUser == null) return RedirectToAction("DangNhap");

            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(m => m.MaDon == id);

            if (donHang == null) return NotFound();

            if (donHang.TaiKhoan != null && donHang.TaiKhoan.Username != currentUser)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View(donHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDonHang(string maDon)
        {
            var username = User.Identity.Name;
            if (username == null) return RedirectToAction("DangNhap");

            var order = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDon == maDon);

            if (order == null) return NotFound();

            if (order.TaiKhoan.Username != username) return RedirectToAction("AccessDenied", "Home");

            if (order.TrangThai == 0 || order.TrangThai == 1)
            {
                order.TrangThai = -1;

                foreach (var item in order.ChiTietDonHangs)
                {
                    var sanPham = await _context.SanPhams.FindAsync(item.SanPhamId);
                    if (sanPham != null)
                    {
                        sanPham.SoLuong += item.SoLuong;
                    }
                }

                var thongBao = new ThongBao
                {
                    TieuDe = "Đơn hàng bị hủy",
                    NoiDung = $"Khách {username} đã hủy đơn hàng #{maDon}",
                    NgayTao = DateTime.Now,
                    DaDoc = false,
                    LoaiThongBao = 3,
                    RedirectId = maDon,
                    RedirectAction = "QuanLyDonHang"
                };
                _context.ThongBaos.Add(thongBao);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này.";
            }

            return RedirectToAction("ChiTietDonHang", new { id = maDon });
        }
    }
}