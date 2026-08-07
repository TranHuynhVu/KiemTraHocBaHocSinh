using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/nganh")]
    public class NganhController : Controller
    {
        private readonly INganhService _nganhService;

        public NganhController(INganhService nganhService)
        {
            _nganhService = nganhService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var list = await _nganhService.LayDanhSachNganhAsync();
            return View("Index", list);
        }

        [HttpPost("nhap-excel")]
        public async Task<IActionResult> NhapNganhTuExcel(IFormFile file)
        {
            var result = await _nganhService.NhapNganhTuExcelAsync(file);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
