using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public class NganhService : INganhService
    {
        private readonly ApplicationDbContext _context;

        public NganhService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Nganh>> LayDanhSachNganhAsync()
        {
            return await _context.Nganhs
                .Include(n => n.ToHopNganhs)
                .ThenInclude(th => th.ToHopMon)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> NhapNganhTuExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "Vui lòng chọn tệp Excel.");
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = file.OpenReadStream())
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("Thông tin ĐKXT HB"))
                                    ?? package.Workbook.Worksheets[0];

                    int totalRows = worksheet.Dimension.End.Row;
                    int startRow = 7;

                    var existingToHops = await _context.ToHopMons.ToListAsync();

                    // Clean existing data for fresh reload
                    _context.ToHopNganhs.RemoveRange(_context.ToHopNganhs);
                    _context.Nganhs.RemoveRange(_context.Nganhs);
                    await _context.SaveChangesAsync();

                    for (int r = startRow; r <= totalRows; r++)
                    {
                        var sttVal = worksheet.Cells[r, 1].Value?.ToString();
                        if (string.IsNullOrWhiteSpace(sttVal)) continue;

                        var tenNganh = worksheet.Cells[r, 2].Value?.ToString()?.Trim();
                        var maNganh = worksheet.Cells[r, 3].Value?.ToString()?.Trim();
                        var heSoThptStr = worksheet.Cells[r, 4].Value?.ToString();
                        var heSoHbStr = worksheet.Cells[r, 5].Value?.ToString();
                        var toHopCodesStr = worksheet.Cells[r, 6].Value?.ToString()?.Trim();
                        var nguongDauVao = worksheet.Cells[r, 8].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(maNganh) || string.IsNullOrEmpty(tenNganh)) continue;

                        float heSoThpt = 0;
                        float heSoHb = 0;
                        float.TryParse(heSoThptStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out heSoThpt);
                        float.TryParse(heSoHbStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out heSoHb);

                        var nganh = new Nganh
                        {
                            MaNganh = maNganh,
                            TenNganh = tenNganh,
                            HeSoTHPT = heSoThpt,
                            HeSoHB = heSoHb,
                            ToHopXetTuyen = toHopCodesStr,
                            NgungDauVao = nguongDauVao
                        };

                        _context.Nganhs.Add(nganh);
                        await _context.SaveChangesAsync();

                        if (!string.IsNullOrEmpty(toHopCodesStr))
                        {
                            var codes = toHopCodesStr.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(c => c.Trim().ToUpper())
                                                    .Distinct();

                            foreach (var code in codes)
                            {
                                var toHop = existingToHops.FirstOrDefault(t => t.MaToHop.Trim().ToUpper() == code);
                                if (toHop == null)
                                {
                                    toHop = new ToHopMon
                                    {
                                        MaToHop = code,
                                        TenToHop = "Tổ hợp " + code
                                    };
                                    _context.ToHopMons.Add(toHop);
                                    await _context.SaveChangesAsync();
                                    existingToHops.Add(toHop);
                                }

                                var link = new ToHopNganh
                                {
                                    MaNganhId = nganh.Id,
                                    ToHopId = toHop.Id
                                };
                                _context.ToHopNganhs.Add(link);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    return (true, "Nhập dữ liệu danh sách ngành tuyển sinh từ Excel thành công!");
                }
            }
            catch (Exception ex)
            {
                return (false, "Lỗi khi import file Excel: " + ex.Message);
            }
        }
    }
}
