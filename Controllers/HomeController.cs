using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models; // Đảm bảo bạn đã import namespace chia TaiKhoanContext và SanPham

namespace TestDoAn.Controllers
{
    // Giả sử tên Controller là Home
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string category)
        {

            try
            {

                ViewData["Categories"] = _context.SanPhams
                                               .Select(p => p.Category)
                                               .Distinct()
                                               .ToList();
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("timeout"))
            {

                ViewData["Categories"] = new List<string>();
                Console.WriteLine("L?i Timeout khi l?y danh m?c s?n ph?m: " + ex.Message);
            }
            catch (Exception ex)
            {

                ViewData["Categories"] = new List<string>();
                Console.WriteLine("L?i chung khi l?y danh m?c s?n ph?m: " + ex.Message);
            }



            var sanphamQuery = _context.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {

                sanphamQuery = sanphamQuery.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            if (!string.IsNullOrEmpty(category) && category != "ALL")
            {
                sanphamQuery = sanphamQuery.Where(p => p.Category == category);
            }

            ViewData["SearchQuery"] = search;
            ViewData["SelectedCategory"] = category;


            try
            {
                var sanphams = sanphamQuery.ToList();
                return View(sanphams);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("timeout"))
            {

                ModelState.AddModelError("", "H? th?ng t?m ki?m s?n ph?m b? quá t?i (Timeout). Vui l?ng th? l?i sau giây lát.");
                Console.WriteLine("L?i Timeout khi t?m ki?m/hi?n th? s?n ph?m: " + ex.Message);
                return View(new List<SanPham>());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đ? x?y ra l?i không xác đ?nh khi t?i s?n ph?m.");
                Console.WriteLine("L?i chung khi t?i s?n ph?m: " + ex.Message);
                return View(new List<SanPham>());
            }
        }

        public IActionResult ChiTietSanPham(int id)
        {
            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }
            return View(sanPham);
        }

        public IActionResult ChiTiet(int id)
        {
            var product = _context.SanPhams.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ThuMuaMayCu()
        {
            var model = new YeuCauThuMuaViewModel();

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userId, out int id))
                {
                    var user = _context.TaiKhoans.FirstOrDefault(u => u.Id == id);
                    if (user != null)
                    {
                        model.SoDienThoai = user.PhoneNumber;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GuiYeuCauThuMua(YeuCauThuMuaViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = "";
                if (model.HinhAnhMay != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/thumua");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.HinhAnhMay.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.HinhAnhMay.CopyToAsync(fileStream);
                    }
                    imagePath = "/uploads/thumua/" + uniqueFileName;
                }

                string noiDungThongBao = $"THU CŨ: Khách (SĐT: {model.SoDienThoai}) muốn bán {model.TenMay}. Tình trạng: {model.TinhTrang}. {(string.IsNullOrEmpty(model.GhiChu) ? "" : "Note: " + model.GhiChu)}";

               
                var thongBao = new ThongBao
                {
                    TieuDe = "Yêu cầu Thu cũ đổi mới",
                    NoiDung = noiDungThongBao,
                    NgayTao = DateTime.Now,
                    DaDoc = false,
                    LoaiThongBao = 2
                };

                _context.ThongBaos.Add(thongBao);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã gửi yêu cầu thành công! Nhân viên sẽ liên hệ định giá trong 15 phút.";
                return RedirectToAction("ThuMuaMayCu");
            }

            return View(model);
        }
    }
}