using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TuyenSinh.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public FileStorageService(IWebHostEnvironment hostingEnvironment, IBackgroundJobClient backgroundJobClient)
        {
            _hostingEnvironment = hostingEnvironment;
            _backgroundJobClient = backgroundJobClient;
        }

        public string GetUploadFolder()
        {
            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
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

            _backgroundJobClient.Schedule<IFileStorageService>(s => s.DeleteExpiredFileAsync(fileId), TimeSpan.FromMinutes(expiredMinutes));

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
                catch { }
            }

            await Task.CompletedTask;
        }
    }
}
