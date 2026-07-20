using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Hangfire;
using System.Globalization;
using TuyenSinh.Data;
using TuyenSinh.ViewModels;

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
            if (file == null || file.Length == 0)
            {
                return (false, "Vui lòng chọn tập Excel để tải lên.", null, null);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                return (false, "Hỗ trợ định dạng .xlsx.", null, null);
            }

            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var excelId = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, excelId);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

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
                using var pkgHB = new ExcelPackage(new FileInfo(fileHocBaPath));
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
            var groupStart = DateTime.Now;
            var hocBaTheoCccd = danhSachHocBa
                .GroupBy(r => r.SoDDCN)
                .ToDictionary(g => g.Key!, g => g.ToList());

            // 2. Đọc file nguyện vọng (header ở Row 5)
            var nvStart = DateTime.Now;
            List<NguyenVongItem> danhSachNV;
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var pkgNV = new ExcelPackage(new FileInfo(fileNguyenVongPath));
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

            // 4. Xử lý từng nguyện vọng Group theo CCCD
            var processingStart = DateTime.Now;
            var maNganhKhongTimThay = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ketQuaThieuDiem = new List<KetQuaDoiChieuItem>();
            int stt = 1;

            // Group nguyện vọng theo CCCD
            var nvTheoCccd = danhSachNV
                .GroupBy(nv => nv.SoDDCN!)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var kvp in nvTheoCccd)
            {
                var cccd = kvp.Key;
                var listNV = kvp.Value;

                // Lấy học bạ của thí sinh bằng Dictionary
                if (!hocBaTheoCccd.TryGetValue(cccd, out var hocBaThiSinh))
                {
                    continue;
                }

                var tenHoVaTen = hocBaThiSinh.FirstOrDefault()?.HoVaTen;

                // Lấy record theo từng năm của thí sinh này một lần duy nhất
                var lopHB10 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 10);
                var lopHB11 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 11);
                var lopHB12 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 12);

                foreach (var nv in listNV)
                {
                    var maNganh = nv.MaXetTuyen!.Trim();

                    // Tìm ngành trong DB bằng Dictionary
                    if (!nganhDict.TryGetValue(maNganh, out var nganh))
                    {
                        maNganhKhongTimThay.Add(maNganh);
                        continue;
                    }

                    // Kiểm tra mỗi tổ hợp của ngành
                    foreach (var toHopNganh in nganh.ToHopNganhs)
                    {
                        var toHop = toHopNganh.ToHopMon;
                        if (toHop == null) continue;

                        // Kiểm tra từng môn trong tổ hợp, theo từng năm
                        var cacNamRecord = new[] {
                            (Nam: "Lớp 10", Record: lopHB10),
                            (Nam: "Lớp 11", Record: lopHB11),
                            (Nam: "Lớp 12", Record: lopHB12)
                        };

                        foreach (var (namHoc, record) in cacNamRecord)
                        {
                            var cacMonThieu = new List<string>();

                            foreach (var monHoc in toHop.MonHocs)
                            {
                                decimal? diem = record == null ? null : GetScore(record, monHoc.FieldName);
                                if (diem == null)
                                {
                                    cacMonThieu.Add(LayTenHienThiMonHocCN(monHoc.FieldName));
                                }
                            }

                            if (cacMonThieu.Count > 0)
                            {
                                ketQuaThieuDiem.Add(new KetQuaDoiChieuItem
                                {
                                    Stt = stt++,
                                    SoDDCN = cccd,
                                    HoVaTen = tenHoVaTen,
                                    ThuTuNV = nv.ThuTuNV,
                                    MaNganh = nganh.MaNganh,
                                    TenNganh = nganh.TenNganh,
                                    MaToHop = toHop.MaToHop,
                                    NamHoc = namHoc,
                                    MonThieu = string.Join(", ", cacMonThieu)
                                });
                            }
                        }
                    }
                }
            }

            ketQua.TongLoiKhongTimThayNganh = maNganhKhongTimThay.Count;
            ketQua.DanhSachMaNganhKhongTim = maNganhKhongTimThay.ToList();
            ketQua.DanhSachThieuDiem = ketQuaThieuDiem;
            ketQua.ThanhCong = true;
            if (maNganhKhongTimThay.Count > 0)
            {
                ketQua.ThongBao = $"Hoàn tất. Có {maNganhKhongTimThay.Count} mã xét tuyển không tìm thấy trong CSDL: {string.Join(", ", maNganhKhongTimThay)}.";
            }

            return ketQua;
        }

        private List<HocBaTHPTImport> ParseExcelToList(string filePath, int? limit = null)
        {
            var list = new List<HocBaTHPTImport>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
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

        private decimal? GetScore(HocBaTHPTImport record, string fieldName)
        {
            return fieldName.ToLower() switch
            {
                "toan" => record.ToanCN,
                "van" => record.VanCN,
                "vatly" => record.VatLyCN,
                "hoahoc" => record.HoaHocCN,
                "sinhhoc" => record.SinhHocCN,
                "dialy" => record.DiaLyCN,
                "tinhoc" => record.TinHocCN,
                "cncn" => record.CNCNCN,
                "ngoaingu" => record.NgoaiNguCN,
                _ => null
            };
        }

        private string LayTenHienThiMonHocCN(string fieldName)
        {
            return fieldName.ToLower() switch
            {
                "toan" => "Toán CN",
                "van" => "Văn CN",
                "vatly" => "Vật lí CN",
                "hoahoc" => "Hóa học CN",
                "sinhhoc" => "Sinh học CN",
                "dialy" => "Địa lí CN",
                "tinhoc" => "Tin học CN",
                "cncn" => "CNCN CN",
                "ngoaingu" => "Ngoại ngữ CN",
                _ => fieldName + " CN"
            };
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

