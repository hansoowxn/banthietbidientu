using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banthietbidientu.Data;
using banthietbidientu.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting; // Thêm: Để xử lý đường dẫn file
using Microsoft.AspNetCore.Http;    // Thêm: Để dùng IFormFile
using System.IO;                    // Thêm: Để thao tác file

namespace banthietbidientu.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Khai báo biến môi trường

        // Inject thêm IWebHostEnvironment vào Constructor
        public DanhGiaController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Thêm tham số: IFormFile hinhAnh
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

            // 2. Tìm đơn hàng hợp lệ
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.TaiKhoanId == user.Id
                            && d.TrangThai == 3
                            && d.ChiTietDonHangs.Any(ct => ct.SanPhamId == sanPhamId))
                .OrderByDescending(d => d.NgayDat)
                .FirstOrDefaultAsync();

            if (donHang == null)
            {
                TempData["Error"] = "Lỗi: Bạn chưa mua sản phẩm này hoặc đơn hàng chưa hoàn thành.";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
            }

            bool daDanhGia = await _context.DanhGias.AnyAsync(dg => dg.MaDon == donHang.MaDon && dg.SanPhamId == sanPhamId);
            if (daDanhGia)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này cho đơn hàng gần nhất rồi.";
                return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
            }

            // 3. Xử lý Upload Ảnh (MỚI)
            string imagePath = null;
            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                // Tạo thư mục lưu trữ nếu chưa có: wwwroot/uploads/reviews
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/reviews");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                // Tạo tên file ngẫu nhiên để tránh trùng
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await hinhAnh.CopyToAsync(fileStream);
                }

                // Đường dẫn lưu vào DB
                imagePath = "/uploads/reviews/" + fileName;
            }

            // 4. Tạo đối tượng đánh giá
            var danhGia = new DanhGia
            {
                SanPhamId = sanPhamId,
                TaiKhoanId = user.Id,
                MaDon = donHang.MaDon,
                Sao = soSao,
                NoiDung = noiDung,
                HinhAnh = imagePath, // Lưu đường dẫn ảnh vào đây
                NgayTao = DateTime.Now,
                DaDuyet = true // Tạm thời cho hiện luôn
            };

            try
            {
                _context.DanhGias.Add(danhGia);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi lưu đánh giá. Vui lòng thử lại.";
            }

            return RedirectToAction("ChiTietSanPham", "Home", new { id = sanPhamId });
        }
    }
}