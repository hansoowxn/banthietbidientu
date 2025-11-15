using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TestDoAn.Data;
using TestDoAn.Models;

namespace TestDoAn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Quản lý sản phẩm
        public IActionResult QuanLySanPham()
        {
            var sanPhams = _context.SanPhams.ToList();
            return View(sanPhams);
        }

        [HttpGet]
        public IActionResult ThemSanPham()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPham(SanPham model)
        {
            if (ModelState.IsValid)
            {
                _context.SanPhams.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("QuanLySanPham");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult SuaSanPham(int id)
        {
            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == id);
            if (sanPham == null)
            {
                return NotFound();
            }
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSanPham(SanPham model)
        {
            if (ModelState.IsValid)
            {
                var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == model.Id);
                if (sanPham == null)
                {
                    return NotFound();
                }
                sanPham.Category = model.Category;
                sanPham.SoLuong = model.SoLuong;
                sanPham.Name = model.Name;
                sanPham.Price = model.Price;
                sanPham.ImageUrl = model.ImageUrl;
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("QuanLySanPham");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaSanPham(int id)
        {
            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == id);
            if (sanPham == null)
            {
                return NotFound();
            }
            _context.SanPhams.Remove(sanPham);
            _context.SaveChanges();
            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction("QuanLySanPham");
        }

        // Quản lý tài khoản
        public IActionResult QuanLyTaiKhoan()
        {
            var taiKhoans = _context.TaiKhoans.ToList();
            return View(taiKhoans);
        }

        [HttpGet]
        public IActionResult ThemTaiKhoan()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemTaiKhoan(TaiKhoan model)
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
                _context.TaiKhoans.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Thêm tài khoản thành công!";
                return RedirectToAction("QuanLyTaiKhoan");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult SuaTaiKhoan(int id)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(u => u.Id == id);
            if (taiKhoan == null)
            {
                return NotFound();
            }
            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaTaiKhoan(TaiKhoan model)
        {
            if (ModelState.IsValid)
            {
                var taiKhoan = _context.TaiKhoans.FirstOrDefault(u => u.Id == model.Id);
                if (taiKhoan == null)
                {
                    return NotFound();
                }
                if (_context.TaiKhoans.Any(u => u.Id != model.Id && u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }
                if (_context.TaiKhoans.Any(u => u.Id != model.Id && u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                    return View(model);
                }
                taiKhoan.Username = model.Username;
                taiKhoan.FullName = model.FullName;
                taiKhoan.DateOfBirth = model.DateOfBirth;
                taiKhoan.Address = model.Address;
                taiKhoan.Gender = model.Gender;
                taiKhoan.Email = model.Email;
                taiKhoan.Role = model.Role;
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật tài khoản thành công!";
                return RedirectToAction("QuanLyTaiKhoan");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaTaiKhoan(int id)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(u => u.Id == id);
            if (taiKhoan == null)
            {
                return NotFound();
            }
            _context.TaiKhoans.Remove(taiKhoan);
            _context.SaveChanges();
            TempData["Success"] = "Xóa tài khoản thành công!";
            return RedirectToAction("QuanLyTaiKhoan");
        }

        // Báo cáo
        public IActionResult BaoCao()
        {
            return View();
        }
        [HttpGet]
        public IActionResult LichSuMuaHang(int userId)
        {
            var taiKhoan = _context.TaiKhoans.FirstOrDefault(u => u.Id == userId);
            if (taiKhoan == null)
            {
                return NotFound();
            }
            var lichSu = _context.LichSuMuaHangs
                .Include(l => l.SanPham)
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.PurchaseDate)
                .ToList();
            ViewBag.TaiKhoan = taiKhoan;
            return View(lichSu);
        }
    }

}