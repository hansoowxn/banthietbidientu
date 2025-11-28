using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banthietbidientu.Data;
using banthietbidientu.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace banthietbidientu.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DanhGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDanhGia(int sanPhamId, int soSao, string noiDung)
        {
            // 1. Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "Login");
            }

            var username = User.Identity.Name;
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            // 2. Tìm đơn hàng hợp lệ (Đã mua SP này + Trạng thái Hoàn thành)
            // Lấy đơn mới nhất để gán vào đánh giá
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.TaiKhoanId == user.Id
                            && d.TrangThai == 3 // 3 = Hoàn thành
                            && d.ChiTietDonHangs.Any(ct => ct.SanPhamId == sanPhamId))
                .OrderByDescending(d => d.NgayDat)
                .FirstOrDefaultAsync();

            // 3. Kiểm tra nếu KHÔNG tìm thấy đơn hàng hợp lệ
            if (donHang == null)
            {
                TempData["Error"] = "Lỗi: Bạn chưa mua sản phẩm này hoặc đơn hàng chưa hoàn thành.";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
            }

            // 4. Kiểm tra nếu User đã đánh giá sản phẩm này cho đơn hàng này rồi (Tránh spam)
            bool daDanhGia = await _context.DanhGias.AnyAsync(dg => dg.MaDon == donHang.MaDon && dg.SanPhamId == sanPhamId);
            if (daDanhGia)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này cho đơn hàng gần nhất rồi.";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
            }

            // 5. Tạo và Lưu đánh giá (ĐẢM BẢO MaDon KHÔNG NULL)
            var danhGia = new DanhGia
            {
                SanPhamId = sanPhamId,
                TaiKhoanId = user.Id,
                MaDon = donHang.MaDon, // <--- Lấy MaDon từ đơn hàng tìm được (Chắc chắn không null)
                Sao = soSao,
                NoiDung = noiDung,
                NgayTao = DateTime.Now
            };

            try
            {
                _context.DanhGias.Add(danhGia);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                TempData["Error"] = "Có lỗi xảy ra khi lưu đánh giá. Vui lòng thử lại.";
            }

            return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
        }
    }
}