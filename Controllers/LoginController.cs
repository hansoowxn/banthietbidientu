using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using banthietbidientu.Data;
using banthietbidientu.Models;
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

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
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

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Xử lý giỏ hàng
                var cartJson = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cart = JsonConvert.DeserializeObject<List<GioHang>>(cartJson);
                    if (cart != null)
                    {
                        foreach (var item in cart)
                        {
                            item.Id = 0;
                            item.UserId = user.Id;
                            item.SanPham = null;
                            _context.GioHangs.Add(item);
                        }
                        _context.SaveChanges();
                        HttpContext.Session.Remove("Cart");
                    }
                }

                if (user.Role == "Admin") return RedirectToAction("Index", "Admin");
                else return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        [HttpGet]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DangKy(TaiKhoan model)
        {
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

                var newUser = new TaiKhoan
                {
                    Username = model.Username,
                    Password = model.Password,
                    Role = "User",
                    FullName = model.FullName,
                    // FIX LỖI NGÀY THÁNG Ở ĐÂY:
                    DateOfBirth = model.DateOfBirth ?? DateTime.Now,
                    Address = model.Address,
                    Gender = model.Gender,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                _context.TaiKhoans.Add(newUser);
                _context.SaveChanges();

                return RedirectToAction("DangNhap");
            }

            return View(model);
        }

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
                    // FIX LỖI NGÀY THÁNG Ở ĐÂY:
                    DateOfBirth = user.DateOfBirth ?? DateTime.Now,
                    Address = user.Address,
                    Gender = user.Gender,
                    Email = user.Email
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

            // Chỉ cho hủy khi đơn hàng Mới hoặc Đã xác nhận (0 hoặc 1)
            if (order.TrangThai == 0 || order.TrangThai == 1)
            {
                order.TrangThai = -1; // Đã hủy

                // Hoàn lại kho
                foreach (var item in order.ChiTietDonHangs)
                {
                    var sanPham = await _context.SanPhams.FindAsync(item.SanPhamId);
                    if (sanPham != null)
                    {
                        sanPham.SoLuong += item.SoLuong;
                    }
                }

                // Thông báo cho Admin
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