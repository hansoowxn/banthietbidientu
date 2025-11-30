using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models;
using System.Collections.Generic;
using System;
using System.Text; // Quan trọng: Để dùng StringBuilder, Encoding
using System.Threading.Tasks; // Quan trọng: Để dùng Async/Await

namespace banthietbidientu.Controllers
{
    // [QUAN TRỌNG] Cho phép cả "Admin" và "Boss" truy cập
    [Authorize(Roles = "Admin,Boss")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. DASHBOARD ---
        public IActionResult Index()
        {
            // Lấy thông tin User hiện tại để lọc dữ liệu
            var user = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == User.Identity.Name);
            int? storeId = user?.StoreId; // Nếu là Admin chi nhánh -> có StoreId

            // Tạo Query cơ bản cho Đơn hàng
            var queryDonHang = _context.DonHangs.AsQueryable();

            // Nếu là Admin chi nhánh (StoreId != null) -> Lọc đơn hàng của Store đó
            if (storeId.HasValue)
            {
                queryDonHang = queryDonHang.Where(d => d.StoreId == storeId);
            }

            // Tính toán các chỉ số
            var tongDoanhThu = queryDonHang.Where(d => d.TrangThai == 3).Sum(x => x.TongTien);
            var donHangMoi = queryDonHang.Count(x => x.NgayDat.Value.Date == DateTime.Today);
            var tongKhachHang = _context.TaiKhoans.Count(x => x.Role == "User");

            // Tạm thời tính tổng sản phẩm sắp hết (chưa tách kho)
            var sapHetHang = _context.SanPhams.Count(x => x.SoLuong < 5);

            var sttHoanThanh = queryDonHang.Count(x => x.TrangThai == 3);
            var sttDangGiao = queryDonHang.Count(x => x.TrangThai == 2);
            var sttChoXuLy = queryDonHang.Count(x => x.TrangThai == 0 || x.TrangThai == 1);
            var sttDaHuy = queryDonHang.Count(x => x.TrangThai == -1);

            ViewBag.ChartData = new List<int> { sttHoanThanh, sttDangGiao, sttChoXuLy, sttDaHuy };

            // Biểu đồ doanh thu 7 ngày (đã lọc theo Store)
            var revenueWeek = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                var total = queryDonHang
                    .Where(x => x.NgayDat.Value.Date == day && x.TrangThai == 3)
                    .Sum(x => (decimal?)x.TongTien) ?? 0;
                revenueWeek.Add(total);
            }
            ViewBag.RevenueWeek = revenueWeek;

            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.DonHangMoi = donHangMoi;
            ViewBag.TongKhachHang = tongKhachHang;
            ViewBag.SapHetHang = sapHetHang;

            return View();
        }

        // --- 2. QUẢN LÝ ĐƠN HÀNG (CÓ LỌC THEO STORE) ---
        public IActionResult QuanLyDonHang(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var user = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == User.Identity.Name);
            int? storeId = user?.StoreId;

            var query = _context.DonHangs
                .Include(d => d.TaiKhoan)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .AsQueryable();

            // [LỌC] Nếu là Admin chi nhánh -> Chỉ xem đơn của Store mình
            if (storeId.HasValue)
            {
                query = query.Where(d => d.StoreId == storeId);
            }

            query = query.OrderByDescending(d => d.NgayDat);

            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var listDonHang = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DonHangViewModel
                {
                    MaDon = d.MaDon,
                    TenKhachHang = d.TaiKhoan != null ? d.TaiKhoan.FullName : "Khách vãng lai",
                    NgayDat = d.NgayDat,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai == 1 ? "Đã xác nhận" :
                                d.TrangThai == 2 ? "Đang giao" :
                                d.TrangThai == 3 ? "Hoàn thành" :
                                d.TrangThai == -1 ? "Đã hủy" : "Chờ xử lý",
                    SanPhams = d.ChiTietDonHangs.Select(ct => new DonHangViewModel
                    {
                        TenSanPham = ct.SanPham.Name,
                        HinhAnh = ct.SanPham.ImageUrl,
                        SoLuong = ct.SoLuong,
                        Gia = ct.Gia
                    }).ToList()
                })
                .ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = pageNumber;

            return View(listDonHang);
        }

        [HttpPost]
        public IActionResult CapNhatTrangThai(string orderId, string status)
        {
            try
            {
                var donHang = _context.DonHangs.FirstOrDefault(x => x.MaDon == orderId);
                if (donHang == null) return Json(new { success = false, message = $"Không tìm thấy đơn {orderId}" });

                int statusInt = 0;
                if (status == "Đã xác nhận") statusInt = 1;
                else if (status == "Đang giao") statusInt = 2;
                else if (status == "Hoàn thành") statusInt = 3;
                else if (status == "Đã hủy") statusInt = -1;

                donHang.TrangThai = statusInt;

                _context.SaveChanges();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // --- 3. QUẢN LÝ NHẬP HÀNG ---
        public IActionResult QuanLyNhapHang()
        {
            var listPhieuNhap = _context.PhieuNhaps
                .Include(p => p.ChiTiets)
                    .ThenInclude(ct => ct.SanPham)
                .OrderByDescending(p => p.NgayNhap)
                .ToList();

            return View(listPhieuNhap);
        }

        [HttpGet]
        public IActionResult NhapHang()
        {
            ViewBag.SanPhams = _context.SanPhams.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult XuLyNhapHang(int sanPhamId, int soLuong, decimal giaNhap, string ghiChu)
        {
            var sanPham = _context.SanPhams.Find(sanPhamId);
            if (sanPham == null) return NotFound();

            decimal tongGiaTriCu = sanPham.SoLuong * sanPham.GiaNhap;
            decimal tongGiaTriNhap = soLuong * giaNhap;
            int tongSoLuongMoi = sanPham.SoLuong + soLuong;

            if (tongSoLuongMoi > 0)
            {
                sanPham.GiaNhap = (tongGiaTriCu + tongGiaTriNhap) / tongSoLuongMoi;
            }

            sanPham.SoLuong += soLuong;

            var phieuNhap = new PhieuNhap
            {
                NgayNhap = DateTime.Now,
                GhiChu = string.IsNullOrEmpty(ghiChu) ? "Nhập hàng" : ghiChu,
                TongTien = soLuong * giaNhap
            };

            _context.PhieuNhaps.Add(phieuNhap);
            _context.SaveChanges();

            var chiTiet = new ChiTietPhieuNhap
            {
                PhieuNhapId = phieuNhap.Id,
                SanPhamId = sanPhamId,
                SoLuongNhap = soLuong,
                GiaNhap = giaNhap
            };

            _context.ChiTietPhieuNhaps.Add(chiTiet);
            _context.SaveChanges();

            TempData["Success"] = $"Đã nhập kho {soLuong} {sanPham.Name}. Giá vốn mới: {sanPham.GiaNhap:N0}đ";
            return RedirectToAction("QuanLyNhapHang");
        }

        // --- 4. API THÔNG BÁO ---
        [HttpGet]
        public IActionResult GetThongBao()
        {
            try
            {
                var thongBaos = _context.ThongBaos
                    .OrderByDescending(x => x.NgayTao)
                    .Take(5)
                    .Select(x => new {
                        id = x.Id,
                        tieuDe = x.TieuDe,
                        noiDung = x.NoiDung,
                        ngayTao = x.NgayTao,
                        daDoc = x.DaDoc,
                        loaiThongBao = x.LoaiThongBao,
                        redirectId = x.RedirectId,
                        redirectAction = x.RedirectAction
                    })
                    .ToList();

                var soChuaDoc = _context.ThongBaos.Count(x => !x.DaDoc);
                return Json(new { success = true, data = thongBaos, unread = soChuaDoc });
            }
            catch
            {
                return Json(new { success = true, data = new List<object>(), unread = 0 });
            }
        }

        [HttpPost]
        public IActionResult MarkAsRead()
        {
            try
            {
                var list = _context.ThongBaos.Where(x => !x.DaDoc).ToList();
                foreach (var item in list) item.DaDoc = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch { return Json(new { success = false }); }
        }

        [HttpPost]
        public IActionResult MarkOneRead(int id)
        {
            try
            {
                var thongBao = _context.ThongBaos.Find(id);
                if (thongBao != null)
                {
                    thongBao.DaDoc = true;
                    _context.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch { }
            return Json(new { success = false });
        }

        public IActionResult QuanLyThongBao()
        {
            var list = _context.ThongBaos.OrderByDescending(x => x.NgayTao).ToList();
            return View(list);
        }

        // --- 5. QUẢN LÝ SẢN PHẨM ---
        public IActionResult QuanLySanPham() => View(_context.SanPhams.AsNoTracking().ToList());

        [HttpGet]
        public IActionResult ThemSanPham() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSanPham(SanPham model, int slHaNoi, int slDaNang, int slHCM)
        {
            if (ModelState.IsValid)
            {
                model.SoLuong = slHaNoi + slDaNang + slHCM;
                model.MoTa = $"Sản phẩm {model.Name} chính hãng.||LOC:{slHaNoi}-{slDaNang}-{slHCM}||";
                _context.SanPhams.Add(model);
                _context.SaveChanges();
                return RedirectToAction("QuanLySanPham");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult SuaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp == null) return NotFound();

            int hn = 0, dn = 0, hcm = 0;

            if (!string.IsNullOrEmpty(sp.MoTa) && sp.MoTa.Contains("||LOC:"))
            {
                try
                {
                    var parts = sp.MoTa.Split(new[] { "||LOC:", "||" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (part.Contains("-"))
                        {
                            var nums = part.Split('-');
                            if (nums.Length == 3)
                            {
                                int.TryParse(nums[0], out hn);
                                int.TryParse(nums[1], out dn);
                                int.TryParse(nums[2], out hcm);
                            }
                            break;
                        }
                    }
                }
                catch { }
            }

            ViewBag.SlHaNoi = hn;
            ViewBag.SlDaNang = dn;
            ViewBag.SlHCM = hcm;

            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaSanPham(SanPham model, int slHaNoi, int slDaNang, int slHCM)
        {
            var sp = _context.SanPhams.Find(model.Id);
            if (sp != null)
            {
                sp.Name = model.Name;
                sp.Price = model.Price;
                sp.GiaNhap = model.GiaNhap;
                sp.Category = model.Category;
                sp.ImageUrl = model.ImageUrl;
                sp.SoLuong = slHaNoi + slDaNang + slHCM;
                sp.MoTa = $"Sản phẩm {model.Name} chính hãng.||LOC:{slHaNoi}-{slDaNang}-{slHCM}||";

                _context.SaveChanges();
            }
            return RedirectToAction("QuanLySanPham");
        }

        // --- API CẬP NHẬT GIÁ NHẬP NHANH (ASYNC) ---
        [HttpPost]
        public async Task<IActionResult> CapNhatGiaNhapNhanh(int id, decimal giaNhap)
        {
            var sp = await _context.SanPhams.FindAsync(id);

            if (sp != null)
            {
                if (giaNhap < 0) return Json(new { success = false, message = "Giá nhập không được âm" });

                sp.GiaNhap = giaNhap;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
        }

        [HttpPost]
        public IActionResult XoaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp != null) { _context.SanPhams.Remove(sp); _context.SaveChanges(); }
            return RedirectToAction("QuanLySanPham");
        }

        // --- 6. QUẢN LÝ TÀI KHOẢN ---
        public IActionResult QuanLyTaiKhoan() => View(_context.TaiKhoans.AsNoTracking().ToList());
        [HttpGet] public IActionResult ThemTaiKhoan() => View();
        [HttpPost]
        public IActionResult ThemTaiKhoan(TaiKhoan model)
        {
            if (ModelState.IsValid) { _context.TaiKhoans.Add(model); _context.SaveChanges(); return RedirectToAction("QuanLyTaiKhoan"); }
            return View(model);
        }
        [HttpGet]
        public IActionResult SuaTaiKhoan(int id)
        {
            var tk = _context.TaiKhoans.Find(id); return tk == null ? NotFound() : View(tk);
        }
        [HttpPost]
        public IActionResult SuaTaiKhoan(TaiKhoan model)
        {
            ModelState.Remove("Password"); ModelState.Remove("DonHangs");
            if (ModelState.IsValid)
            {
                var tk = _context.TaiKhoans.Find(model.Id);
                if (tk != null) { tk.FullName = model.FullName; tk.Email = model.Email; tk.Role = model.Role; tk.Address = model.Address; tk.PhoneNumber = model.PhoneNumber; _context.SaveChanges(); }
                return RedirectToAction("QuanLyTaiKhoan");
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult XoaTaiKhoan(int id)
        {
            var tk = _context.TaiKhoans.Find(id); if (tk != null) { _context.TaiKhoans.Remove(tk); _context.SaveChanges(); }
            return RedirectToAction("QuanLyTaiKhoan");
        }

        // --- 7. BÁO CÁO NÂNG CAO (Updated) ---
        public IActionResult BaoCao(int? storeId, int? month, int? year)
        {
            var now = DateTime.Now;
            int selectedYear = year ?? now.Year;
            int selectedMonth = month ?? now.Month;

            var user = _context.TaiKhoans.AsNoTracking().FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user.Role == "Admin") storeId = user.StoreId;

            string storeName = "Toàn hệ thống";
            if (storeId == 1) storeName = "Chi nhánh Hà Nội";
            else if (storeId == 2) storeName = "Chi nhánh Đà Nẵng";
            else if (storeId == 3) storeName = "Chi nhánh TP.HCM";

            // 1. Dữ liệu Biểu đồ 3 năm (Giữ nguyên logic cũ)
            List<decimal> GetMonthlyData(int y)
            {
                var data = new List<decimal>();
                for (int m = 1; m <= 12; m++)
                {
                    var revenue = _context.DonHangs
                        .Where(d => d.TrangThai == 3 && d.NgayDat.Value.Year == y && d.NgayDat.Value.Month == m
                                    && (!storeId.HasValue || d.StoreId == storeId))
                        .Sum(d => (decimal?)d.TongTien) ?? 0;
                    data.Add(revenue);
                }
                return data;
            }
            var dataNamHienTai = GetMonthlyData(selectedYear);
            var dataNamTruoc = GetMonthlyData(selectedYear - 1);
            var dataNamKia = GetMonthlyData(selectedYear - 2);

            // 2. Tính KPI Tháng
            var donHangThangQuery = _context.DonHangs
                .Where(d => d.TrangThai == 3
                            && d.NgayDat.Value.Year == selectedYear
                            && d.NgayDat.Value.Month == selectedMonth
                            && (!storeId.HasValue || d.StoreId == storeId));

            decimal tongDoanhThuThuc = donHangThangQuery.Sum(d => d.TongTien); // Tiền thực thu (đã trừ KM)
            int tongDonHang = donHangThangQuery.Count();

            // Lấy chi tiết đơn hàng để tính giá vốn và doanh thu niêm yết
            var chiTietQuery = _context.ChiTietDonHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.DonHang.TrangThai == 3
                             && ct.DonHang.NgayDat.Value.Year == selectedYear
                             && ct.DonHang.NgayDat.Value.Month == selectedMonth
                             && (!storeId.HasValue || ct.DonHang.StoreId == storeId));

            decimal tongDoanhThuNiemYet = chiTietQuery.Sum(ct => (ct.Gia ?? 0) * ct.SoLuong); // Tổng giá bán trên web chưa trừ KM
            decimal tongGiaVon = chiTietQuery.Sum(ct => ct.GiaGoc * ct.SoLuong);

            // Tính Lợi Nhuận Thực = Thực Thu - Giá Vốn
            decimal loiNhuanThuc = tongDoanhThuThuc - tongGiaVon;

            int tongSanPhamBan = chiTietQuery.Sum(ct => (int?)ct.SoLuong) ?? 0;

            // 3. Tính tỷ lệ thực thu (Discount Factor) để phân bổ lợi nhuận cho từng SP
            // Ví dụ: Niêm yết 100tr, Thực thu 80tr -> Tỷ lệ 0.8
            decimal tyLeThucThu = tongDoanhThuNiemYet > 0 ? (tongDoanhThuThuc / tongDoanhThuNiemYet) : 1;

            var baoCaoLoiNhuan = chiTietQuery
                .GroupBy(ct => new { ct.SanPhamId, ct.SanPham.Name, ct.SanPham.ImageUrl })
                .Select(g => new
                {
                    Ten = g.Key.Name,
                    Hinh = g.Key.ImageUrl,
                    Sl = g.Sum(x => x.SoLuong),
                    DtNiemYet = g.Sum(x => (x.Gia ?? 0) * x.SoLuong),
                    Gv = g.Sum(x => x.GiaGoc * x.SoLuong)
                })
                .ToList() // Query về bộ nhớ để tính toán logic phức tạp
                .Select(x => new LoiNhuanSanPham
                {
                    TenSanPham = x.Ten,
                    HinhAnh = x.Hinh,
                    SoLuongBan = x.Sl,
                    DoanhThuNiemYet = x.DtNiemYet,
                    GiaVon = x.Gv,
                    // Phân bổ doanh thu thực theo tỷ lệ
                    DoanhThuThuc = x.DtNiemYet * tyLeThucThu,
                    // Lợi nhuận = DoanhThuThuc - GiáVốn
                    LoiNhuan = (x.DtNiemYet * tyLeThucThu) - x.Gv
                })
                .OrderByDescending(x => x.LoiNhuan)
                .ToList();

            // 4. [MỚI] KHÁCH HÀNG TIỀM NĂNG (VIP USER)
            // Logic: Role là User + Tổng chi tiêu (cả lịch sử) >= 100tr
            var vipUsers = _context.TaiKhoans
                .Where(u => u.Role == "User") // Chỉ lấy khách thường
                .Select(u => new
                {
                    User = u,
                    TotalSpent = u.DonHangs.Where(d => d.TrangThai == 3).Sum(d => (decimal?)d.TongTien) ?? 0,
                    OrderCount = u.DonHangs.Count(d => d.TrangThai == 3)
                })
                .Where(x => x.TotalSpent >= 100000000) // Điều kiện Kim Cương
                .OrderByDescending(x => x.TotalSpent)
                .Take(10) // Lấy top 10
                .Select(x => new KhachHangTiemNang
                {
                    Id = x.User.Id,
                    HoTen = x.User.FullName,
                    Username = x.User.Username,
                    SoDienThoai = x.User.PhoneNumber,
                    TongChiTieu = x.TotalSpent,
                    SoDonHang = x.OrderCount
                })
                .ToList();

            // Đóng gói ViewModel
            var model = new BaoCaoViewModel
            {
                TongDoanhThu = tongDoanhThuThuc,
                TongDoanhThuGoc = tongDoanhThuNiemYet,
                TongDonHang = tongDonHang,
                TongSanPhamDaBan = tongSanPhamBan,
                LoiNhuanUocTinh = loiNhuanThuc,

                DataNamHienTai = dataNamHienTai,
                DataNamTruoc = dataNamTruoc,
                DataNamKia = dataNamKia,
                CurrentYear = selectedYear,

                SelectedStoreId = storeId,
                StoreName = storeName,

                BaoCaoLoiNhuan = baoCaoLoiNhuan,
                KhachHangTiemNangs = vipUsers // Dữ liệu mới
            };

            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.IsBoss = (user.Role == "Boss");

            return View(model);
        }

        // --- 8. XUẤT EXCEL & IN HÓA ĐƠN ---
        [HttpGet]
        public IActionResult XuatExcelDonHang()
        {
            var orders = _context.DonHangs
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Mã đơn hàng,Ngày đặt,Khách hàng,Số điện thoại,Địa chỉ giao hàng,Tổng tiền,Trạng thái");

            foreach (var item in orders)
            {
                string trangThaiText = item.TrangThai switch
                {
                    0 => "Chờ xử lý",
                    1 => "Đã xác nhận",
                    2 => "Đang giao",
                    3 => "Hoàn thành",
                    -1 => "Đã hủy",
                    _ => "Khác"
                };

                string diaChiSafe = item.DiaChi?.Replace("\"", "\"\"") ?? "";
                string ngayDatStr = item.NgayDat.HasValue ? item.NgayDat.Value.ToString("dd/MM/yyyy HH:mm") : "";

                sb.AppendLine($"{item.MaDon},{ngayDatStr},{item.NguoiNhan},{item.SDT},\"{diaChiSafe}\",{item.TongTien},{trangThaiText}");
            }

            var fileContent = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(fileContent, "text/csv", $"BaoCaoDonHang_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        [HttpGet]
        public IActionResult InHoaDon(string orderId)
        {
            var order = _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .Include(d => d.TaiKhoan)
                .FirstOrDefault(d => d.MaDon == orderId);

            if (order == null) return NotFound();

            string shopAddress = "120 Xuân Thủy, Cầu Giấy, Hà Nội";
            string zoneCode = "HN-CG-01";

            string addr = (order.DiaChi ?? "").ToLower();
            string[] mienTrung = { "đà nẵng", "huế", "quảng nam", "quảng ngãi", "bình định", "phú yên", "khánh hòa", "quảng bình", "quảng trị", "nghệ an", "hà tĩnh", "thanh hóa" };
            string[] mienNam = { "hồ chí minh", "tp.hcm", "hcm", "sài gòn", "bình dương", "đồng nai", "bà rịa", "vũng tàu", "long an", "tiền giang", "cần thơ" };

            if (mienNam.Any(k => addr.Contains(k))) { shopAddress = "55 Nguyễn Huệ, Quận 1, TP.HCM"; zoneCode = "SG-Q1-03"; }
            else if (mienTrung.Any(k => addr.Contains(k))) { shopAddress = "78 Bạch Đằng, Hải Châu, Đà Nẵng"; zoneCode = "DN-HC-02"; }

            ViewBag.ShopAddress = shopAddress;
            ViewBag.ZoneCode = zoneCode;

            var model = new DonHangViewModel
            {
                MaDon = order.MaDon,
                NgayDat = order.NgayDat,
                TenKhachHang = !string.IsNullOrEmpty(order.NguoiNhan) ? order.NguoiNhan : (order.TaiKhoan?.FullName ?? "Khách lẻ"),
                SoDienThoai = !string.IsNullOrEmpty(order.SDT) ? order.SDT : (order.TaiKhoan?.PhoneNumber ?? ""),
                DiaChiGiaoHang = order.DiaChi ?? "",
                TongTien = order.TongTien,
                PhuongThucThanhToan = "Thanh toán khi nhận hàng (COD)",
                SanPhams = order.ChiTietDonHangs.Select(ct => new DonHangViewModel
                {
                    TenSanPham = ct.SanPham.Name,
                    SoLuong = ct.SoLuong,
                    Gia = ct.Gia
                }).ToList()
            };

            return View(model);
        }

        // --- 9. QUẢN LÝ THU CŨ ĐỔI MỚI (MỚI) ---
        public IActionResult QuanLyThuMua()
        {
            var listYeuCau = _context.YeuCauThuMuas
                .Include(y => y.TaiKhoan)
                .OrderByDescending(y => y.NgayTao)
                .ToList();
            return View(listYeuCau);
        }

        [HttpPost]
        public IActionResult CapNhatThuMua(int id, int trangThai, string ghiChuAdmin)
        {
            var yeuCau = _context.YeuCauThuMuas.Find(id);
            if (yeuCau != null)
            {
                if (yeuCau.TrangThai == 2 || yeuCau.TrangThai == -1)
                {
                    TempData["Error"] = "Yêu cầu này đã hoàn tất hoặc đã hủy, không thể thay đổi trạng thái nữa!";
                    return RedirectToAction("QuanLyThuMua");
                }

                if (trangThai == 2 || trangThai == -1)
                {
                    if (!string.IsNullOrEmpty(ghiChuAdmin))
                    {
                        yeuCau.GhiChu = ghiChuAdmin;
                    }
                }
                else if (trangThai == 1 && string.IsNullOrEmpty(yeuCau.GhiChu) && !string.IsNullOrEmpty(ghiChuAdmin))
                {
                    yeuCau.GhiChu = ghiChuAdmin;
                }

                yeuCau.TrangThai = trangThai;

                _context.SaveChanges();
                TempData["Success"] = "Đã cập nhật trạng thái yêu cầu.";
            }
            return RedirectToAction("QuanLyThuMua");
        }

        // --- 10. QUẢN LÝ ĐÁNH GIÁ (MỚI) ---
        public IActionResult QuanLyDanhGia()
        {
            var listDanhGia = _context.DanhGias
                .Include(d => d.SanPham)
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayTao)
                .ToList();

            return View(listDanhGia);
        }

        [HttpPost]
        public IActionResult DuyetDanhGia(int id)
        {
            var danhGia = _context.DanhGias.Find(id);
            if (danhGia != null)
            {
                danhGia.DaDuyet = !danhGia.DaDuyet;
                _context.SaveChanges();
                return Json(new { success = true, newStatus = danhGia.DaDuyet });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public IActionResult TraLoiDanhGia(int id, string noiDungTraLoi)
        {
            var danhGia = _context.DanhGias.Find(id);
            if (danhGia != null)
            {
                danhGia.TraLoi = noiDungTraLoi;
                danhGia.NgayTraLoi = DateTime.Now;
                _context.SaveChanges();
                TempData["Success"] = "Đã trả lời đánh giá thành công!";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy đánh giá.";
            }
            return RedirectToAction("QuanLyDanhGia");
        }

        [HttpPost]
        public IActionResult XoaDanhGia(int id)
        {
            var danhGia = _context.DanhGias.Find(id);
            if (danhGia != null)
            {
                _context.DanhGias.Remove(danhGia);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa đánh giá.";
            }
            return RedirectToAction("QuanLyDanhGia");
        }
    }
}