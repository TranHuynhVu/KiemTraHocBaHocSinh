using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/to-hop-mon")]
    public class ToHopMonController(IToHopMonService toHopMonService, IMonHocService monHocService) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var combinations = await toHopMonService.LayDanhSachToHopAsync();
            ViewBag.Subjects = await monHocService.LayDanhSachMonHocAsync();
            return View("Index", combinations);
        }

        [HttpPost("them")]
        public async Task<IActionResult> ThemToHopMon(string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            var result = await toHopMonService.ThemToHopAsync(maToHop, tenToHop, selectedSubjectIds);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("sua")]
        public async Task<IActionResult> SuaToHopMon(int id, string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            var result = await toHopMonService.SuaToHopAsync(id, maToHop, tenToHop, selectedSubjectIds);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("xoa")]
        public async Task<IActionResult> XoaToHopMon(int id)
        {
            var result = await toHopMonService.XoaToHopAsync(id);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }
    }
}
