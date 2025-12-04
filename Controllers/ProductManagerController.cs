using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using banthietbidientu.Data;
using banthietbidientu.Models;
using banthietbidientu.Services;
using System;

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
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Lưu sản phẩm trước
                        model.SoLuong = slHaNoi + slDaNang + slHCM;
                        if (string.IsNullOrEmpty(model.MoTa)) model.MoTa = "";

                        _context.SanPhams.Add(model);
                        _context.SaveChanges(); // Lưu lần 1 để lấy ID

                        // 2. Lưu kho hàng
                        var k1 = new KhoHang { SanPhamId = model.Id, StoreId = 1, SoLuong = slHaNoi };
                        var k2 = new KhoHang { SanPhamId = model.Id, StoreId = 2, SoLuong = slDaNang };
                        var k3 = new KhoHang { SanPhamId = model.Id, StoreId = 3, SoLuong = slHCM };

                        _context.KhoHangs.AddRange(k1, k2, k3);
                        _context.SaveChanges(); // Lưu lần 2

                        transaction.Commit(); // Chốt giao dịch

                        GhiNhatKy("Thêm sản phẩm", $"Thêm mới: {model.Name}");
                        TempData["Success"] = "Thêm sản phẩm thành công!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Hoàn tác nếu lỗi
                        string err = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        TempData["Error"] = $"LỖI LƯU DB: {err}"; // Hiện lỗi ra màn hình
                    }
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Dữ liệu không hợp lệ: " + string.Join(", ", errors);
            }
            return View("~/Views/Admin/ThemSanPham.cshtml", model);
        }

        [Authorize(Roles = "Boss")]
        [HttpGet]
        public IActionResult SuaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp == null) return NotFound();

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
            try
            {
                var sp = _context.SanPhams.Find(model.Id);
                if (sp != null)
                {
                    // Cập nhật thông tin cơ bản
                    sp.Name = model.Name;
                    sp.Price = model.Price;
                    sp.GiaNhap = model.GiaNhap; // [QUAN TRỌNG] Đã thêm cập nhật giá nhập
                    sp.Category = model.Category;
                    sp.ImageUrl = model.ImageUrl;
                    sp.Description = model.Description;
                    sp.SoLuong = slHaNoi + slDaNang + slHCM;

                    // Cập nhật Kho
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

                    _context.SaveChanges(); // Lưu thay đổi

                    GhiNhatKy("Sửa sản phẩm", $"Cập nhật SP {sp.Id}");
                    TempData["Success"] = "Cập nhật thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm!";
                }
            }
            catch (Exception ex)
            {
                string err = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = $"LỖI LƯU DB: {err}";
                return View("~/Views/Admin/SuaSanPham.cshtml", model);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public IActionResult XoaSanPham(int id)
        {
            try
            {
                var sp = _context.SanPhams.Find(id);
                if (sp != null)
                {
                    var khoHangs = _context.KhoHangs.Where(k => k.SanPhamId == id);
                    _context.KhoHangs.RemoveRange(khoHangs);

                    string tenSp = sp.Name;
                    _context.SanPhams.Remove(sp);
                    _context.SaveChanges();

                    GhiNhatKy("Xóa sản phẩm", $"Đã xóa: {tenSp}");
                    TempData["Success"] = "Xóa sản phẩm thành công!";
                }
            }
            catch (Exception ex)
            {
                string err = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = $"Không thể xóa (có thể do ràng buộc đơn hàng): {err}";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Boss")]
        [HttpPost]
        public async Task<IActionResult> CapNhatGiaNhapNhanh(int id, decimal giaNhap)
        {
            try
            {
                var sp = await _context.SanPhams.FindAsync(id);
                if (sp != null && giaNhap >= 0)
                {
                    sp.GiaNhap = giaNhap;
                    await _context.SaveChangesAsync();
                    GhiNhatKy("Cập nhật giá vốn", $"Cập nhật nhanh giá vốn SP {id}: {giaNhap:N0}");
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}