using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banthietbidientu.Data;
using banthietbidientu.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace banthietbidientu.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DanhGiaController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemDanhGia(int sanPhamId, int soSao, string noiDung, IFormFile hinhAnh)
        {
            // 1. Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "Login");
            }

            var username = User.Identity.Name;
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("DangNhap", "Login");

            // 2. Tìm đơn hàng hợp lệ (Đã mua SP này + Hoàn thành)
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.TaiKhoanId == user.Id
                            && d.TrangThai == 3 // 3 = Hoàn thành
                            && d.ChiTietDonHangs.Any(ct => ct.SanPhamId == sanPhamId))
                .OrderByDescending(d => d.NgayDat)
                .FirstOrDefaultAsync();

            if (donHang == null)
            {
                TempData["Error"] = "Lỗi: Bạn cần mua sản phẩm này và đơn hàng ở trạng thái 'Hoàn thành' để đánh giá.";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
            }

            // 3. Kiểm tra đã đánh giá chưa
            bool daDanhGia = await _context.DanhGias
                .AnyAsync(dg => dg.MaDon == donHang.MaDon && dg.SanPhamId == sanPhamId);

            if (daDanhGia)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này cho đơn hàng gần nhất rồi.";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
            }

            // 4. Xử lý Upload Ảnh
            string imagePath = null;
            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reviews");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await hinhAnh.CopyToAsync(fileStream);
                }
                imagePath = "/uploads/reviews/" + fileName;
            }

            // 5. Lưu đánh giá
            var danhGia = new DanhGia
            {
                SanPhamId = sanPhamId,
                TaiKhoanId = user.Id,
                MaDon = donHang.MaDon,
                Sao = soSao,
                NoiDung = noiDung ?? "",
                HinhAnh = imagePath ?? "",
                TraLoi = "",
                NgayTao = DateTime.Now,
                // Lưu ý: Nếu muốn Admin duyệt trước khi hiện thì để false
                DaDuyet = true
            };

            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            // --- 6. [PHẦN BỔ SUNG] TẠO THÔNG BÁO CHO ADMIN ---
            try
            {
                var sanPham = await _context.SanPhams.FindAsync(sanPhamId);
                string tenSp = sanPham?.Name ?? "Sản phẩm";
                string extraInfo = (imagePath != null) ? " (kèm ảnh)" : "";

                var thongBao = new ThongBao
                {
                    TieuDe = "Đánh giá mới" + extraInfo,
                    NoiDung = $"{user.FullName ?? user.Username} đánh giá {soSao} sao cho {tenSp}",
                    NgayTao = DateTime.Now,
                    DaDoc = false,
                    LoaiThongBao = 1, // Icon tin nhắn/đánh giá

                    // Cấu hình chuyển hướng Highlight
                    RedirectAction = "QuanLyDanhGia",
                    RedirectId = danhGia.Id.ToString()
                };

                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần, nhưng không chặn việc đánh giá thành công
                Console.WriteLine("Lỗi tạo thông báo: " + ex.Message);
            }
            // --------------------------------------------------

            TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
        }
    }
}