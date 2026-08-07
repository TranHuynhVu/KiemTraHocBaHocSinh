using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/mon-hoc")]
    public class MonHocController : Controller
    {
        private readonly IMonHocService _monHocService;

        public MonHocController(IMonHocService monHocService)
        {
            _monHocService = monHocService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var list = await _monHocService.LayDanhSachMonHocAsync();
            return View("Index", list);
        }

        [HttpPost("them")]
        public async Task<IActionResult> ThemMonHoc(string tenMonHoc, string fieldName)
        {
            var result = await _monHocService.ThemMonHocAsync(tenMonHoc, fieldName);
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

        [HttpPost("sua")]
        public async Task<IActionResult> SuaMonHoc(int id, string tenMonHoc, string fieldName)
        {
            var result = await _monHocService.SuaMonHocAsync(id, tenMonHoc, fieldName);
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

        [HttpPost("xoa")]
        public async Task<IActionResult> XoaMonHoc(int id)
        {
            var result = await _monHocService.XoaMonHocAsync(id);
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
