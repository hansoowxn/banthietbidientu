using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using TestDoAn.Data;
using TestDoAn.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace TestDoAn.Controllers
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
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FullName", user.FullName ?? ""),
                    new Claim("Email", user.Email ?? "")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                var cartJson = HttpContext.Session.GetString("Cart");
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cart = JsonConvert.DeserializeObject<List<GioHang>>(cartJson);
                    foreach (var item in cart)
                    {
                        item.UserId = user.Id;
                        _context.GioHangs.Add(item);
                    }
                    _context.SaveChanges();
                    HttpContext.Session.Remove("Cart");
                }

                return RedirectToAction("Index", "Home");
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
                    DateOfBirth = model.DateOfBirth,
                    Address = model.Address,
                    Gender = model.Gender,
                    Email = model.Email
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
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("DangNhap");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("DangNhap");

            return View(user);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("DangNhap");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("DangNhap");

            var model = new ChinhSuaTaiKhoan
            {
                FullName = user.FullName,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                Gender = user.Gender,
                Email = user.Email
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(ChinhSuaTaiKhoan model)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("DangNhap");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("DangNhap");

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

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("DangNhap");

            return View(new DoiMatKhau());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(DoiMatKhau model)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("DangNhap");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("DangNhap");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult LichSuMuaHang()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("DangNhap");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var lichSu = _context.LichSuMuaHangs
                .Include(l => l.SanPham)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.PurchaseDate)
                .ToList();
            return View(lichSu);
        }
    }
}