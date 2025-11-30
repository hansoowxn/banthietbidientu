using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace banthietbidientu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- TRANG CHỦ & TÌM KIẾM & SẮP XẾP ---
        public IActionResult Index(string search, string category, string sortOrder)
        {
            try
            {
                ViewData["Categories"] = _context.SanPhams
                                               .Select(p => p.Category)
                                               .Distinct()
                                               .ToList();
            }
            catch (Exception ex)
            {
                ViewData["Categories"] = new List<string>();
                Console.WriteLine("Lỗi khi lấy danh mục sản phẩm: " + ex.Message);
            }

            var sanphamQuery = _context.SanPhams.AsQueryable();

            // 1. Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                sanphamQuery = sanphamQuery.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // 2. Lọc Danh mục
            if (!string.IsNullOrEmpty(category) && category != "ALL")
            {
                sanphamQuery = sanphamQuery.Where(p => p.Category == category);
            }

            // 3. Sắp xếp (Sorting)
            // Logic sắp xếp theo yêu cầu của bạn
            switch (sortOrder)
            {
                case "price_asc": // Giá thấp -> cao
                    sanphamQuery = sanphamQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc": // Giá cao -> thấp
                    sanphamQuery = sanphamQuery.OrderByDescending(p => p.Price);
                    break;
                case "name_asc": // Tên A-Z
                    sanphamQuery = sanphamQuery.OrderBy(p => p.Name);
                    break;
                case "newest": // Mới nhất (theo ID giảm dần hoặc ngày tạo nếu có)
                default:
                    sanphamQuery = sanphamQuery.OrderByDescending(p => p.Id);
                    break;
            }

            // Lưu lại trạng thái để hiển thị trên View
            ViewData["SearchQuery"] = search;
            ViewData["SelectedCategory"] = category;
            ViewData["CurrentSort"] = sortOrder;

            try
            {
                var sanphams = sanphamQuery.ToList();
                return View(sanphams);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi không xác định khi tải sản phẩm.");
                Console.WriteLine("Lỗi chung khi tải sản phẩm: " + ex.Message);
                return View(new List<SanPham>());
            }
        }

        // --- [MỚI] API TÌM KIẾM TỨC THÌ (LIVE SEARCH) ---
        // Trả về JSON để JS hiển thị dropdown gợi ý
        [HttpGet]
        public IActionResult SearchLive(string query)
        {
            if (string.IsNullOrEmpty(query)) return Json(new List<object>());

            var products = _context.SanPhams
                .AsNoTracking()
                .Where(p => p.Name.Contains(query))
                .OrderByDescending(p => p.Id)
                .Take(5) // Chỉ lấy 5 sản phẩm gợi ý
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    image = p.ImageUrl
                })
                .ToList();

            return Json(products);
        }

        // --- ACTION CHI TIẾT SẢN PHẨM ---
        public IActionResult ChiTietSanPham(int id)
        {
            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == id);
            if (sanPham == null) return NotFound();

            var danhGias = _context.DanhGias
                .Include(d => d.TaiKhoan)
                .Where(d => d.SanPhamId == id)
                .OrderByDescending(d => d.NgayTao)
                .ToList();

            double diemTrungBinh = 0;
            if (danhGias.Any()) diemTrungBinh = danhGias.Average(d => d.Sao);

            bool duocPhepDanhGia = false;
            if (User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    var daMua = _context.DonHangs
                        .Include(d => d.ChiTietDonHangs)
                        .Any(d => d.TaiKhoanId == user.Id
                               && d.TrangThai == 3
                               && d.ChiTietDonHangs.Any(ct => ct.SanPhamId == id));
                    duocPhepDanhGia = daMua;
                }
            }

            ViewBag.DanhGias = danhGias;
            ViewBag.DiemTrungBinh = diemTrungBinh;
            ViewBag.LuotDanhGia = danhGias.Count;
            ViewBag.DuocPhepDanhGia = duocPhepDanhGia;

            return View(sanPham);
        }

        public IActionResult ChiTiet(int id)
        {
            var product = _context.SanPhams.FirstOrDefault(p => p.Id == id);
            return product == null ? NotFound() : View(product);
        }

        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();

        [HttpGet]
        [Authorize]
        public IActionResult ThuMuaMayCu()
        {
            var model = new YeuCauThuMuaViewModel();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userId, out int id))
                {
                    var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == id);
                    if (user != null) model.SoDienThoai = user.PhoneNumber;
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GuiYeuCauThuMua(YeuCauThuMuaViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = "";
                if (model.HinhAnhMay != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/thumua");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.HinhAnhMay.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create)) { await model.HinhAnhMay.CopyToAsync(fileStream); }
                    imagePath = "/uploads/thumua/" + uniqueFileName;
                }

                int? taiKhoanId = null;
                var username = User.Identity.Name;
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == username);
                if (user != null) taiKhoanId = user.Id;

                var yeuCau = new YeuCauThuMua
                {
                    TaiKhoanId = taiKhoanId,
                    TenMay = model.TenMay,
                    TinhTrang = model.TinhTrang,
                    SoDienThoai = model.SoDienThoai,
                    GhiChu = model.GhiChu ?? "",
                    HinhAnh = imagePath,
                    TrangThai = 0,
                    NgayTao = DateTime.Now,
                    MaYeuCau = "_TEMP_"
                };

                _context.YeuCauThuMuas.Add(yeuCau);
                await _context.SaveChangesAsync();
                yeuCau.MaYeuCau = $"YM{yeuCau.Id:D6}";
                await _context.SaveChangesAsync();

                var thongBao = new ThongBao
                {
                    TieuDe = "Yêu cầu Thu cũ mới",
                    NoiDung = $"Yêu cầu {yeuCau.MaYeuCau} của khách {user.FullName ?? user.Username}",
                    NgayTao = DateTime.Now,
                    DaDoc = false,
                    LoaiThongBao = 2,
                    RedirectId = yeuCau.Id.ToString(),
                    RedirectAction = "QuanLyThuMua"
                };
                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã gửi yêu cầu {yeuCau.MaYeuCau} thành công! Nhân viên sẽ liên hệ định giá trong 15 phút.";
                return RedirectToAction("ThuMuaMayCu");
            }
            return View(model);
        }
    }
}