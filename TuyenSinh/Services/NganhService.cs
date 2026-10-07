using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public sealed class NganhService(ApplicationDbContext context, ILogger<NganhService> logger) : INganhService
    {
        public async Task<List<Nganh>> LayDanhSachNganhAsync()
        {
            return await context.Nganhs
                .Include(n => n.ToHopNganhs)
                .ThenInclude(th => th.ToHopMon)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ServiceResult> NhapNganhTuExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ServiceResult.Fail("Vui lòng chọn tệp Excel.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                return ServiceResult.Fail("Chỉ chấp nhận tệp tin Excel định dạng .xlsx.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("Thông tin ĐKXT HB"))
                                ?? package.Workbook.Worksheets[0];

                if (worksheet?.Dimension == null)
                {
                    return ServiceResult.Fail("Tệp Excel không chứa dữ liệu.");
                }

                int totalRows = worksheet.Dimension.End.Row;
                int startRow = 7;

                var existingToHops = await context.ToHopMons.ToListAsync();
                var missingToHops = new HashSet<string>();

                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    context.ToHopNganhs.RemoveRange(context.ToHopNganhs);
                    context.Nganhs.RemoveRange(context.Nganhs);
                    await context.SaveChangesAsync();

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
                        var dxt = worksheet.Cells[r, 9].Value?.ToString()?.Trim();
                        var diemSanToan = worksheet.Cells[r, 10].Value?.ToString()?.Trim();
                        if (string.IsNullOrEmpty(maNganh) || string.IsNullOrEmpty(tenNganh)) continue;

                        float.TryParse(heSoThptStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float heSoThpt);
                        float.TryParse(heSoHbStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float heSoHb);

                        var nganh = new Nganh
                        {
                            MaNganh = maNganh,
                            TenNganh = tenNganh,
                            HeSoTHPT = heSoThpt,
                            HeSoHB = heSoHb,
                            ToHopXetTuyen = toHopCodesStr ?? string.Empty,
                            NguongDauVao = nguongDauVao ?? string.Empty,
                            DXT = decimal.TryParse(dxt, out var dxtValue) ? dxtValue : 0,
                            DiemSanToan = decimal.TryParse(diemSanToan, out var diemValue) ? diemValue : 0
                        };

                        context.Nganhs.Add(nganh);
                        await context.SaveChangesAsync();

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
                                    missingToHops.Add(code);
                                }
                                else
                                {
                                    var link = new ToHopNganh
                                    {
                                        MaNganhId = nganh.Id,
                                        ToHopId = toHop.Id
                                    };
                                    context.ToHopNganhs.Add(link);
                                }
                            }
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (missingToHops.Any())
                    {
                        var missingStr = string.Join(", ", missingToHops);
                        return ServiceResult.Fail($"Nhập dữ liệu hoàn tất. Tuy nhiên, các tổ hợp sau chưa tồn tại trong hệ thống và bị bỏ qua: {missingStr}");
                    }

                    return ServiceResult.Ok("Nhập dữ liệu danh sách ngành tuyển sinh từ Excel thành công!");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Lỗi xảy ra trong quá trình lưu dữ liệu ngành tuyển sinh.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi import file Excel ngành tuyển sinh.");
                return ServiceResult.Fail("Lỗi khi import file Excel: " + ex.Message);
            }
        }
    }
}
