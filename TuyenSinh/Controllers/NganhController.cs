using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/nganh")]
    public class NganhController(INganhService nganhService) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var list = await nganhService.LayDanhSachNganhAsync();
            return View("Index", list);
        }

        [HttpPost("nhap-excel")]
        public async Task<IActionResult> NhapNganhTuExcel(IFormFile file)
        {
            var result = await nganhService.NhapNganhTuExcelAsync(file);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }
    }
}

