using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.ViewModels;

namespace TuyenSinh.Services
{
    public interface IHocBaService
    {
        Task<(bool Success, string Message, string? ExcelId, List<HocBaPreviewItem>? PreviewData)> UploadAndPreviewAsync(IFormFile file);
        Task<List<HocBaTHPTImport>?> GetPreviewDataAsync(string excelId, int? limit = null);
        Task<KetQuaKiemTraHocBa> CheckHocBaAsync(string excelId);
        Task DeleteExpiredFileAsync(string excelId);

    }
}
