using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Data;
using TuyenSinh.Models;
using TuyenSinh.Services.Excel;

namespace TuyenSinh.Services
{
    public sealed class DiemCongService(
        ApplicationDbContext context,
        IDiemCongExcelService excelService,
        ILogger<DiemCongService> logger) : IDiemCongService
    {
        public async Task<List<DiemCong>> LayDanhSachDiemCongAsync(int? namHoc = null, string? search = null)
        {
            var query = context.DiemCongs.AsNoTracking().AsQueryable();

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
            var dbYears = await context.DiemCongs.Select(x => x.NamHoc).Distinct().ToListAsync();

            if (!dbYears.Contains(currentYear))
            {
                dbYears.Add(currentYear);
            }

            return dbYears.OrderByDescending(x => x).ToList();
        }

        public async Task<DiemCong?> LayTheoIdAsync(int id)
        {
            return await context.DiemCongs.FindAsync(id);
        }

        public async Task<ServiceResult> ThemDiemCongAsync(DiemCong model)
        {
            if (string.IsNullOrWhiteSpace(model.DDCN))
                return ServiceResult.Fail("Định danh cá nhân (ĐDCN) không được để trống.");

            if (string.IsNullOrWhiteSpace(model.HoTen))
                return ServiceResult.Fail("Họ tên không được để trống.");

            if (model.NamHoc <= 0)
                return ServiceResult.Fail("Năm học không hợp lệ.");

            try
            {
                model.DDCN = model.DDCN.Trim();
                model.HoTen = model.HoTen.Trim();
                model.MaXetTuyen = model.MaXetTuyen?.Trim() ?? string.Empty;
                model.MaPTXT = model.MaPTXT?.Trim() ?? string.Empty;
                model.MaToHop = model.MaToHop?.Trim() ?? string.Empty;

                context.DiemCongs.Add(model);
                await context.SaveChangesAsync();
                return ServiceResult.Ok("Thêm mới điểm cộng thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi thêm điểm cộng.");
                return ServiceResult.Fail("Lỗi khi thêm điểm cộng: " + ex.Message);
            }
        }

        public async Task<ServiceResult> SuaDiemCongAsync(DiemCong model)
        {
            var entity = await context.DiemCongs.FindAsync(model.Id);
            if (entity == null)
                return ServiceResult.Fail("Không tìm thấy dữ liệu điểm cộng cần sửa.");

            if (string.IsNullOrWhiteSpace(model.DDCN))
                return ServiceResult.Fail("Định danh cá nhân (ĐDCN) không được để trống.");

            if (string.IsNullOrWhiteSpace(model.HoTen))
                return ServiceResult.Fail("Họ tên không được để trống.");

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

                context.DiemCongs.Update(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Ok("Cập nhật thông tin điểm cộng thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi cập nhật điểm cộng.");
                return ServiceResult.Fail("Lỗi khi cập nhật điểm cộng: " + ex.Message);
            }
        }

        public async Task<ServiceResult> XoaDiemCongAsync(int id)
        {
            var entity = await context.DiemCongs.FindAsync(id);
            if (entity == null)
                return ServiceResult.Fail("Không tìm thấy dữ liệu điểm cộng cần xóa.");

            try
            {
                context.DiemCongs.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Ok("Xóa bản ghi điểm cộng thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xóa bản ghi điểm cộng ID {Id}.", id);
                return ServiceResult.Fail("Lỗi khi xóa bản ghi điểm cộng: " + ex.Message);
            }
        }

        public async Task<ServiceResult> XoaTheoNamAsync(int namHoc)
        {
            var records = await context.DiemCongs.Where(x => x.NamHoc == namHoc).ToListAsync();
            if (!records.Any())
                return ServiceResult.Fail($"Không có dữ liệu điểm cộng cho năm học {namHoc}.");

            try
            {
                context.DiemCongs.RemoveRange(records);
                await context.SaveChangesAsync();
                return ServiceResult.Ok($"Đã xóa thành công {records.Count} bản ghi điểm cộng năm {namHoc}.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xóa dữ liệu điểm cộng năm học {NamHoc}.", namHoc);
                return ServiceResult.Fail("Lỗi khi xóa dữ liệu theo năm: " + ex.Message);
            }
        }

        public async Task<ServiceResult<ImportResultData>> ImportExcelAsync(IFormFile file, int namHoc, bool overwriteExisting = false)
        {
            if (file == null || file.Length == 0)
                return ServiceResult<ImportResultData>.Fail("Vui lòng chọn file Excel để upload.");

            var fileExt = Path.GetExtension(file.FileName).ToLower();
            if (fileExt == ".xls")
            {
                return ServiceResult<ImportResultData>.Fail("Hệ thống chỉ hỗ trợ file định dạng Excel 2007 trở lên (.xlsx). Nếu file của bạn là .xls, vui lòng mở file trên Excel và Lưu lại (Save As) dưới dạng '.xlsx'.");
            }
            if (fileExt != ".xlsx")
            {
                return ServiceResult<ImportResultData>.Fail("Định dạng file không được hỗ trợ. Vui lòng chọn file Excel (.xlsx).");
            }

            if (namHoc <= 0)
                return ServiceResult<ImportResultData>.Fail("Vui lòng chọn năm học hợp lệ.");

            try
            {
                using var stream = file.OpenReadStream();
                var (newRecords, totalSkipped) = excelService.ReadImportStream(stream, namHoc);

                if (!newRecords.Any())
                {
                    return ServiceResult<ImportResultData>.Fail("Không tìm thấy dữ liệu hợp lệ trong file Excel.");
                }

                if (overwriteExisting)
                {
                    var existingRecords = await context.DiemCongs.Where(x => x.NamHoc == namHoc).ToListAsync();
                    if (existingRecords.Any())
                    {
                        context.DiemCongs.RemoveRange(existingRecords);
                    }
                }

                await context.DiemCongs.AddRangeAsync(newRecords);
                await context.SaveChangesAsync();

                int totalImported = newRecords.Count;
                string msg = $"Import thành công {totalImported} dòng dữ liệu điểm cộng năm {namHoc}.";
                if (totalSkipped > 0)
                {
                    msg += $" Bỏ qua {totalSkipped} dòng không hợp lệ.";
                }

                return ServiceResult<ImportResultData>.Ok(new ImportResultData(totalImported, totalSkipped), msg);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xử lý file Excel điểm cộng.");
                return ServiceResult<ImportResultData>.Fail("Lỗi khi xử lý file Excel: " + ex.Message);
            }
        }

        public async Task<ServiceResult<FileDownloadDto>> XuatExcelAsync(int? namHoc = null, string? search = null)
        {
            var data = await LayDanhSachDiemCongAsync(namHoc, search);
            if (!data.Any())
                return ServiceResult<FileDownloadDto>.Fail("Không có dữ liệu điểm cộng để xuất Excel.");

            try
            {
                var fileDto = excelService.Export(data, namHoc);
                return ServiceResult<FileDownloadDto>.Ok(fileDto, fileDto.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xuất file Excel điểm cộng.");
                return ServiceResult<FileDownloadDto>.Fail("Lỗi khi xuất file Excel: " + ex.Message);
            }
        }

        public Task<ServiceResult<FileDownloadDto>> TaoFileMauExcelAsync()
        {
            try
            {
                var template = excelService.CreateTemplate();
                return Task.FromResult(ServiceResult<FileDownloadDto>.Ok(template, template.FileName));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi tạo file mẫu Excel điểm cộng.");
                return Task.FromResult(ServiceResult<FileDownloadDto>.Fail("Lỗi khi tạo file mẫu Excel: " + ex.Message));
            }
        }
    }
}
