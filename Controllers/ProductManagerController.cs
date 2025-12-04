using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using banthietbidientu.Data;
using banthietbidientu.Models;
using banthietbidientu.Services;

namespace banthietbidientu.Controllers
{
    public class ProductManagerController : BaseAdminController
    {
        public ProductManagerController(ApplicationDbContext context, IEmailSender emailSender) : base(context, emailSender)
        {
        }

        public IActionResult Index()
        {
            return View("~/Views/Admin/QuanLySanPham.cshtml", _context.SanPhams.AsNoTracking().ToList());
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult ThemSanPham()
        {
            return View("~/Views/Admin/ThemSanPham.cshtml");
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPham(SanPham model, int slHaNoi, int slDaNang, int slHCM)
        {
            if (ModelState.IsValid)
            {
                // 1. Lưu thông tin chung
                model.SoLuong = slHaNoi + slDaNang + slHCM; // Tổng tồn kho hiển thị
                if (string.IsNullOrEmpty(model.MoTa)) model.MoTa = ""; // Không dùng MoTa lưu kho nữa

                _context.SanPhams.Add(model);
                _context.SaveChanges();

                // 2. [LOGIC MỚI] Lưu vào bảng KhoHang
                var k1 = new KhoHang { SanPhamId = model.Id, StoreId = 1, SoLuong = slHaNoi };
                var k2 = new KhoHang { SanPhamId = model.Id, StoreId = 2, SoLuong = slDaNang };
                var k3 = new KhoHang { SanPhamId = model.Id, StoreId = 3, SoLuong = slHCM };

                _context.KhoHangs.AddRange(k1, k2, k3);
                _context.SaveChanges();

                GhiNhatKy("Thêm sản phẩm", $"Thêm mới: {model.Name} (HN:{slHaNoi}, ĐN:{slDaNang}, HCM:{slHCM})");
                return RedirectToAction("Index");
            }
            return View("~/Views/Admin/ThemSanPham.cshtml", model);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult SuaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp == null) return NotFound();

            // [LOGIC MỚI] Lấy dữ liệu từ bảng KhoHang đổ lên View
            var khoHangs = _context.KhoHangs.Where(k => k.SanPhamId == id).ToList();

            ViewBag.SlHaNoi = khoHangs.FirstOrDefault(k => k.StoreId == 1)?.SoLuong ?? 0;
            ViewBag.SlDaNang = khoHangs.FirstOrDefault(k => k.StoreId == 2)?.SoLuong ?? 0;
            ViewBag.SlHCM = khoHangs.FirstOrDefault(k => k.StoreId == 3)?.SoLuong ?? 0;

            return View("~/Views/Admin/SuaSanPham.cshtml", sp);
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSanPham(SanPham model, int slHaNoi, int slDaNang, int slHCM)
        {
            var sp = _context.SanPhams.Find(model.Id);
            if (sp != null)
            {
                // Cập nhật thông tin
                sp.Name = model.Name;
                sp.Price = model.Price;
                sp.GiaNhap = model.GiaNhap;
                sp.Category = model.Category;
                sp.ImageUrl = model.ImageUrl;
                sp.Description = model.Description;
                sp.SoLuong = slHaNoi + slDaNang + slHCM;

                // [LOGIC MỚI] Cập nhật bảng KhoHang
                var khoHangs = _context.KhoHangs.Where(k => k.SanPhamId == sp.Id).ToList();

                void UpdateStock(int storeId, int qty)
                {
                    var kho = khoHangs.FirstOrDefault(k => k.StoreId == storeId);
                    if (kho != null) kho.SoLuong = qty;
                    else _context.KhoHangs.Add(new KhoHang { SanPhamId = sp.Id, StoreId = storeId, SoLuong = qty });
                }

                UpdateStock(1, slHaNoi);
                UpdateStock(2, slDaNang);
                UpdateStock(3, slHCM);

                _context.SaveChanges();
                GhiNhatKy("Sửa sản phẩm", $"Cập nhật SP {sp.Id}: HN({slHaNoi}) - ĐN({slDaNang}) - HCM({slHCM})");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult XoaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp != null)
            {
                // Xóa cả dữ liệu kho liên quan (Cascade delete thường tự lo, nhưng xóa tay cho chắc)
                var khoHangs = _context.KhoHangs.Where(k => k.SanPhamId == id);
                _context.KhoHangs.RemoveRange(khoHangs);

                string tenSp = sp.Name;
                _context.SanPhams.Remove(sp);
                _context.SaveChanges();
                GhiNhatKy("Xóa sản phẩm", $"Đã xóa: {tenSp}");
            }
            return RedirectToAction("Index");
        }

        // API Cập nhật giá nhanh
        [Authorize(Roles = "Boss")]
        [HttpPost]
        public async Task<IActionResult> CapNhatGiaNhapNhanh(int id, decimal giaNhap)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp != null && giaNhap >= 0)
            {
                sp.GiaNhap = giaNhap;
                await _context.SaveChangesAsync();
                GhiNhatKy("Cập nhật giá vốn", $"Cập nhật nhanh giá vốn SP {id}: {giaNhap:N0}");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}