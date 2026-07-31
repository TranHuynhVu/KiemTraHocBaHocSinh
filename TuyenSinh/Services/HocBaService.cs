using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Hangfire;
using System.Globalization;
using TuyenSinh.Data;
using TuyenSinh.ViewModels;
using TuyenSinh.Enums;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public class HocBaService : IHocBaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HocBaService(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<(bool Success, string Message, string? ExcelId, List<HocBaPreviewItem>? PreviewData)> UploadAndPreviewAsync(IFormFile file)
        {
            string excelId;
            try
            {
                excelId = await LuuFileTamThoiAsync(file);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null, null);
            }

            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, "uploads", excelId);


            try
            {
                var previewList = ParseExcelToList(filePath, 100);

                var previewData = previewList.Select(r => new HocBaPreviewItem
                {
                    Stt = r.STT,
                    SoDDCN = r.SoDDCN,
                    HoVaTen = r.HoVaTen,
                    NgaySinh = r.NgaySinh?.ToString("dd/MM/yyyy"),
                    GioiTinh = r.GioiTinh,
                    Lop = r.Lop,
                    ChuongTrinhHoc = r.ChuongTrinhHoc,
                    DiemTrungBinhNam = r.DiemTrungBinhNam,
                    ToanCN = r.ToanCN,
                    VanCN = r.VanCN,
                    VatLyCN = r.VatLyCN,
                    HoaHocCN = r.HoaHocCN,
                    SinhHocCN = r.SinhHocCN,
                    NgoaiNguCN = r.NgoaiNguCN
                }).ToList();

                _backgroundJobClient.Schedule<IHocBaService>(s => s.DeleteExpiredFileAsync(excelId), TimeSpan.FromMinutes(30));

                return (true, "Tải lên và đọc tập thành công.", excelId, previewData);
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return (false, "Lỗi khi đọc tập Excel: " + ex.Message, null, null);
            }
        }

        public async Task<List<HocBaTHPTImport>?> GetPreviewDataAsync(string excelId, int? limit = null)
        {
            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            var filePath = Path.Combine(uploadsFolder, excelId);

            if (!System.IO.File.Exists(filePath))
            {
                return null;
            }

            try
            {
                return ParseExcelToList(filePath, limit);
            }
            catch
            {
                return null;
            }
        }

        public async Task DeleteExpiredFileAsync(string excelId)
        {
            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            var filePath = Path.Combine(uploadsFolder, excelId);

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch { }
            }
        }

        public async Task<KetQuaKiemTraHocBa> CheckHocBaAsync(string excelId)
        {
            var result = new KetQuaKiemTraHocBa();

            if (string.IsNullOrEmpty(excelId))
            {
                result.ThanhCong = false;
                result.ThongBao = "Không tìm thấy mã tập Excel.";
                return result;
            }

            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            var filePath = Path.Combine(uploadsFolder, excelId);

            if (!System.IO.File.Exists(filePath))
            {
                result.ThanhCong = false;
                result.ThongBao = "Tập Excel không tồn tại trên hệ thống.";
                return result;
            }

            List<HocBaTHPTImport> records;
            try
            {
                records = ParseExcelToList(filePath);
            }
            catch (Exception ex)
            {
                result.ThanhCong = false;
                result.ThongBao = "Có lỗi xảy ra khi đọc tập Excel: " + ex.Message;
                return result;
            }

            var grouped = records.GroupBy(r => r.SoDDCN).ToList();

            var baoCaoThieuNamHoc = new List<BaoCaoThieuNamHocItem>();
            var baoCaoThieuDiem = new List<BaoCaoThieuDiemItem>();

            var danhSachToHop = await _context.ToHopMons.Include(t => t.MonHocs).ToListAsync();

            int thieuNamHocStt = 1;
            int thieuDiemStt = 1;

            foreach (var group in grouped)
            {
                var cccd = group.Key;
                var firstRecord = group.First();
                var name = firstRecord.HoVaTen;

                // Check 1: Missing years (10, 11, 12)
                var cacLop = group.Select(r => r.Lop).Where(l => l.HasValue).Select(l => l!.Value).Distinct().OrderBy(g => g).ToList();
                bool has10 = cacLop.Contains(10);
                bool has11 = cacLop.Contains(11);
                bool has12 = cacLop.Contains(12);

                if (!has10 || !has11 || !has12)
                {
                    var cacLopHienCo = string.Join(", ", cacLop);
                    var danhSachLopThieu = new List<string>();
                    if (!has10) danhSachLopThieu.Add("Lớp 10");
                    if (!has11) danhSachLopThieu.Add("Lớp 11");
                    if (!has12) danhSachLopThieu.Add("Lớp 12");
                    var namThieu = string.Join(", ", danhSachLopThieu);

                    baoCaoThieuNamHoc.Add(new BaoCaoThieuNamHocItem
                    {
                        Stt = thieuNamHocStt++,
                        Cccd = cccd,
                        HoVaTen = name,
                        NamHienCo = cacLopHienCo,
                        NamThieu = namThieu
                    });
                }

                // Check 2: Missing subject scores based on combinations
                foreach (var gradeRecord in group)
                {
                    int currentGrade = gradeRecord.Lop ?? 0;
                    if (currentGrade != 10 && currentGrade != 11 && currentGrade != 12) continue;

                    foreach (var toHop  in danhSachToHop)
                    {
                        var cacMonThieuTrongToHop = new List<string>();

                        foreach (var subject in toHop.MonHocs)
                        {
                            var score = GetScore(gradeRecord, subject.FieldName);
                            if (score == null)
                            {
                                var displayName = LayTenHienThiMonHocCN(subject.FieldName);
                                cacMonThieuTrongToHop.Add(displayName);
                            }
                        }

                        if (cacMonThieuTrongToHop.Count > 0)
                        {
                            baoCaoThieuDiem.Add(new BaoCaoThieuDiemItem
                            {
                                Stt = thieuDiemStt++,
                                Cccd = cccd,
                                HoVaTen = name,
                                NamLoi = "Lớp " + currentGrade,
                                ToHop = toHop.MaToHop,
                                MonThieu = string.Join(", ", cacMonThieuTrongToHop)
                            });
                        }
                    }
                }
            }

            result.ThanhCong = true;
            result.DanhSachThieuNamHoc = baoCaoThieuNamHoc;
            result.DanhSachThieuDiem = baoCaoThieuDiem;

            return result;
        }

        public async Task<string> LuuFileTamThoiAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Tệp tin trống.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                throw new ArgumentException("Chỉ chấp nhận tệp tin Excel định dạng .xlsx.");
            }
            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileId = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileId);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _backgroundJobClient.Schedule<IHocBaService>(s => s.DeleteExpiredFileAsync(fileId), TimeSpan.FromMinutes(30));

            return fileId;
        }

        public async Task<KetQuaDoiChieu> DoiChieuHocBaVaNguyenVongAsync(string hocBaFileId, string nguyenVongFileId)
        {
            var ketQua = new KetQuaDoiChieu();
            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");

            var fileHocBaPath = Path.Combine(uploadsFolder, hocBaFileId);
            var fileNguyenVongPath = Path.Combine(uploadsFolder, nguyenVongFileId);

            if (!System.IO.File.Exists(fileHocBaPath))
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "File học bạ không tồn tại hoặc đã hết hạn.";
                return ketQua;
            }

            if (!System.IO.File.Exists(fileNguyenVongPath))
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "File nguyện vọng không tồn tại hoặc đã hết hạn.";
                return ketQua;
            }

            // 1. Đọc file học bạ vào bộ nhớ tạm
            var hbStart = DateTime.Now;
            List<HocBaTHPTImport> danhSachHocBa;
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var msHB = new MemoryStream(System.IO.File.ReadAllBytes(fileHocBaPath));
                using var pkgHB = new ExcelPackage(msHB);
                var sheetHB = pkgHB.Workbook.Worksheets[0];
                int totalRowsHB = sheetHB.Dimension.End.Row;

                danhSachHocBa = new List<HocBaTHPTImport>();
                for (int r = 4; r <= totalRowsHB; r++)
                {
                    var item = new HocBaTHPTImport
                    {
                        STT = ParseInt(sheetHB.Cells[r, 1].Value),
                        SoDDCN = ParseString(sheetHB.Cells[r, 2].Value),
                        HoVaTen = ParseString(sheetHB.Cells[r, 3].Value),
                        Lop = ParseInt(sheetHB.Cells[r, 6].Value),
                        ToanCN = ParseDecimal(sheetHB.Cells[r, 26].Value),
                        VanCN = ParseDecimal(sheetHB.Cells[r, 29].Value),
                        VatLyCN = ParseDecimal(sheetHB.Cells[r, 32].Value),
                        HoaHocCN = ParseDecimal(sheetHB.Cells[r, 35].Value),
                        SinhHocCN = ParseDecimal(sheetHB.Cells[r, 38].Value),
                        LichSuCN = ParseDecimal(sheetHB.Cells[r, 41].Value),
                        DiaLyCN = ParseDecimal(sheetHB.Cells[r, 44].Value),
                        GDCDCN = ParseDecimal(sheetHB.Cells[r, 47].Value),
                        KTPLCN = ParseDecimal(sheetHB.Cells[r, 50].Value),
                        TinHocCN = ParseDecimal(sheetHB.Cells[r, 53].Value),
                        CNCNCN = ParseDecimal(sheetHB.Cells[r, 56].Value),
                        CNNNCN = ParseDecimal(sheetHB.Cells[r, 59].Value),
                        NgoaiNguCN = ParseDecimal(sheetHB.Cells[r, 62].Value),
                        MonNgoaiNgu = ParseString(sheetHB.Cells[r, 63].Value),
                        TuChonSongNguCN = ParseDecimal(sheetHB.Cells[r, 66].Value),
                        QPANCN = ParseDecimal(sheetHB.Cells[r, 69].Value),
                        TiengDanTocCN = ParseDecimal(sheetHB.Cells[r, 72].Value),
                        NgoaiNgu2CN = ParseDecimal(sheetHB.Cells[r, 75].Value),
                        ToanPhapCN = ParseDecimal(sheetHB.Cells[r, 79].Value),
                    };
                    if (!string.IsNullOrWhiteSpace(item.SoDDCN) && !string.IsNullOrWhiteSpace(item.HoVaTen))
                        danhSachHocBa.Add(item);
                }              
            }
            catch (Exception ex)
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "Lỗi khi đọc file học bạ: " + ex.Message;
                return ketQua;
            }

            // Group học bạ theo CCCD, lấy từng năm 10, 11, 12
            var hocBaTheoCccd = danhSachHocBa
                .GroupBy(r => r.SoDDCN)
                .ToDictionary(g => g.Key!, g => g.ToList());

            // 2. Đọc file nguyện vọng (header ở Row 5)
            var nvStart = DateTime.Now;
            List<NguyenVongItem> danhSachNV;
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var msNV = new MemoryStream(System.IO.File.ReadAllBytes(fileNguyenVongPath));
                using var pkgNV = new ExcelPackage(msNV);
                var sheetNV = pkgNV.Workbook.Worksheets[0];
                int totalRowsNV = sheetNV.Dimension.End.Row;
                
                danhSachNV = new List<NguyenVongItem>();
                for (int r = 6; r <= totalRowsNV; r++)
                {
                    var cccd = ParseString(sheetNV.Cells[r, 2].Value);
                    var thuTuNV = ParseInt(sheetNV.Cells[r, 3].Value) ?? 0;
                    var maXetTuyen = ParseString(sheetNV.Cells[r, 6].Value);
                    var tenNganh = ParseString(sheetNV.Cells[r, 7].Value);
                    if (!string.IsNullOrWhiteSpace(cccd) && !string.IsNullOrWhiteSpace(maXetTuyen))
                    {
                        danhSachNV.Add(new NguyenVongItem
                        {
                            SoDDCN = cccd,
                            ThuTuNV = thuTuNV,
                            MaXetTuyen = maXetTuyen,
                            TenNganh = tenNganh
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "Lỗi khi đọc file nguyện vọng: " + ex.Message;
                return ketQua;
            }

            ketQua.TongNguyenVong = danhSachNV.Count;

            // 3. Load toàn bộ Nganh + ToHopNganh + ToHopMon + MonHocs 
            var dbStart = DateTime.Now;
            var danhSachNganh = await _context.Nganhs
                .AsNoTracking()
                .Include(n => n.ToHopNganhs)
                    .ThenInclude(th => th.ToHopMon)
                        .ThenInclude(t => t.MonHocs)
                .ToListAsync();

            // Chuyển sang Dictionary để tìm kiếm 
            var nganhDict = danhSachNganh
                .Where(n => !string.IsNullOrEmpty(n.MaNganh))
                .ToDictionary(n => n.MaNganh!.Trim(), n => n, StringComparer.OrdinalIgnoreCase);

            // 4. Xử lý từng nguyện vọng
            var maNganhKhongTimThay = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var danhSachTam = new List<KetQuaDoiChieuItem>();
            var thiSinhKhongHocBa = new HashSet<string>();
            var listNVThongKe = new List<(string Cccd, int ThuTuNV, string MaNganh, string TenNganh, string Loai)>();

            foreach (var nv in danhSachNV)
            {
                var cccd = nv.SoDDCN!;
                var thuTuNV = nv.ThuTuNV;
                var maNganh = nv.MaXetTuyen!.Trim();
                var tenNganh = nv.TenNganh ?? string.Empty;

                // 1. Kiểm tra mã ngành trong DB
                if (nganhDict.TryGetValue(maNganh, out var nganhEntity))
                {
                    if (!string.IsNullOrEmpty(nganhEntity.TenNganh))
                        tenNganh = nganhEntity.TenNganh;

                    // Nếu ngành có HeSoHB == 0 (chỉ xét điểm THPT) -> bỏ qua đối chiếu học bạ
                    if (nganhEntity.HeSoHB == 0)
                    {
                        listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "BoQua"));
                        continue;
                    }
                }
                else
                {
                    maNganhKhongTimThay.Add(maNganh);
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongDiemCN"));
                    continue;
                }

                // 2. Kiểm tra thí sinh có trong file học bạ không
                if (!hocBaTheoCccd.TryGetValue(cccd, out var hocBaThiSinh))
                {
                    thiSinhKhongHocBa.Add(cccd);
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongHocBa"));
                    continue;
                }

                var hoVaTen = hocBaThiSinh.FirstOrDefault()?.HoVaTen ?? string.Empty;
                var lopHB10 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 10);
                var lopHB11 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 11);
                var lopHB12 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 12);
                var cacNamRecord = new[] {
                    (Nam: "Lớp 10", Record: lopHB10),
                    (Nam: "Lớp 11", Record: lopHB11),
                    (Nam: "Lớp 12", Record: lopHB12)
                };

                if (nganhEntity.ToHopNganhs == null || !nganhEntity.ToHopNganhs.Any())
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongDiemCN"));
                    continue;
                }

                // 3. Kiểm tra các tổ hợp môn của ngành
                bool hasAnyValidToHop = false;
                bool hasAnyScoreData = false;

                foreach (var toHopNganh in nganhEntity.ToHopNganhs)
                {
                    var toHop = toHopNganh.ToHopMon;
                    if (toHop == null || toHop.MonHocs == null || !toHop.MonHocs.Any()) continue;

                    bool toHopDu = true;
                    foreach (var (namHoc, record) in cacNamRecord)
                    {
                        var cacMonThieuTrongNam = new List<string>();
                        foreach (var monHoc in toHop.MonHocs)
                        {
                            decimal? diem = record == null ? null : GetScore(record, monHoc.FieldName);
                            if (diem != null)
                            {
                                hasAnyScoreData = true;
                            }
                            else
                            {
                                toHopDu = false;
                                cacMonThieuTrongNam.Add(LayTenHienThiMonHocCN(monHoc.FieldName));
                            }
                        }

                        if (cacMonThieuTrongNam.Count > 0)
                        {
                            danhSachTam.Add(new KetQuaDoiChieuItem
                            {
                                SoDDCN = cccd,
                                HoVaTen = hoVaTen,
                                ThuTuNV = thuTuNV,
                                MaNganh = nganhEntity.MaNganh,
                                TenNganh = nganhEntity.TenNganh,
                                MaToHop = toHop.MaToHop,
                                NamHoc = namHoc,
                                MonThieu = string.Join(", ", cacMonThieuTrongNam)
                            });
                        }
                    }

                    if (toHopDu)
                    {
                        hasAnyValidToHop = true;
                    }
                }

                if (hasAnyValidToHop)
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "CoToHopDu"));
                }
                else if (!hasAnyScoreData)
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongDiemCN"));
                }
                else
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "ThieuMoiToHop"));
                }
            }

            var ketQuaThieuDiem = danhSachTam
                .GroupBy(x => new { x.SoDDCN, x.HoVaTen, x.ThuTuNV, x.MaNganh, x.TenNganh, x.MaToHop, x.MonThieu })
                .Select(g => new KetQuaDoiChieuItem
                {
                    SoDDCN = g.Key.SoDDCN,
                    HoVaTen = g.Key.HoVaTen,
                    ThuTuNV = g.Key.ThuTuNV,
                    MaNganh = g.Key.MaNganh,
                    TenNganh = g.Key.TenNganh,
                    MaToHop = g.Key.MaToHop,
                    NamHoc = string.Join(", ", g.Select(x => x.NamHoc)),
                    MonThieu = g.Key.MonThieu
                })
                .OrderBy(x => x.SoDDCN)
                .ThenBy(x => x.ThuTuNV)
                .ToList();

            var thongKeTongHop = new ThongKeTongHopViewModel
            {
                TongDongNguyenVong = listNVThongKe.Count,
                TongThiSinhDuyNhat = listNVThongKe.Select(x => x.Cccd).Distinct().Count(),
                NguyenVongCoToHopDu = listNVThongKe.Count(x => x.Loai == "CoToHopDu"),
                NguyenVongThieuMoiToHop = listNVThongKe.Count(x => x.Loai == "ThieuMoiToHop"),
                NguyenVongKhongHocBa = listNVThongKe.Count(x => x.Loai == "KhongHocBa"),
                NguyenVongKhongDiemCN = listNVThongKe.Count(x => x.Loai == "KhongDiemCN"),
                NguyenVongBoQua = listNVThongKe.Count(x => x.Loai == "BoQua"),

                DanhSachThieuMoiToHop = listNVThongKe.Where(x => x.Loai == "ThieuMoiToHop")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                DanhSachKhongHocBa = listNVThongKe.Where(x => x.Loai == "KhongHocBa")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                DanhSachKhongDiemCN = listNVThongKe.Where(x => x.Loai == "KhongDiemCN")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                DanhSachBoQua = listNVThongKe.Where(x => x.Loai == "BoQua")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList()
            };

            var thongKeTheoNganh = listNVThongKe
                .GroupBy(x => new { x.MaNganh, x.TenNganh })
                .Select(g =>
                {
                    int tongNV = g.Count();
                    int soThiSinh = g.Select(x => x.Cccd).Distinct().Count();
                    int nvCoToHopDu = g.Count(x => x.Loai == "CoToHopDu");
                    int nvThieuMoiToHop = g.Count(x => x.Loai == "ThieuMoiToHop");
                    int nvKhongDiemCN = g.Count(x => x.Loai == "KhongDiemCN");
                    int nvKhongHocBa = g.Count(x => x.Loai == "KhongHocBa");
                    int nvBoQua = g.Count(x => x.Loai == "BoQua");
                    int tongThieu = nvThieuMoiToHop + nvKhongDiemCN + nvKhongHocBa;
                    double tyLeThieu = tongNV > 0 ? Math.Round((double)tongThieu / tongNV * 100, 2) : 0;

                    return new ThongKeTheoNganhItemViewModel
                    {
                        MaXetTuyen = g.Key.MaNganh,
                        TenNganh = g.Key.TenNganh,
                        TongNV = tongNV,
                        SoThiSinh = soThiSinh,
                        NVCoToHopDu = nvCoToHopDu,
                        NVThieuMoiToHop = nvThieuMoiToHop,
                        NVKhongDiemCN = nvKhongDiemCN,
                        NVKhongHocBa = nvKhongHocBa,
                        NVBoQua = nvBoQua,
                        TyLeThieu = tyLeThieu
                    };
                })
                .OrderByDescending(x => x.TongNV)
                .ToList();

            ketQua.TongLoiKhongTimThayNganh = maNganhKhongTimThay.Count;
            ketQua.DanhSachMaNganhKhongTim = maNganhKhongTimThay.ToList();
            ketQua.DanhSachThieuDiem = ketQuaThieuDiem;

            ketQua.ThongKeTongHop = thongKeTongHop;
            ketQua.ThongKeTheoNganh = thongKeTheoNganh;

            ketQua.ThanhCong = true;
            if (maNganhKhongTimThay.Count > 0)
            {
                ketQua.ThongBao = $"Hoàn tất. Có {maNganhKhongTimThay.Count} mã xét tuyển không tìm thấy trong CSDL: {string.Join(", ", maNganhKhongTimThay)}.";
            }

            return ketQua;
        }

        public async Task<KetQuaKiemTraDiemSan> KiemTraDiemSan(string maNganh, string fileId)
        {
            var result = new KetQuaKiemTraDiemSan();

            if (string.IsNullOrEmpty(fileId))
            {
                result.ThanhCong = false;
                result.ThongBao = "Mã tệp Excel không hợp lệ.";
                return result;
            }

            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            var filePath = System.IO.File.Exists(fileId) ? fileId : Path.Combine(uploadsFolder, fileId);

            if (!System.IO.File.Exists(filePath))
            {
                result.ThanhCong = false;
                result.ThongBao = "Tệp Excel không tồn tại trên hệ thống.";
                return result;
            }

            List<KetQuaNguyenVongImport> danhSach;
            try
            {
                danhSach = ReadKetQuaNguyenVongExcel(filePath);
            }
            catch (Exception ex)
            {
                result.ThanhCong = false;
                result.ThongBao = "Có lỗi xảy ra khi đọc tệp Excel: " + ex.Message;
                return result;
            }

            var danhSachNganh = await _context.Nganhs
                .AsNoTracking()
                .Include(n => n.ToHopNganhs)
                    .ThenInclude(th => th.ToHopMon)
                .ToListAsync();

            var nganhDict = danhSachNganh
                .Where(n => !string.IsNullOrEmpty(n.MaNganh))
                .ToDictionary(n => n.MaNganh.Trim(), n => n, StringComparer.OrdinalIgnoreCase);

            // Nếu người dùng chọn 1 ngành cụ thể, chỉ kiểm tra các học sinh thuộc ngành đó
            if (!string.IsNullOrWhiteSpace(maNganh))
            {
                danhSach = danhSach
                    .Where(r => !string.IsNullOrWhiteSpace(r.MaNganh) && r.MaNganh.Trim().Equals(maNganh.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var danhSachKiemTra = new List<BaoCaoKiemTraDiemSanItem>();

            foreach (var item in danhSach)
            {
                var ghiChuList = new List<string>();

                // 1. Kiểm tra MaNganh có tồn tại trong hệ thống không
                Nganh? nganh = null;
                if (string.IsNullOrWhiteSpace(item.MaNganh))
                {
                    ghiChuList.Add("Mã ngành trống");
                }
                else if (!nganhDict.TryGetValue(item.MaNganh.Trim(), out nganh))
                {
                    ghiChuList.Add($"Mã ngành '{item.MaNganh}' không tồn tại trong hệ thống");
                }
                else
                {
                    // 2. Kiểm tra ToHop có trong ngành đó không
                    if (string.IsNullOrWhiteSpace(item.ToHop))
                    {
                        ghiChuList.Add("Mã tổ hợp trống");
                    }
                    else
                    {
                        bool toHopValid = nganh.ToHopNganhs.Any(th => th.ToHopMon != null &&
                            th.ToHopMon.MaToHop.Trim().Equals(item.ToHop.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (!toHopValid)
                        {
                            ghiChuList.Add($"Tổ hợp '{item.ToHop}' không thuộc ngành {nganh.MaNganh}");
                        }
                    }
                }

                // 3. Kiểm tra các điểm môn 1, 2, 3 HB và THPT có trường nào null không
                string mon1Ten = string.IsNullOrWhiteSpace(item.Mon1) ? "Môn 1" : item.Mon1;
                string mon2Ten = string.IsNullOrWhiteSpace(item.Mon2) ? "Môn 2" : item.Mon2;
                string mon3Ten = string.IsNullOrWhiteSpace(item.Mon3) ? "Môn 3" : item.Mon3;

                // Nếu HeSoHB > 0 hoặc không tìm thấy ngành, mới yêu cầu kiểm tra điểm học bạ
                bool kiemTraHocBa = nganh == null || nganh.HeSoHB > 0;

                if (kiemTraHocBa)
                {
                    if (item.DiemMon1HB == null) ghiChuList.Add($"Thiếu điểm HB môn {mon1Ten}");
                    if (item.DiemMon2HB == null) ghiChuList.Add($"Thiếu điểm HB môn {mon2Ten}");
                    if (item.DiemMon3HB == null) ghiChuList.Add($"Thiếu điểm HB môn {mon3Ten}");
                }

                if (item.DiemMon1THPT == null) ghiChuList.Add($"Thiếu điểm THPT môn {mon1Ten}");
                if (item.DiemMon2THPT == null) ghiChuList.Add($"Thiếu điểm THPT môn {mon2Ten}");
                if (item.DiemMon3THPT == null) ghiChuList.Add($"Thiếu điểm THPT môn {mon3Ten}");

                // 4. Kiểm tra điểm xét tuyển của học sinh với DXT của ngành
                if (nganh != null)
                {
                    if (nganh.DXT > 0 && (item.DiemXetTuyen == null || item.DiemXetTuyen < nganh.DXT))
                    {
                        ghiChuList.Add($"Điểm xét tuyển ({item.DiemXetTuyen ?? 0}) dưới điểm sàn ngành ({nganh.DXT})");
                    }

                    // 5. Kiểm tra điểm sàn Toán (nếu DiemSanToan > 0)
                    if (nganh.DiemSanToan > 0)
                    {
                        if (item.DiemMon1THPT == null || item.DiemMon1THPT < nganh.DiemSanToan)
                        {
                            ghiChuList.Add($"Điểm THPT môn Toán ({item.DiemMon1THPT ?? 0}) dưới điểm sàn môn Toán ({nganh.DiemSanToan})");
                        }
                    }
                }

                // Nếu vi phạm điều kiện nào thì thêm vào danh sách
                if (ghiChuList.Count > 0)
                {
                    danhSachKiemTra.Add(new BaoCaoKiemTraDiemSanItem
                    {
                        HoTen = item.HoTen,
                        CCCD = item.CCCD,
                        MaNganh = item.MaNganh ?? "",
                        ToHop = item.ToHop ?? "",
                        DiemXetTuyen = item.DiemXetTuyen ?? 0,
                        DiemSan = nganh?.DXT ?? 0,
                        DiemSanToan = nganh?.DiemSanToan ?? 0,
                        GhiChu = string.Join("; ", ghiChuList)
                    });
                }
            }

            result.ThanhCong = true;
            result.ThongBao = "Kiểm tra điểm sàn hoàn tất.";
            result.TongSoThiSinh = danhSach.Count;
            result.SoThiSinhKhongDat = danhSachKiemTra.Count;
            result.SoThiSinhDat = Math.Max(0, result.TongSoThiSinh - result.SoThiSinhKhongDat);
            result.DanhSachKiemTraDiemSan = danhSachKiemTra;

            return result;
        }

        public async Task<List<Nganh>> LayDanhSachNganhAsync()
        {
            return await _context.Nganhs
                .AsNoTracking()
                .Include(n => n.ToHopNganhs)
                    .ThenInclude(th => th.ToHopMon)
                .ToListAsync();
        }

        private List<HocBaTHPTImport> ParseExcelToList(string filePath, int? limit = null)
        {
            var list = new List<HocBaTHPTImport>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(filePath)))
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets[0];
                int totalRows = sheet.Dimension.End.Row;
                int startRow = 4; // Data rows start at Row 4
                int endRow = limit.HasValue ? Math.Min(startRow + limit.Value - 1, totalRows) : totalRows;

                for (int r = startRow; r <= endRow; r++)
                {
                    var item = new HocBaTHPTImport
                    {
                        STT = ParseInt(sheet.Cells[r, 1].Value),
                        SoDDCN = ParseString(sheet.Cells[r, 2].Value),
                        HoVaTen = ParseString(sheet.Cells[r, 3].Value),
                        NgaySinh = ParseDateTime(sheet.Cells[r, 4].Value),
                        GioiTinh = ParseString(sheet.Cells[r, 5].Value),
                        Lop = ParseInt(sheet.Cells[r, 6].Value),
                        ChuongTrinhHoc = ParseInt(sheet.Cells[r, 7].Value),

                        DiemTrungBinhNam = ParseDecimal(sheet.Cells[r, 8].Value),
                        DiemTongKetHKI = ParseDecimal(sheet.Cells[r, 9].Value),
                        DiemTongKetHKII = ParseDecimal(sheet.Cells[r, 10].Value),
                        DiemTongKetCN = ParseDecimal(sheet.Cells[r, 11].Value),

                        HocLucHKI = ParseString(sheet.Cells[r, 12].Value),
                        HocLucHKII = ParseString(sheet.Cells[r, 13].Value),
                        HocLucCN = ParseString(sheet.Cells[r, 14].Value),

                        HanhKiemHKI = ParseString(sheet.Cells[r, 15].Value),
                        HanhKiemHKII = ParseString(sheet.Cells[r, 16].Value),
                        HanhKiemCN = ParseString(sheet.Cells[r, 17].Value),

                        KetQuaHocTapHKI = ParseString(sheet.Cells[r, 18].Value),
                        KetQuaHocTapHKII = ParseString(sheet.Cells[r, 19].Value),
                        KetQuaHocTapCN = ParseString(sheet.Cells[r, 20].Value),

                        KetQuaRenLuyenHKI = ParseString(sheet.Cells[r, 21].Value),
                        KetQuaRenLuyenHKII = ParseString(sheet.Cells[r, 22].Value),
                        KetQuaRenLuyenCN = ParseString(sheet.Cells[r, 23].Value),

                        ToanHKI = ParseDecimal(sheet.Cells[r, 24].Value),
                        ToanHKII = ParseDecimal(sheet.Cells[r, 25].Value),
                        ToanCN = ParseDecimal(sheet.Cells[r, 26].Value),

                        VanHKI = ParseDecimal(sheet.Cells[r, 27].Value),
                        VanHKII = ParseDecimal(sheet.Cells[r, 28].Value),
                        VanCN = ParseDecimal(sheet.Cells[r, 29].Value),

                        VatLyHKI = ParseDecimal(sheet.Cells[r, 30].Value),
                        VatLyHKII = ParseDecimal(sheet.Cells[r, 31].Value),
                        VatLyCN = ParseDecimal(sheet.Cells[r, 32].Value),

                        HoaHocHKI = ParseDecimal(sheet.Cells[r, 33].Value),
                        HoaHocHKII = ParseDecimal(sheet.Cells[r, 34].Value),
                        HoaHocCN = ParseDecimal(sheet.Cells[r, 35].Value),

                        SinhHocHKI = ParseDecimal(sheet.Cells[r, 36].Value),
                        SinhHocHKII = ParseDecimal(sheet.Cells[r, 37].Value),
                        SinhHocCN = ParseDecimal(sheet.Cells[r, 38].Value),

                        LichSuHKI = ParseDecimal(sheet.Cells[r, 39].Value),
                        LichSuHKII = ParseDecimal(sheet.Cells[r, 40].Value),
                        LichSuCN = ParseDecimal(sheet.Cells[r, 41].Value),

                        DiaLyHKI = ParseDecimal(sheet.Cells[r, 42].Value),
                        DiaLyHKII = ParseDecimal(sheet.Cells[r, 43].Value),
                        DiaLyCN = ParseDecimal(sheet.Cells[r, 44].Value),

                        GDCDHKI = ParseDecimal(sheet.Cells[r, 45].Value),
                        GDCDHKII = ParseDecimal(sheet.Cells[r, 46].Value),
                        GDCDCN = ParseDecimal(sheet.Cells[r, 47].Value),

                        KTPLHKI = ParseDecimal(sheet.Cells[r, 48].Value),
                        KTPLHKII = ParseDecimal(sheet.Cells[r, 49].Value),
                        KTPLCN = ParseDecimal(sheet.Cells[r, 50].Value),

                        TinHocHKI = ParseDecimal(sheet.Cells[r, 51].Value),
                        TinHocHKII = ParseDecimal(sheet.Cells[r, 52].Value),
                        TinHocCN = ParseDecimal(sheet.Cells[r, 53].Value),

                        CNCNHKI = ParseDecimal(sheet.Cells[r, 54].Value),
                        CNCNHKII = ParseDecimal(sheet.Cells[r, 55].Value),
                        CNCNCN = ParseDecimal(sheet.Cells[r, 56].Value),

                        CNNNHKI = ParseDecimal(sheet.Cells[r, 57].Value),
                        CNNNHKII = ParseDecimal(sheet.Cells[r, 58].Value),
                        CNNNCN = ParseDecimal(sheet.Cells[r, 59].Value),

                        NgoaiNguHKI = ParseDecimal(sheet.Cells[r, 60].Value),
                        NgoaiNguHKII = ParseDecimal(sheet.Cells[r, 61].Value),
                        NgoaiNguCN = ParseDecimal(sheet.Cells[r, 62].Value),
                        MonNgoaiNgu = ParseString(sheet.Cells[r, 63].Value),

                        TuChonSongNguHKI = ParseDecimal(sheet.Cells[r, 64].Value),
                        TuChonSongNguHKII = ParseDecimal(sheet.Cells[r, 65].Value),
                        TuChonSongNguCN = ParseDecimal(sheet.Cells[r, 66].Value),

                        QPANHKI = ParseDecimal(sheet.Cells[r, 67].Value),
                        QPANHKII = ParseDecimal(sheet.Cells[r, 68].Value),
                        QPANCN = ParseDecimal(sheet.Cells[r, 69].Value),

                        TiengDanTocHKI = ParseDecimal(sheet.Cells[r, 70].Value),
                        TiengDanTocHKII = ParseDecimal(sheet.Cells[r, 71].Value),
                        TiengDanTocCN = ParseDecimal(sheet.Cells[r, 72].Value),

                        NgoaiNgu2HKI = ParseDecimal(sheet.Cells[r, 73].Value),
                        NgoaiNgu2HKII = ParseDecimal(sheet.Cells[r, 74].Value),
                        NgoaiNgu2CN = ParseDecimal(sheet.Cells[r, 75].Value),
                        MonNgoaiNgu2 = ParseString(sheet.Cells[r, 76].Value),

                        ToanPhapHKI = ParseDecimal(sheet.Cells[r, 77].Value),
                        ToanPhapHKII = ParseDecimal(sheet.Cells[r, 78].Value),
                        ToanPhapCN = ParseDecimal(sheet.Cells[r, 79].Value),
                    };

                    if (!string.IsNullOrWhiteSpace(item.SoDDCN) && !string.IsNullOrWhiteSpace(item.HoVaTen))
                    {
                        list.Add(item);
                    }
                }
            }

            return list;
        }
        public List<KetQuaNguyenVongImport> ReadKetQuaNguyenVongExcel(string filePath)
        {
            var list = new List<KetQuaNguyenVongImport>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(filePath)))
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets[0];
                int totalRows = sheet.Dimension.End.Row;
                int startRow = 2; // Assuming row 1 is header

                for (int r = startRow; r <= totalRows; r++)
                {
                    var item = new KetQuaNguyenVongImport
                    {
                        HoTen = ParseString(sheet.Cells[r, 1].Value) ?? "",
                        CCCD = ParseString(sheet.Cells[r, 2].Value) ?? "",
                        NgaySinh = ParseDateTime(sheet.Cells[r, 3].Value) ?? default,
                        NamTN = ParseInt(sheet.Cells[r, 4].Value) ?? 0,
                        DTUT = ParseString(sheet.Cells[r, 5].Value),
                        KVUT = ParseString(sheet.Cells[r, 6].Value),
                        HocLuc = ParseString(sheet.Cells[r, 7].Value),
                        DiemXetTN = ParseString(sheet.Cells[r, 8].Value),
                        ThuTuNV = ParseInt(sheet.Cells[r, 9].Value),
                        MaTruong = ParseString(sheet.Cells[r, 10].Value),
                        MaNganh = ParseString(sheet.Cells[r, 11].Value),
                        PTXT = ParseString(sheet.Cells[r, 12].Value),
                        ToHop = ParseString(sheet.Cells[r, 13].Value),

                        Mon1 = ParseString(sheet.Cells[r, 14].Value),
                        TrongSoMon1 = ParseDecimal(sheet.Cells[r, 15].Value),
                        DiemMon1HB = ParseDecimal(sheet.Cells[r, 16].Value),
                        DiemMon1THPT = ParseDecimal(sheet.Cells[r, 17].Value),

                        Mon2 = ParseString(sheet.Cells[r, 18].Value),
                        TrongSoMon2 = ParseDecimal(sheet.Cells[r, 19].Value),
                        DiemMon2HB = ParseDecimal(sheet.Cells[r, 20].Value),
                        DiemMon2THPT = ParseDecimal(sheet.Cells[r, 21].Value),

                        Mon3 = ParseString(sheet.Cells[r, 22].Value),
                        TrongSoMon3 = ParseDecimal(sheet.Cells[r, 23].Value),
                        DiemMon3HB = ParseDecimal(sheet.Cells[r, 24].Value),
                        DiemMon3THPT = ParseDecimal(sheet.Cells[r, 25].Value),

                        TrongSoHB = ParseDecimal(sheet.Cells[r, 26].Value),
                        TrongSoTHPT = ParseDecimal(sheet.Cells[r, 27].Value),
                        DiemCong = ParseDecimal(sheet.Cells[r, 28].Value),
                        DiemUuTien = ParseDecimal(sheet.Cells[r, 29].Value),
                        DS = ParseDecimal(sheet.Cells[r, 30].Value),
                        TDHB = ParseDecimal(sheet.Cells[r, 31].Value),
                        TDTHPT = ParseDecimal(sheet.Cells[r, 32].Value),
                        TD = ParseDecimal(sheet.Cells[r, 33].Value),
                        DiemXetTuyen = ParseDecimal(sheet.Cells[r, 34].Value),
                        KQKiemTraNguong = ParseString(sheet.Cells[r, 35].Value)?.Trim().Equals("Đạt", StringComparison.OrdinalIgnoreCase) ?? false,
                        GhiChu = ParseString(sheet.Cells[r, 36].Value)
                    };

                    if (!string.IsNullOrWhiteSpace(item.CCCD) && !string.IsNullOrWhiteSpace(item.HoTen))
                    {
                        list.Add(item);
                    }
                }
            }

            return list;
        }
        private decimal? GetScore(HocBaTHPTImport record, string fieldName)
        {
            if (record == null || string.IsNullOrWhiteSpace(fieldName)) return null;

            var code = fieldName.Trim().ToUpper();
            if (Enum.TryParse<MaMonHoc>(code, out var maMon))
            {
                return maMon switch
                {
                    MaMonHoc.TO => record.ToanCN,
                    MaMonHoc.VA => record.VanCN,
                    MaMonHoc.LI => record.VatLyCN,
                    MaMonHoc.HO => record.HoaHocCN,
                    MaMonHoc.SI => record.SinhHocCN,
                    MaMonHoc.SU => record.LichSuCN,
                    MaMonHoc.DI => record.DiaLyCN,
                    MaMonHoc.GD => record.GDCDCN,
                    MaMonHoc.TI => record.TinHocCN,
                    MaMonHoc.CNCN => record.CNCNCN,
                    MaMonHoc.N1 or MaMonHoc.N2 or MaMonHoc.N3 or MaMonHoc.N4 or MaMonHoc.N5 or MaMonHoc.N6 => GetNgoaiNguScore(record, code),
                    _ => null
                };
            }

            return code switch
            {
                "KTPL" => record.KTPLCN,
                "CNNN" => record.CNNNCN,
                "CNCN" => record.CNCNCN,
                _ => null
            };
        }

        private string LayTenHienThiMonHocCN(string fieldName)
        {
            if (Enum.TryParse<MaMonHoc>(fieldName.ToUpper(), out var maMon))
            {
                return maMon switch
                {
                    MaMonHoc.TO => "Toán CN",
                    MaMonHoc.VA => "Văn CN",
                    MaMonHoc.LI => "Vật lí CN",
                    MaMonHoc.HO => "Hóa học CN",
                    MaMonHoc.SI => "Sinh học CN",
                    MaMonHoc.SU => "Lịch sử CN",
                    MaMonHoc.DI => "Địa lí CN",
                    MaMonHoc.GD => "GDCD CN",
                    MaMonHoc.TI => "Tin học CN",
                    MaMonHoc.CNCN => "CNCN CN",
                    MaMonHoc.NN => "Ngoại ngữ CN",
                    _ => fieldName + " CN"
                };
            }
            return fieldName + " CN";
        }

        private decimal? GetNgoaiNguScore(HocBaTHPTImport record, string code)
        {
            if (record == null) return null;

            var mon1 = record.MonNgoaiNgu?.Trim().ToUpper();
            var mon2 = record.MonNgoaiNgu2?.Trim().ToUpper();

            // 1. Nếu Môn ngoại ngữ 1 khớp mã yêu cầu -> lấy điểm Ngoại ngữ CN (dự phòng Ngoại ngữ 2 CN)
            if (!string.IsNullOrEmpty(mon1) && mon1.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return record.NgoaiNguCN ?? record.NgoaiNgu2CN;
            }

            // 2. Nếu Môn ngoại ngữ 2 khớp mã yêu cầu -> lấy điểm Ngoại ngữ 2 CN (dự phòng Ngoại ngữ CN)
            if (!string.IsNullOrEmpty(mon2) && mon2.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return record.NgoaiNgu2CN ?? record.NgoaiNguCN;
            }

            return null;
        }

        private int? ParseInt(object? val)
        {
            if (val == null) return null;
            if (val is int i) return i;
            if (val is double d) return (int)d;
            if (int.TryParse(val.ToString(), out int res)) return res;
            return null;
        }

        private string? ParseString(object? val)
        {
            if (val == null) return null;
            var s = val.ToString()?.Trim();
            return string.IsNullOrEmpty(s) ? null : s;
        }

        private decimal? ParseDecimal(object? val)
        {
            if (val == null) return null;
            if (val is decimal dec) return dec;
            if (val is double d) return (decimal)d;
            if (val is int i) return (decimal)i;
            var s = val.ToString()?.Replace(",", ".").Trim();
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal res)) return res;
            return null;
        }

        public async Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelThieuDiemToHopAsync(string excelId)
        {
            if (string.IsNullOrEmpty(excelId))
            {
                return (false, "Excel không hợp lệ.", null);
            }

            var result = await CheckHocBaAsync(excelId);
            if (!result.ThanhCong)
            {
                return (false, result.ThongBao ?? "Kiểm tra học bạ không thành công.", null);
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Thí sinh thiếu điểm");

                // Headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Số ĐDCN (CCCD)";
                worksheet.Cells[1, 3].Value = "Họ và tên";
                worksheet.Cells[1, 4].Value = "Năm lỗi";
                worksheet.Cells[1, 5].Value = "Tổ hợp";
                worksheet.Cells[1, 6].Value = "Môn bị thiếu điểm";

                // Styling headers
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(229, 241, 255));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 122, 255));
                }

                // Data
                int row = 2;
                foreach (var item in result.DanhSachThieuDiem)
                {
                    worksheet.Cells[row, 1].Value = item.Stt;
                    worksheet.Cells[row, 2].Value = item.Cccd;
                    worksheet.Cells[row, 3].Value = item.HoVaTen;
                    worksheet.Cells[row, 4].Value = item.NamLoi;
                    worksheet.Cells[row, 5].Value = item.ToHop;
                    worksheet.Cells[row, 6].Value = item.MonThieu;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return (true, "", package.GetAsByteArray());
            }
        }

        public async Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelKetQuaDoiChieuAsync(string hocBaFileId, string nguyenVongFileId)
        {
            if (string.IsNullOrEmpty(hocBaFileId) || string.IsNullOrEmpty(nguyenVongFileId))
                return (false, "Yêu cầu không hợp lệ.", null);

            var result = await DoiChieuHocBaVaNguyenVongAsync(hocBaFileId, nguyenVongFileId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Đối chiếu HB - NV");

            // Headers
            string[] headers = { "STT", "Số ĐDCN (CCCD)", "Họ và Tên", "TT Nguyện Vọng", "Mã Ngành", "Tên Ngành", "Mã Tổ Hợp", "Năm Học", "Môn Thiếu" };
            for (int c = 0; c < headers.Length; c++)
                ws.Cells[1, c + 1].Value = headers[c];

            using (var range = ws.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(229, 241, 255));
                range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 122, 255));
            }

            int row = 2;
            foreach (var item in result.DanhSachThieuDiem)
            {
                ws.Cells[row, 1].Value = item.Stt;
                ws.Cells[row, 2].Value = item.SoDDCN;
                ws.Cells[row, 3].Value = item.HoVaTen;
                ws.Cells[row, 4].Value = item.ThuTuNV;
                ws.Cells[row, 5].Value = item.MaNganh;
                ws.Cells[row, 6].Value = item.TenNganh;
                ws.Cells[row, 7].Value = item.MaToHop;
                ws.Cells[row, 8].Value = item.NamHoc;
                ws.Cells[row, 9].Value = item.MonThieu;
                row++;
            }
            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            return (true, "", package.GetAsByteArray());
        }

        public async Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelKiemTraDiemSanAsync(string maNganh, string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                return (false, "Yêu cầu không hợp lệ.", null);

            var result = await KiemTraDiemSan(maNganh, fileId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Kiểm tra điểm sàn");

            string[] headers = { "STT", "Họ và Tên", "Số ĐDCN (CCCD)", "Mã Ngành", "Tổ Hợp", "Điểm Xét Tuyển", "Điểm Sàn Ngành", "Điểm Sàn Toán", "Ghi Chú Lỗi" };
            for (int c = 0; c < headers.Length; c++)
                ws.Cells[1, c + 1].Value = headers[c];

            using (var range = ws.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(229, 241, 255));
                range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 122, 255));
            }

            int row = 2;
            int stt = 1;
            foreach (var item in result.DanhSachKiemTraDiemSan)
            {
                ws.Cells[row, 1].Value = stt++;
                ws.Cells[row, 2].Value = item.HoTen;
                ws.Cells[row, 3].Value = item.CCCD;
                ws.Cells[row, 4].Value = item.MaNganh;
                ws.Cells[row, 5].Value = item.ToHop;
                ws.Cells[row, 6].Value = item.DiemXetTuyen;
                ws.Cells[row, 7].Value = item.DiemSan;
                ws.Cells[row, 8].Value = item.DiemSanToan;
                ws.Cells[row, 9].Value = item.GhiChu;
                row++;
            }
            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            return (true, "", package.GetAsByteArray());
        }

        private DateTime? ParseDateTime(object? val)
        {
            if (val == null) return null;
            if (val is DateTime dt) return dt;
            if (val is double d)
            {
                try { return DateTime.FromOADate(d); } catch { return null; }
            }
            var s = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(s)) return null;

            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "dd/MM/yyyy HH:mm:ss" };
            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime res))
            {
                return res;
            }
            if (DateTime.TryParse(s, out DateTime resGeneral))
            {
                return resGeneral;
            }
            return null;
        }
    }
}

