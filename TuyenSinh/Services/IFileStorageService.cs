using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TuyenSinh.Services
{
    public interface IFileStorageService
    {
        Task<string> LuuFileTamThoiAsync(IFormFile file, string allowedExtension = ".xlsx", int expiredMinutes = 30);
        Task DeleteExpiredFileAsync(string fileId);
        string GetUploadPath(string fileId);
        bool FileExists(string fileId);
    }
}
