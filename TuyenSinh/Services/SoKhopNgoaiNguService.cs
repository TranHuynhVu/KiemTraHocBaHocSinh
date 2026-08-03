using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TuyenSinh.ViewModels;

namespace TuyenSinh.Services
{
    public class SoKhopNgoaiNguService : ISoKhopNgoaiNguService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public SoKhopNgoaiNguService(IWebHostEnvironment hostingEnvironment, IBackgroundJobClient backgroundJobClient)
        {
            _hostingEnvironment = hostingEnvironment;
            _backgroundJobClient = backgroundJobClient;
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

            _backgroundJobClient.Schedule<SoKhopNgoaiNguService>(s => s.DeleteExpiredFileAsync(fileId), TimeSpan.FromMinutes(30));

            return fileId;
        }

        public async Task DeleteExpiredFileAsync(string fileId)
        {
            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            var filePath = Path.Combine(uploadsFolder, fileId);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch { }
            }
        }

        public async Task<SoKhopNgoaiNguThongKeViewModel> Join3ExcelFilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");

            var pathNV = Path.Combine(uploadsFolder, nvFileId);
            var pathDSTS = Path.Combine(uploadsFolder, dstsFileId);
            var pathNN = Path.Combine(uploadsFolder, nnFileId);

            // Read File 1: Nguyện Vọng
            var (mapNV, totalCountNV) = ReadNguyenVongFile(pathNV);

            // Read File 2: Danh Sách Thí Sinh
            var (mapDSTS, totalCountDSTS) = ReadDstsFile(pathDSTS);

            // Read File 3: Hợp Lệ Ngoại Ngữ
            var (listNN, mapNN, totalCountNN) = ReadHopLeNnFile(pathNN);

            // Perform Join on DDCN strictly starting from DSHopLeNN candidates
            var resultItems = new List<KetQuaSoKhopNgoaiNguItem>();

            foreach (var nnItem in listNN)
            {
                var ddcn = nnItem.Ddcn;
                if (string.IsNullOrEmpty(ddcn)) continue;

                mapDSTS.TryGetValue(ddcn, out var dstsInfo);
                mapNV.TryGetValue(ddcn, out var mxtList);

                string sbd = dstsInfo.Sbd ?? nnItem.Sbd ?? string.Empty;
                string hoTen = dstsInfo.HoTen ?? string.Empty;
                string ngaySinh = dstsInfo.NgaySinh ?? string.Empty;
                string chungChi = nnItem.ChungChi ?? string.Empty;
                string diemBac = nnItem.DiemBac ?? string.Empty;
                string maXetTuyen = mxtList != null && mxtList.Any()
                    ? string.Join("; ", mxtList.Distinct())
                    : string.Empty;

                resultItems.Add(new KetQuaSoKhopNgoaiNguItem
                {
                    SoBaoDanh = sbd,
                    HoTen = hoTen,
                    NgaySinh = ngaySinh,
                    Ddcn = ddcn,
                    ChungChiNgoaiNgu = chungChi,
                    DiemBacChungChi = diemBac,
                    MaXetTuyen = maXetTuyen,
                    MatchStatus = "Khớp ĐDCN"
                });
            }

            // Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                resultItems = resultItems.Where(x =>
                    x.Ddcn.ToLower().Contains(s) ||
                    x.HoTen.ToLower().Contains(s) ||
                    x.SoBaoDanh.ToLower().Contains(s) ||
                    x.MaXetTuyen.ToLower().Contains(s) ||
                    x.ChungChiNgoaiNgu.ToLower().Contains(s)
                ).ToList();
            }

            // Assign STT
            for (int i = 0; i < resultItems.Count; i++)
            {
                resultItems[i].Stt = i + 1;
            }

            return new SoKhopNgoaiNguThongKeViewModel
            {
                TongHopLeNN = totalCountNN,
                TongDanhSachThiSinh = totalCountDSTS,
                TongNguyenVong = totalCountNV,
                TongSoKhop = resultItems.Count,
                DanhSachKetQua = resultItems
            };
        }

        public async Task<(bool Success, string Message, byte[]? FileContents)> XuatExcel3FilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null)
        {
            var data = await Join3ExcelFilesAsync(nvFileId, dstsFileId, nnFileId, search);
            if (data.DanhSachKetQua == null || !data.DanhSachKetQua.Any())
                return (false, "Không có dữ liệu so khớp để xuất Excel.", null);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("KetQua_SoKhop_3File");

            // Header styling
            string[] headers = new[] { "STT", "Số báo danh", "Họ Tên", "Ngày sinh", "ĐDCN", "Chứng chỉ ngoại ngữ", "Điểm / Bậc chứng chỉ", "Mã xét tuyển" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = sheet.Cells[1, col + 1];
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 122, 255));
                cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            int row = 2;
            foreach (var item in data.DanhSachKetQua)
            {
                sheet.Cells[row, 1].Value = item.Stt;
                sheet.Cells[row, 2].Value = item.SoBaoDanh;
                sheet.Cells[row, 3].Value = item.HoTen;
                sheet.Cells[row, 4].Value = item.NgaySinh;
                sheet.Cells[row, 5].Value = item.Ddcn;
                sheet.Cells[row, 6].Value = item.ChungChiNgoaiNgu;
                sheet.Cells[row, 7].Value = item.DiemBacChungChi;
                sheet.Cells[row, 8].Value = item.MaXetTuyen;

                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            return (true, "Thành công", package.GetAsByteArray());
        }

        #region Private Parsing Helpers

        private (Dictionary<string, List<string>> MapNV, int TotalCount) ReadNguyenVongFile(string filePath)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int totalRows = 0;

            if (!File.Exists(filePath)) return (result, 0);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null) return (result, 0);

            int endRow = sheet.Dimension.End.Row;

            // Fixed column positions: Header at Row 5, Data starts at Row 6
            // Column 2: Số ĐDCN
            // Column 6: Mã xét tuyển
            int headerRow = 5;
            int colDdcn = 2;
            int colMaXetTuyen = 6;

            for (int r = headerRow + 1; r <= endRow; r++)
            {
                var ddcn = ParseString(sheet.Cells[r, colDdcn].Value);
                var mxt = ParseString(sheet.Cells[r, colMaXetTuyen].Value);

                if (string.IsNullOrWhiteSpace(ddcn)) continue;
                totalRows++;

                if (!string.IsNullOrWhiteSpace(mxt))
                {
                    if (!result.TryGetValue(ddcn, out var list))
                    {
                        list = new List<string>();
                        result[ddcn] = list;
                    }
                    list.Add(mxt);
                }
            }

            return (result, totalRows);
        }

        private (Dictionary<string, (string? Sbd, string? HoTen, string? NgaySinh)> MapDSTS, int TotalCount) ReadDstsFile(string filePath)
        {
            var result = new Dictionary<string, (string? Sbd, string? HoTen, string? NgaySinh)>(StringComparer.OrdinalIgnoreCase);
            int totalRows = 0;

            if (!File.Exists(filePath)) return (result, 0);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null) return (result, 0);

            int endRow = sheet.Dimension.End.Row;

            // Fixed column positions: Header at Row 1, Data starts at Row 2
            // Column 2: SBD, Column 3: Họ Tên, Column 4: ĐDCN, Column 5: Ngày sinh
            int colSbd = 2, colHoTen = 3, colDdcn = 4, colNgaySinh = 5;

            for (int r = 2; r <= endRow; r++)
            {
                var ddcn = ParseString(sheet.Cells[r, colDdcn].Value);
                var sbd = ParseString(sheet.Cells[r, colSbd].Value);
                var hoTen = ParseString(sheet.Cells[r, colHoTen].Value);
                var ngaySinh = ParseString(sheet.Cells[r, colNgaySinh].Value);

                if (string.IsNullOrWhiteSpace(ddcn) && string.IsNullOrWhiteSpace(sbd) && string.IsNullOrWhiteSpace(hoTen)) continue;
                totalRows++;

                if (!string.IsNullOrWhiteSpace(ddcn))
                {
                    result[ddcn] = (sbd, hoTen, ngaySinh);
                }
            }

            return (result, totalRows);
        }

        private (List<(string Ddcn, string? Sbd, string? ChungChi, string? DiemBac)> ListNN, Dictionary<string, (string? Sbd, string? ChungChi, string? DiemBac)> MapNN, int TotalCount) ReadHopLeNnFile(string filePath)
        {
            var list = new List<(string Ddcn, string? Sbd, string? ChungChi, string? DiemBac)>();
            var map = new Dictionary<string, (string? Sbd, string? ChungChi, string? DiemBac)>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(filePath)) return (list, map, 0);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null) return (list, map, 0);

            int endRow = sheet.Dimension.End.Row;

            // Fixed column positions: Header at Row 1, Data starts at Row 2
            // Column 2: SBD, Column 3: ĐDCN, Column 4: Chứng chỉ ngoại ngữ, Column 5: Điểm / Bậc CC
            int colSbd = 2, colDdcn = 3, colCc = 4, colDiem = 5;

            for (int r = 2; r <= endRow; r++)
            {
                var ddcn = ParseString(sheet.Cells[r, colDdcn].Value);
                var sbd = ParseString(sheet.Cells[r, colSbd].Value);
                var cc = ParseString(sheet.Cells[r, colCc].Value);
                var diem = ParseString(sheet.Cells[r, colDiem].Value);

                if (string.IsNullOrWhiteSpace(ddcn) && string.IsNullOrWhiteSpace(sbd)) continue;

                if (!string.IsNullOrWhiteSpace(ddcn))
                {
                    list.Add((ddcn, sbd, cc, diem));
                    map[ddcn] = (sbd, cc, diem);
                }
            }

            return (list, map, list.Count);
        }

        private static string? ParseString(object? val)
        {
            if (val == null) return null;
            var str = val.ToString()?.Trim();
            return string.IsNullOrEmpty(str) ? null : str;
        }

        #endregion
    }
}
