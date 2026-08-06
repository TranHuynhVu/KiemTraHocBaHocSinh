using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TuyenSinh.ViewModels;

namespace TuyenSinh.Services
{
    public interface ISoKhopNgoaiNguService
    {
        Task<SoKhopNgoaiNguThongKeViewModel> Join3ExcelFilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null);
        Task<(bool Success, string Message, byte[]? FileContents)> XuatExcel3FilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null);
    }
}
