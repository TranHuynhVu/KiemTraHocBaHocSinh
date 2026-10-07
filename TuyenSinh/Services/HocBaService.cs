using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;
using TuyenSinh.Services.Excel;
using TuyenSinh.Services.HocBa;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services
{
    public sealed class HocBaService(
        IHocBaCheckService checkService,
        IHocBaDoiChieuService doiChieuService,
        IHocBaDiemSanService diemSanService,
        IHocBaExcelService excelService,
        ILogger<HocBaService> logger) : IHocBaService
    {
        public Task<List<HocBaTHPTImport>?> GetPreviewDataAsync(string excelId, int? limit = null)
        {
            return checkService.GetPreviewDataAsync(excelId, limit);
        }

        public Task<KetQuaKiemTraHocBa> CheckHocBaAsync(string excelId)
        {
            return checkService.CheckHocBaAsync(excelId);
        }

        public Task<KetQuaDoiChieu> DoiChieuHocBaVaNguyenVongAsync(string hocBaFileId, string nguyenVongFileId)
        {
            return doiChieuService.DoiChieuAsync(hocBaFileId, nguyenVongFileId);
        }

        public Task<KetQuaKiemTraDiemSan> KiemTraDiemSan(string maNganh, string fileId)
        {
            return diemSanService.KiemTraDiemSanAsync(maNganh, fileId);
        }

        public Task<List<Nganh>> LayDanhSachNganhAsync()
        {
            return diemSanService.LayDanhSachNganhAsync();
        }

        public async Task<ServiceResult<FileDownloadDto>> XuatExcelThieuDiemToHopAsync(string excelId)
        {
            if (string.IsNullOrEmpty(excelId))
            {
                return ServiceResult<FileDownloadDto>.Fail("Mã tập Excel không hợp lệ.");
            }

            var result = await checkService.CheckHocBaAsync(excelId);
            if (!result.ThanhCong)
            {
                return ServiceResult<FileDownloadDto>.Fail(result.ThongBao ?? "Kiểm tra học bạ không thành công.");
            }

            try
            {
                var fileDto = excelService.ExportThieuDiemToHop(result);
                return ServiceResult<FileDownloadDto>.Ok(fileDto, fileDto.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xuất Excel thiếu điểm tổ hợp cho file {ExcelId}.", excelId);
                return ServiceResult<FileDownloadDto>.Fail("Lỗi khi xuất file Excel: " + ex.Message);
            }
        }

        public async Task<ServiceResult<FileDownloadDto>> XuatExcelKetQuaDoiChieuAsync(string hocBaFileId, string nguyenVongFileId)
        {
            if (string.IsNullOrEmpty(hocBaFileId) || string.IsNullOrEmpty(nguyenVongFileId))
            {
                return ServiceResult<FileDownloadDto>.Fail("Yêu cầu không hợp lệ.");
            }

            var result = await doiChieuService.DoiChieuAsync(hocBaFileId, nguyenVongFileId);
            if (!result.ThanhCong)
            {
                return ServiceResult<FileDownloadDto>.Fail(result.ThongBao ?? "Đối chiếu không thành công.");
            }

            try
            {
                var fileDto = excelService.ExportKetQuaDoiChieu(result);
                return ServiceResult<FileDownloadDto>.Ok(fileDto, fileDto.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xuất Excel kết quả đối chiếu.");
                return ServiceResult<FileDownloadDto>.Fail("Lỗi khi xuất file Excel: " + ex.Message);
            }
        }

        public async Task<ServiceResult<FileDownloadDto>> XuatExcelKiemTraDiemSanAsync(string maNganh, string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return ServiceResult<FileDownloadDto>.Fail("Yêu cầu không hợp lệ.");
            }

            var result = await diemSanService.KiemTraDiemSanAsync(maNganh, fileId);
            if (!result.ThanhCong)
            {
                return ServiceResult<FileDownloadDto>.Fail(result.ThongBao ?? "Kiểm tra điểm sàn không thành công.");
            }

            try
            {
                var fileDto = excelService.ExportKiemTraDiemSan(result);
                return ServiceResult<FileDownloadDto>.Ok(fileDto, fileDto.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xuất Excel kiểm tra điểm sàn.");
                return ServiceResult<FileDownloadDto>.Fail("Lỗi khi xuất file Excel: " + ex.Message);
            }
        }
    }
}
