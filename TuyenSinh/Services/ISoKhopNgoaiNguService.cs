using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.ViewModels.SoKhopNgoaiNgu;

namespace TuyenSinh.Services
{
    public interface ISoKhopNgoaiNguService
    {
        Task<SoKhopNgoaiNguThongKeViewModel> Join3ExcelFilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null);
        Task<ServiceResult<FileDownloadDto>> XuatExcel3FilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null);
    }
}
