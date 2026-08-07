using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IDiemCongService
    {
        Task<List<DiemCong>> LayDanhSachDiemCongAsync(int? namHoc = null, string? search = null);
        Task<List<int>> LayDanhSachNamHocAsync();
        Task<DiemCong?> LayTheoIdAsync(int id);
        Task<(bool Success, string Message)> ThemDiemCongAsync(DiemCong model);
        Task<(bool Success, string Message)> SuaDiemCongAsync(DiemCong model);
        Task<(bool Success, string Message)> XoaDiemCongAsync(int id);
        Task<(bool Success, string Message)> XoaTheoNamAsync(int namHoc);
        Task<(bool Success, string Message, int TotalImported, int TotalSkipped)> ImportExcelAsync(IFormFile file, int namHoc, bool overwriteExisting = false);
        Task<(bool Success, string Message, byte[]? FileContents)> XuatExcelAsync(int? namHoc = null, string? search = null);
        Task<(bool Success, string Message, byte[]? FileContents)> TaoFileMauExcelAsync();
    }
}
