using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models;
using Microsoft.AspNetCore.Hosting; // Thêm dòng này để dùng IWebHostEnvironment
using System.IO; // Thêm dòng này để dùng Path, Directory, FileStream

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

        public IActionResult Index(string search, string category)
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
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi không xác định khi tải sản phẩm.");
                Console.WriteLine("Lỗi chung khi tải sản phẩm: " + ex.Message);
                return View(new List<SanPham>());
            }
        }

        // --- ACTION CHI TIẾT SẢN PHẨM (ĐÃ GỘP LOGIC ĐÁNH GIÁ) ---
        public IActionResult ChiTietSanPham(int id)
        {
            // 1. Lấy sản phẩm
            var sanPham = _context.SanPhams.FirstOrDefault(p => p.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            // 2. Lấy danh sách đánh giá
            var danhGias = _context.DanhGias
                .Include(d => d.TaiKhoan) // Lấy thông tin người đánh giá
                .Where(d => d.SanPhamId == id)
                .OrderByDescending(d => d.NgayTao)
                .ToList();

            // 3. Tính điểm trung bình
            double diemTrungBinh = 0;
            if (danhGias.Any())
            {
                diemTrungBinh = danhGias.Average(d => d.Sao);
            }

            // 4. Kiểm tra quyền đánh giá
            bool duocPhepDanhGia = false;
            if (User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                var user = _context.TaiKhoans.FirstOrDefault(u => u.Username == username);
                if (user != null)
                {
                    // Check: Đã mua hàng + Đơn hoàn thành + Chưa đánh giá (hoặc cho phép đánh giá nhiều lần)
                    var daMua = _context.DonHangs
                        .Include(d => d.ChiTietDonHangs)
                        .Any(d => d.TaiKhoanId == user.Id
                               && d.TrangThai == 3
                               && d.ChiTietDonHangs.Any(ct => ct.SanPhamId == id));

                    duocPhepDanhGia = daMua;
                }
            }

            // 5. Truyền dữ liệu qua ViewBag
            ViewBag.DanhGias = danhGias;
            ViewBag.DiemTrungBinh = diemTrungBinh;
            ViewBag.LuotDanhGia = danhGias.Count;
            ViewBag.DuocPhepDanhGia = duocPhepDanhGia;

            return View(sanPham);
        }

        // Action này có vẻ dư thừa nếu bạn đã dùng ChiTietSanPham, 
        // nhưng nếu bạn có dùng ở đâu đó khác thì giữ lại, đổi tên hoặc xóa đi nếu không dùng.
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
                    TieuDe = "Yêu cầu Thu cũ đổi mới", // Đã fix lỗi thiếu TieuDe
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