using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Helpers;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public class DiemCongService : IDiemCongService
    {
        private readonly ApplicationDbContext _context;

        public DiemCongService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DiemCong>> LayDanhSachDiemCongAsync(int? namHoc = null, string? search = null)
        {
            var query = _context.DiemCongs.AsNoTracking().AsQueryable();

            if (namHoc.HasValue && namHoc.Value > 0)
            {
                query = query.Where(x => x.NamHoc == namHoc.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DDCN.ToLower().Contains(s) ||
                    x.HoTen.ToLower().Contains(s) ||
                    x.MaXetTuyen.ToLower().Contains(s) ||
                    x.MaPTXT.ToLower().Contains(s) ||
                    (x.MaToHop != null && x.MaToHop.ToLower().Contains(s))
                );
            }

            return await query.OrderByDescending(x => x.NamHoc)
                              .ThenBy(x => x.HoTen)
                              .ToListAsync();
        }

        public async Task<List<int>> LayDanhSachNamHocAsync()
        {
            var currentYear = DateTime.Now.Year;
            var dbYears = await _context.DiemCongs.Select(x => x.NamHoc).Distinct().ToListAsync();

            if (!dbYears.Contains(currentYear))
            {
                dbYears.Add(currentYear);
            }

            return dbYears.OrderByDescending(x => x).ToList();
        }

        public async Task<DiemCong?> LayTheoIdAsync(int id)
        {
            return await _context.DiemCongs.FindAsync(id);
        }

        public async Task<(bool Success, string Message)> ThemDiemCongAsync(DiemCong model)
        {
            if (string.IsNullOrWhiteSpace(model.DDCN))
                return (false, "Định danh cá nhân (ĐDCN) không được để trống.");

            if (string.IsNullOrWhiteSpace(model.HoTen))
                return (false, "Họ tên không được để trống.");

            if (model.NamHoc <= 0)
                return (false, "Năm học không hợp lệ.");

            try
            {
                model.DDCN = model.DDCN.Trim();
                model.HoTen = model.HoTen.Trim();
                model.MaXetTuyen = model.MaXetTuyen?.Trim() ?? string.Empty;
                model.MaPTXT = model.MaPTXT?.Trim() ?? string.Empty;
                model.MaToHop = model.MaToHop?.Trim() ?? string.Empty;

                _context.DiemCongs.Add(model);
                await _context.SaveChangesAsync();
                return (true, "Thêm mới điểm cộng thành công.");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi thêm điểm cộng: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> SuaDiemCongAsync(DiemCong model)
        {
            var entity = await _context.DiemCongs.FindAsync(model.Id);
            if (entity == null)
                return (false, "Không tìm thấy dữ liệu điểm cộng cần sửa.");

            if (string.IsNullOrWhiteSpace(model.DDCN))
                return (false, "Định danh cá nhân (ĐDCN) không được để trống.");

            if (string.IsNullOrWhiteSpace(model.HoTen))
                return (false, "Họ tên không được để trống.");

            try
            {
                entity.DDCN = model.DDCN.Trim();
                entity.HoTen = model.HoTen.Trim();
                entity.DOB = model.DOB;
                entity.MaXetTuyen = model.MaXetTuyen?.Trim() ?? string.Empty;
                entity.MaPTXT = model.MaPTXT?.Trim() ?? string.Empty;
                entity.MaToHop = model.MaToHop?.Trim() ?? string.Empty;
                entity.LoaiDiemCong = model.LoaiDiemCong;
                entity.Diem = model.Diem;
                entity.NamHoc = model.NamHoc;

                _context.DiemCongs.Update(entity);
                await _context.SaveChangesAsync();
                return (true, "Cập nhật thông tin điểm cộng thành công.");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi cập nhật điểm cộng: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> XoaDiemCongAsync(int id)
        {
            var entity = await _context.DiemCongs.FindAsync(id);
            if (entity == null)
                return (false, "Không tìm thấy dữ liệu điểm cộng cần xóa.");

            try
            {
                _context.DiemCongs.Remove(entity);
                await _context.SaveChangesAsync();
                return (true, "Xóa bản ghi điểm cộng thành công.");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi xóa bản ghi điểm cộng: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> XoaTheoNamAsync(int namHoc)
        {
            var records = await _context.DiemCongs.Where(x => x.NamHoc == namHoc).ToListAsync();
            if (!records.Any())
                return (false, $"Không có dữ liệu điểm cộng cho năm học {namHoc}.");

            try
            {
                _context.DiemCongs.RemoveRange(records);
                await _context.SaveChangesAsync();
                return (true, $"Đã xóa thành công {records.Count} bản ghi điểm cộng năm {namHoc}.");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi xóa dữ liệu theo năm: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message, int TotalImported, int TotalSkipped)> ImportExcelAsync(IFormFile file, int namHoc, bool overwriteExisting = false)
        {
            if (file == null || file.Length == 0)
                return (false, "Vui lòng chọn file Excel để upload.", 0, 0);

            var fileExt = Path.GetExtension(file.FileName).ToLower();
            if (fileExt == ".xls")
            {
                return (false, "Hệ thống chỉ hỗ trợ file định dạng Excel 2007 trở lên (.xlsx). Nếu file của bạn là .xls, vui lòng mở file trên Excel và Lưu lại (Save As) dưới dạng '.xlsx'.", 0, 0);
            }
            if (fileExt != ".xlsx")
            {
                return (false, "Định dạng file không được hỗ trợ. Vui lòng chọn file Excel (.xlsx).", 0, 0);
            }

            if (namHoc <= 0)
                return (false, "Vui lòng chọn năm học hợp lệ.", 0, 0);

            ExcelHelper.EnsureLicenseContext();

            int totalImported = 0;
            int totalSkipped = 0;

            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var sheet = package.Workbook.Worksheets.FirstOrDefault();
                if (sheet == null || sheet.Dimension == null)
                    return (false, "File Excel không chứa dữ liệu hoặc sai định dạng.", 0, 0);

                int startRow = 2; // Row 1 is Header
                int endRow = sheet.Dimension.End.Row;

                // Check if row 1 is actually header. If first cell is non-numeric "STT", start at row 2.
                var firstCellValue = sheet.Cells[1, 1].Value?.ToString();

                var newRecords = new List<DiemCong>();

                for (int r = startRow; r <= endRow; r++)
                {
                    var ddcn = ExcelHelper.ParseString(sheet.Cells[r, 2].Value);
                    var hoTen = ExcelHelper.ParseString(sheet.Cells[r, 3].Value);

                    if (string.IsNullOrWhiteSpace(ddcn) && string.IsNullOrWhiteSpace(hoTen))
                    {
                        totalSkipped++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(ddcn) || string.IsNullOrWhiteSpace(hoTen))
                    {
                        totalSkipped++;
                        continue;
                    }

                    var dobVal = sheet.Cells[r, 4].Value;
                    DateTime dob = ParseDate(dobVal);

                    var maXetTuyen = ExcelHelper.ParseString(sheet.Cells[r, 5].Value) ?? string.Empty;
                    var maPTXT = ExcelHelper.ParseString(sheet.Cells[r, 6].Value) ?? string.Empty;
                    var maToHop = ExcelHelper.ParseString(sheet.Cells[r, 7].Value) ?? string.Empty;
                    var loaiDiemCong = ExcelHelper.ParseInt(sheet.Cells[r, 8].Value) ?? 0;
                    var diem = ExcelHelper.ParseDecimal(sheet.Cells[r, 9].Value) ?? 0m;

                    newRecords.Add(new DiemCong
                    {
                        DDCN = ddcn.Trim(),
                        HoTen = hoTen.Trim(),
                        DOB = dob,
                        MaXetTuyen = maXetTuyen.Trim(),
                        MaPTXT = maPTXT.Trim(),
                        MaToHop = maToHop.Trim(),
                        LoaiDiemCong = loaiDiemCong,
                        Diem = diem,
                        NamHoc = namHoc
                    });
                }

                if (!newRecords.Any())
                {
                    return (false, "Không tìm thấy dữ liệu hợp lệ trong file Excel.", 0, totalSkipped);
                }

                if (overwriteExisting)
                {
                    var existingRecords = await _context.DiemCongs.Where(x => x.NamHoc == namHoc).ToListAsync();
                    if (existingRecords.Any())
                    {
                        _context.DiemCongs.RemoveRange(existingRecords);
                    }
                }

                await _context.DiemCongs.AddRangeAsync(newRecords);
                await _context.SaveChangesAsync();

                totalImported = newRecords.Count;
                string msg = $"Import thành công {totalImported} dòng dữ liệu điểm cộng năm {namHoc}.";
                if (totalSkipped > 0)
                {
                    msg += $" Bỏ qua {totalSkipped} dòng không hợp lệ.";
                }

                return (true, msg, totalImported, totalSkipped);
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi xử lý file Excel: " + ex.Message, 0, 0);
            }
        }

        public async Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelAsync(int? namHoc = null, string? search = null)
        {
            var data = await LayDanhSachDiemCongAsync(namHoc, search);
            if (!data.Any())
                return (false, "Không có dữ liệu điểm cộng để xuất Excel.", null);

            ExcelHelper.EnsureLicenseContext();
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("DiemCong");

            string[] headers = new[] { "STT", "ĐDCN", "Họ Tên", "Ngày sinh", "Mã xét tuyển", "Mã PTXT", "Mã tổ hợp", "Loại điểm cộng", "Điểm Cộng", "Năm Học" };
            ExcelHelper.FormatHeaderRow(sheet, headers);

            int row = 2;
            int stt = 1;
            foreach (var item in data)
            {
                sheet.Cells[row, 1].Value = stt++;
                sheet.Cells[row, 2].Value = item.DDCN;
                sheet.Cells[row, 3].Value = item.HoTen;
                sheet.Cells[row, 4].Value = item.DOB == DateTime.MinValue ? "" : item.DOB.ToString("dd/MM/yyyy");
                sheet.Cells[row, 5].Value = item.MaXetTuyen;
                sheet.Cells[row, 6].Value = item.MaPTXT;
                sheet.Cells[row, 7].Value = item.MaToHop;
                sheet.Cells[row, 8].Value = item.LoaiDiemCong;
                sheet.Cells[row, 9].Value = item.Diem;
                sheet.Cells[row, 10].Value = item.NamHoc;

                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            string fileName = namHoc.HasValue ? $"DanhSach_DiemCong_{namHoc.Value}.xlsx" : "DanhSach_DiemCong_TatCa.xlsx";
            return (true, fileName, package.GetAsByteArray());
        }

        public async Task<(bool Success, string Message, byte[]? FileContents)> TaoFileMauExcelAsync()
        {
            ExcelHelper.EnsureLicenseContext();
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Mau_DiemCong");

            string[] headers = new[] { "STT", "ĐDCN", "Họ Tên", "Ngày sinh", "Mã xét tuyển", "Mã PTXT", "Mã tổ hợp", "Loại điểm cộng", "Điểm Cộng" };
            ExcelHelper.FormatHeaderRow(sheet, headers);

            // Add sample row
            sheet.Cells[2, 1].Value = 1;
            sheet.Cells[2, 2].Value = "051208004175";
            sheet.Cells[2, 3].Value = "PHẠM THÀNH LONG";
            sheet.Cells[2, 4].Value = "01/12/2008";
            sheet.Cells[2, 5].Value = "7510103";
            sheet.Cells[2, 6].Value = "407";
            sheet.Cells[2, 7].Value = "";
            sheet.Cells[2, 8].Value = 2;
            sheet.Cells[2, 9].Value = 1.25;

            sheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            return await Task.FromResult((true, "Mau_Import_DiemCong.xlsx", package.GetAsByteArray()));
        }

        private DateTime ParseDate(object? val)
        {
            if (val == null) return DateTime.MinValue;

            if (val is DateTime dt) return dt;

            if (val is double dbl)
            {
                try { return DateTime.FromOADate(dbl); } catch { }
            }

            var str = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(str)) return DateTime.MinValue;

            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy", "d/M/yy", "dd/MM/yy" };
            if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }

            if (DateTime.TryParse(str, out var generalDate))
            {
                return generalDate;
            }

            return DateTime.MinValue;
        }
    }
}
