using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/template")]
    public class TemplateController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TemplateController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadTemplate(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return ReturnErrorWithAlert("Tệp tin mẫu không hợp lệ.");
            }

            var safeFileName = Path.GetFileName(fileName);

            // Thư mục đích trong wwwroot/excel-mau
            var webRootPath = _hostingEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var wwwrootExcelMauFolder = Path.Combine(webRootPath, "excel-mau");
            var targetFilePath = Path.Combine(wwwrootExcelMauFolder, safeFileName);

            if (System.IO.File.Exists(targetFilePath))
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(targetFilePath);
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", safeFileName);
            }

            return ReturnErrorWithAlert($"Không tìm thấy tệp tin mẫu Excel '{safeFileName}' trong thư mục wwwroot/excel-mau.");
        }

        private IActionResult ReturnErrorWithAlert(string errorMessage)
        {
            TempData["Error"] = errorMessage;
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return Redirect("/admin");
        }
    }
}
