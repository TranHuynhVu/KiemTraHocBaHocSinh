using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IDiemCongService
    {
        Task<List<DiemCong>> LayDanhSachDiemCongAsync(int? namHoc = null, string? search = null);
        Task<List<int>> LayDanhSachNamHocAsync();
        Task<DiemCong?> LayTheoIdAsync(int id);
        Task<ServiceResult> ThemDiemCongAsync(DiemCong model);
        Task<ServiceResult> SuaDiemCongAsync(DiemCong model);
        Task<ServiceResult> XoaDiemCongAsync(int id);
        Task<ServiceResult> XoaTheoNamAsync(int namHoc);
        Task<ServiceResult<ImportResultData>> ImportExcelAsync(IFormFile file, int namHoc, bool overwriteExisting = false);
        Task<ServiceResult<FileDownloadDto>> XuatExcelAsync(int? namHoc = null, string? search = null);
        Task<ServiceResult<FileDownloadDto>> TaoFileMauExcelAsync();
    }
}
