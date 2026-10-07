using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services
{
    public interface IHocBaService
    {
        Task<List<HocBaTHPTImport>?> GetPreviewDataAsync(string excelId, int? limit = null);
        Task<KetQuaKiemTraHocBa> CheckHocBaAsync(string excelId);
        Task<KetQuaDoiChieu> DoiChieuHocBaVaNguyenVongAsync(string hocBaFileId, string nguyenVongFileId);
        Task<KetQuaKiemTraDiemSan> KiemTraDiemSan(string maNganh, string fileId);
        Task<List<Nganh>> LayDanhSachNganhAsync();
        
        Task<ServiceResult<FileDownloadDto>> XuatExcelThieuDiemToHopAsync(string excelId);
        Task<ServiceResult<FileDownloadDto>> XuatExcelKetQuaDoiChieuAsync(string hocBaFileId, string nguyenVongFileId);
        Task<ServiceResult<FileDownloadDto>> XuatExcelKiemTraDiemSanAsync(string maNganh, string fileId);
    }
}
