using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using banthietbidientu.Data;
using banthietbidientu.Models;
using System.Collections.Generic;
using System;
using System.Text;

namespace banthietbidientu.Controllers
{
    [Authorize(Roles = "Admin")]
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
            var tongDoanhThu = _context.DonHangs.Where(d => d.TrangThai == 3).Sum(x => x.TongTien);
            var donHangMoi = _context.DonHangs.Count(x => x.NgayDat.Value.Date == DateTime.Today);
            var tongKhachHang = _context.TaiKhoans.Count(x => x.Role == "User");
            var sapHetHang = _context.SanPhams.Count(x => x.SoLuong < 5);

            var sttHoanThanh = _context.DonHangs.Count(x => x.TrangThai == 3);
            var sttDangGiao = _context.DonHangs.Count(x => x.TrangThai == 2);
            var sttChoXuLy = _context.DonHangs.Count(x => x.TrangThai == 0 || x.TrangThai == 1);
            var sttDaHuy = _context.DonHangs.Count(x => x.TrangThai == -1);

            ViewBag.ChartData = new List<int> { sttHoanThanh, sttDangGiao, sttChoXuLy, sttDaHuy };

            var revenueWeek = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                var total = _context.DonHangs
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

        // --- 2. QUẢN LÝ ĐƠN HÀNG ---
        public IActionResult QuanLyDonHang(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var query = _context.DonHangs
                .Include(d => d.TaiKhoan)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking();

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

            // --- 1. TÍNH GIÁ VỐN BÌNH QUÂN GIA QUYỀN (Moving Average) ---
            decimal tongGiaTriCu = sanPham.SoLuong * sanPham.GiaNhap;
            decimal tongGiaTriNhap = soLuong * giaNhap;
            int tongSoLuongMoi = sanPham.SoLuong + soLuong;

            if (tongSoLuongMoi > 0)
            {
                // Cập nhật giá nhập mới cho sản phẩm
                sanPham.GiaNhap = (tongGiaTriCu + tongGiaTriNhap) / tongSoLuongMoi;
            }

            // Cập nhật số lượng tồn kho
            sanPham.SoLuong += soLuong;

            // --- 2. LƯU PHIẾU NHẬP ---
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

            // Lưu tất cả thay đổi (bao gồm giá nhập mới của sản phẩm)
            _context.SaveChanges();

            TempData["Success"] = $"Đã nhập kho {soLuong} {sanPham.Name}. Giá vốn mới: {sanPham.GiaNhap:N0}đ";
            return RedirectToAction("QuanLyNhapHang");
        }

        // --- 4. API THÔNG BÁO (ĐÃ UPDATE CHO HIGHLIGHT) ---
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
                        // Thêm 2 trường này để frontend điều hướng
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
                sp.Category = model.Category;
                sp.ImageUrl = model.ImageUrl;
                sp.SoLuong = slHaNoi + slDaNang + slHCM;
                sp.MoTa = $"Sản phẩm {model.Name} chính hãng.||LOC:{slHaNoi}-{slDaNang}-{slHCM}||";

                _context.SaveChanges();
            }
            return RedirectToAction("QuanLySanPham");
        }

        [HttpPost]
        public IActionResult XoaSanPham(int id)
        {
            var sp = _context.SanPhams.Find(id);
            if (sp != null) { _context.SanPhams.Remove(sp); _context.SaveChanges(); }
            return RedirectToAction("QuanLySanPham");
        }

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

        // --- 6. XUẤT EXCEL & IN HÓA ĐƠN ---
        [HttpGet]
        public IActionResult XuatExcelDonHang()
        {
            var orders = _context.DonHangs
                .Include(d => d.TaiKhoan)
                .OrderByDescending(d => d.NgayDat)
                .ToList();

            var sb = new System.Text.StringBuilder();
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

            var fileContent = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

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
            string[] mienTrung = {
                "đà nẵng", "thừa thiên huế", "huế", "quảng nam", "quảng ngãi", "bình định",
                "phú yên", "khánh hòa", "quảng bình", "quảng trị", "nghệ an", "hà tĩnh", "thanh hóa",
                "ninh thuận", "bình thuận", "kon tum", "gia lai", "đắk lắk", "đắc lắc", "đắk nông", "lâm đồng", "đà lạt",
                "da nang", "hue", "quang nam", "quang ngai", "binh dinh", "phu yen", "khanh hoa",
                "quang binh", "quang tri", "nghe an", "ha tinh", "thanh hoa",
                "ninh thuan", "binh thuan", "kon tum", "gia lai", "dak lak", "dak nong", "lam dong", "da lat"
            };

            string[] mienNam = {
                "hồ chí minh", "tp.hcm", "hcm", "sài gòn", "bình dương", "đồng nai", "bà rịa", "vũng tàu",
                "long an", "tiền giang", "bến tre", "trà vinh", "vĩnh long", "đồng tháp", "an giang",
                "kiên giang", "cần thơ", "hậu giang", "sóc trăng", "bạc liêu", "cà mau", "tây ninh", "bình phước",
                "ho chi minh", "sai gon", "binh duong", "dong nai", "ba ria", "vung tau",
                "long an", "tien giang", "ben tre", "tra vinh", "vinh long", "dong thap", "an giang",
                "kien giang", "can tho", "hau giang", "soc trang", "bac lieu", "ca mau", "tay ninh", "binh phuoc"
            };

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

        // --- 7. BÁO CÁO THỐNG KÊ ---
        public IActionResult BaoCao()
        {
            // 1. Số liệu tổng quan
            var tongDoanhThu = _context.DonHangs.Where(d => d.TrangThai == 3).Sum(d => d.TongTien);
            var tongDonHang = _context.DonHangs.Count();
            var sanPhamSapHet = _context.SanPhams.Count(s => s.SoLuong < 5);
            var tongSanPhamDaBan = _context.ChiTietDonHangs.Where(ct => ct.DonHang.TrangThai == 3).Sum(ct => ct.SoLuong);

            // 2. Biểu đồ 7 ngày
            var labelsNgay = new List<string>();
            var valuesDoanhThu = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                labelsNgay.Add(date.ToString("dd/MM"));
                var revenue = _context.DonHangs
                    .Where(d => d.TrangThai == 3 && d.NgayDat.HasValue && d.NgayDat.Value.Date == date)
                    .Sum(d => d.TongTien);
                valuesDoanhThu.Add(revenue);
            }

            // 3. Top sản phẩm bán chạy (theo số lượng)
            var topProducts = _context.ChiTietDonHangs
                .Where(ct => ct.DonHang.TrangThai == 3)
                .GroupBy(ct => ct.SanPham.Name)
                .Select(g => new { Name = g.Key, Count = g.Sum(x => x.SoLuong) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // 4. Top khách hàng
            var topUsers = _context.DonHangs
                .Where(d => d.TrangThai == 3 && d.TaiKhoanId != null)
                .GroupBy(d => d.TaiKhoan)
                .Select(g => new TopKhachHang
                {
                    HoTen = g.Key.FullName ?? "Ẩn danh",
                    Email = g.Key.Email ?? "---",
                    Role = g.Key.Role,
                    SoLanMua = g.Count(),
                    TongChiTieu = g.Sum(x => x.TongTien)
                })
                .OrderByDescending(x => x.TongChiTieu)
                .Take(5)
                .ToList();

            // 5. --- TÍNH TOÁN LỢI NHUẬN CHI TIẾT TỪNG SẢN PHẨM ---
            // Chỉ lấy các đơn hàng đã Hoàn thành (TrangThai == 3)
            var baoCaoLoiNhuan = _context.ChiTietDonHangs
                .Include(ct => ct.SanPham)
                .Where(ct => ct.DonHang.TrangThai == 3)
                .GroupBy(ct => new { ct.SanPhamId, ct.SanPham.Name, ct.SanPham.ImageUrl })
                .Select(g => new LoiNhuanSanPham
                {
                    TenSanPham = g.Key.Name,
                    HinhAnh = g.Key.ImageUrl,
                    SoLuongBan = g.Sum(x => x.SoLuong),

                    // Doanh thu = Tổng (Giá bán * Số lượng)
                    DoanhThu = g.Sum(x => (x.Gia ?? 0) * x.SoLuong),

                    // Giá vốn = Tổng (Giá gốc lúc bán * Số lượng)
                    GiaVon = g.Sum(x => x.GiaGoc * x.SoLuong),

                    // Lợi nhuận = Doanh thu - Giá vốn
                    LoiNhuan = g.Sum(x => ((x.Gia ?? 0) - x.GiaGoc) * x.SoLuong)
                })
                .OrderByDescending(x => x.LoiNhuan) // Sắp xếp theo lợi nhuận cao nhất
                .ToList();

            var model = new BaoCaoViewModel
            {
                TongDoanhThu = tongDoanhThu,
                TongSanPhamDaBan = tongSanPhamDaBan,
                TongDonHang = tongDonHang,
                SanPhamSapHet = sanPhamSapHet,
                LabelsNgay = labelsNgay,
                ValuesDoanhThu = valuesDoanhThu,
                TopSanPhamTen = topProducts.Select(p => p.Name).ToList(),
                TopSanPhamSoLuong = topProducts.Select(p => p.Count).ToList(),
                TopKhachHangs = topUsers,
                BaoCaoLoiNhuan = baoCaoLoiNhuan // Gán dữ liệu vào ViewModel
            };

            return View(model);
        }

        // --- 8. QUẢN LÝ ĐÁNH GIÁ (MỚI) ---
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

        // --- 9. QUẢN LÝ THU CŨ ĐỔI MỚI (MỚI) ---
        public IActionResult QuanLyThuMua()
        {
            var listYeuCau = _context.YeuCauThuMuas
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

                yeuCau.TrangThai = trangThai;

                if (!string.IsNullOrEmpty(ghiChuAdmin))
                {
                    yeuCau.GhiChu = ghiChuAdmin;
                }

                _context.SaveChanges();
                TempData["Success"] = "Đã cập nhật trạng thái yêu cầu.";
            }
            return RedirectToAction("QuanLyThuMua");
        }
    }
}