using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Models;
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
                return (false, "Vui lòng chọn tệp Excel để tải lên.", null, null);
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls" && extension != ".csv")
            {
                return (false, "Hỗ trợ định dạng .xlsx, .xls, .csv.", null, null);
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
                // Parse first 100 rows for preview
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

                // Schedule deletion after 30 minutes via Hangfire
                _backgroundJobClient.Schedule<IHocBaService>(s => s.DeleteExpiredFileAsync(excelId), TimeSpan.FromMinutes(30));

                return (true, "Tải lên và đọc tệp thành công.", excelId, previewData);
            }
            catch (Exception ex)
            {
                // Clean up file if parsing fails
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                return (false, "Lỗi khi đọc tệp Excel: " + ex.Message, null, null);
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
                result.ThongBao = "Không tìm thấy mã tệp Excel.";
                return result;
            }

            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            var filePath = Path.Combine(uploadsFolder, excelId);

            if (!System.IO.File.Exists(filePath))
            {
                result.ThanhCong = false;
                result.ThongBao = "Tệp Excel không tồn tại trên hệ thống.";
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
                result.ThongBao = "Có lỗi xảy ra khi đọc tệp Excel: " + ex.Message;
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

        #region Helper Methods for Excel Parsing & Score Mapping

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

        #endregion
    }
}
