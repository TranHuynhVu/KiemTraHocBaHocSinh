using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TuyenSinh.Services
{
    public sealed class FileStorageService(
        IWebHostEnvironment hostingEnvironment,
        IBackgroundJobClient backgroundJobClient,
        ILogger<FileStorageService> logger) : IFileStorageService
    {

        public string GetUploadFolder()
        {
            var webRootPath = hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            return uploadsFolder;
        }

        public string GetUploadPath(string fileId)
        {
            var uploadsFolder = GetUploadFolder();
            return Path.Combine(uploadsFolder, fileId);
        }

        public bool FileExists(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId)) return false;
            var filePath = GetUploadPath(fileId);
            return File.Exists(filePath);
        }

        public async Task<string> LuuFileTamThoiAsync(IFormFile file, string allowedExtension = ".xlsx", int expiredMinutes = 30)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Tệp tin trống.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!string.IsNullOrEmpty(allowedExtension) && extension != allowedExtension.ToLower())
            {
                throw new ArgumentException($"Chỉ chấp nhận tệp tin định dạng {allowedExtension}.");
            }

            var uploadsFolder = GetUploadFolder();
            var fileId = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileId);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            backgroundJobClient.Schedule<IFileStorageService>(s => s.DeleteExpiredFileAsync(fileId), TimeSpan.FromMinutes(expiredMinutes));

            return fileId;
        }

        public async Task DeleteExpiredFileAsync(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId)) return;
            var filePath = GetUploadPath(fileId);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Không thể xóa file tạm đã hết hạn: {FilePath}", filePath);
                }
            }

            await Task.CompletedTask;
        }
    }
}
