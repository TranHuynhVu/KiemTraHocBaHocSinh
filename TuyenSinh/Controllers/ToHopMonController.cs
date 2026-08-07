using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/to-hop-mon")]
    public class ToHopMonController : Controller
    {
        private readonly IToHopMonService _toHopMonService;
        private readonly IMonHocService _monHocService;

        public ToHopMonController(IToHopMonService toHopMonService, IMonHocService monHocService)
        {
            _toHopMonService = toHopMonService;
            _monHocService = monHocService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var combinations = await _toHopMonService.LayDanhSachToHopAsync();
            ViewBag.Subjects = await _monHocService.LayDanhSachMonHocAsync();
            return View("Index", combinations);
        }

        [HttpPost("them")]
        public async Task<IActionResult> ThemToHopMon(string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            var result = await _toHopMonService.ThemToHopAsync(maToHop, tenToHop, selectedSubjectIds);
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
        public async Task<IActionResult> SuaToHopMon(int id, string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            var result = await _toHopMonService.SuaToHopAsync(id, maToHop, tenToHop, selectedSubjectIds);
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
        public async Task<IActionResult> XoaToHopMon(int id)
        {
            var result = await _toHopMonService.XoaToHopAsync(id);
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
