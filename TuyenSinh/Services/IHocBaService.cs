using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Models;
using TuyenSinh.ViewModels;

namespace TuyenSinh.Services
{
    public interface IHocBaService
    {
        Task<List<HocBaTHPTImport>?> GetPreviewDataAsync(string excelId, int? limit = null);
        Task<KetQuaKiemTraHocBa> CheckHocBaAsync(string excelId);
        Task<KetQuaDoiChieu> DoiChieuHocBaVaNguyenVongAsync(string hocBaFileId, string nguyenVongFileId);
        Task<KetQuaKiemTraDiemSan> KiemTraDiemSan(string maNganh, string fileId);
        Task<List<Nganh>> LayDanhSachNganhAsync();
        
        Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelThieuDiemToHopAsync(string excelId);
        Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelKetQuaDoiChieuAsync(string hocBaFileId, string nguyenVongFileId);
        Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelKiemTraDiemSanAsync(string maNganh, string fileId);
    }
}
