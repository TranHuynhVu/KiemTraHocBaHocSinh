using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMonHocService _monHocService;
        private readonly IToHopMonService _toHopMonService;
        private readonly INganhService _nganhService;

        public AdminController(
            IMonHocService monHocService,
            IToHopMonService toHopMonService,
            INganhService nganhService)
        {
            _monHocService = monHocService;
            _toHopMonService = toHopMonService;
            _nganhService = nganhService;
        }

        public async Task<IActionResult> Index()
        {
            var subjects = await _monHocService.LayDanhSachMonHocAsync();
            var combinations = await _toHopMonService.LayDanhSachToHopAsync();
            ViewBag.CountSubjects = subjects.Count;
            ViewBag.CountCombinations = combinations.Count;
            return View();
        }

        #region Quản lý môn học (MonHoc CRUD)

        [HttpGet]
        public async Task<IActionResult> MonHoc()
        {
            var list = await _monHocService.LayDanhSachMonHocAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMonHoc(string tenMonHoc, string fieldName)
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
            return RedirectToAction(nameof(MonHoc));
        }

        [HttpPost]
        public async Task<IActionResult> EditMonHoc(int id, string tenMonHoc, string fieldName)
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
            return RedirectToAction(nameof(MonHoc));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMonHoc(int id)
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
            return RedirectToAction(nameof(MonHoc));
        }

        #endregion

        #region Quản lý tổ hợp (ToHopMon CRUD)

        [HttpGet]
        public async Task<IActionResult> ToHopMon()
        {
            var combinations = await _toHopMonService.LayDanhSachToHopAsync();
            ViewBag.Subjects = await _monHocService.LayDanhSachMonHocAsync();
            return View(combinations);
        }

        [HttpPost]
        public async Task<IActionResult> CreateToHopMon(string maToHop, string tenToHop, List<int> selectedSubjectIds)
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
            return RedirectToAction(nameof(ToHopMon));
        }

        [HttpPost]
        public async Task<IActionResult> EditToHopMon(int id, string maToHop, string tenToHop, List<int> selectedSubjectIds)
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
            return RedirectToAction(nameof(ToHopMon));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteToHopMon(int id)
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
            return RedirectToAction(nameof(ToHopMon));
        }

        #endregion

        #region Quản lý ngành học (Nganh CRUD)

        [HttpGet]
        public async Task<IActionResult> Nganh()
        {
            var list = await _nganhService.LayDanhSachNganhAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ImportNganh(IFormFile file)
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
            return RedirectToAction(nameof(Nganh));
        }

        #endregion
    }
}
