using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/mon-hoc")]
    public class MonHocController(IMonHocService monHocService) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var list = await monHocService.LayDanhSachMonHocAsync();
            return View("Index", list);
        }

        [HttpPost("them")]
        public async Task<IActionResult> ThemMonHoc(string tenMonHoc, string fieldName)
        {
            var result = await monHocService.ThemMonHocAsync(tenMonHoc, fieldName);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("sua")]
        public async Task<IActionResult> SuaMonHoc(int id, string tenMonHoc, string fieldName)
        {
            var result = await monHocService.SuaMonHocAsync(id, tenMonHoc, fieldName);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("xoa")]
        public async Task<IActionResult> XoaMonHoc(int id)
        {
            var result = await monHocService.XoaMonHocAsync(id);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }
    }
}

