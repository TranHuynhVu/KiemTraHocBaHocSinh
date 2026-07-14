using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
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

        [HttpGet("")]
        public async Task<IActionResult> TongQuan()
        {
            var subjects = await _monHocService.LayDanhSachMonHocAsync();
            var combinations = await _toHopMonService.LayDanhSachToHopAsync();
            ViewBag.CountSubjects = subjects.Count;
            ViewBag.CountCombinations = combinations.Count;
            return View("Index");
        }

        #region Quản lý môn học (MonHoc CRUD)

        [HttpGet("mon-hoc")]
        public async Task<IActionResult> QuanLyMonHoc()
        {
            var list = await _monHocService.LayDanhSachMonHocAsync();
            return View("MonHoc", list);
        }

        [HttpPost("mon-hoc/them")]
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
            return RedirectToAction(nameof(QuanLyMonHoc));
        }

        [HttpPost("mon-hoc/sua")]
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
            return RedirectToAction(nameof(QuanLyMonHoc));
        }

        [HttpPost("mon-hoc/xoa")]
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
            return RedirectToAction(nameof(QuanLyMonHoc));
        }

        #endregion

        #region Quản lý tổ hợp (ToHopMon CRUD)

        [HttpGet("to-hop-mon")]
        public async Task<IActionResult> QuanLyToHopMon()
        {
            var combinations = await _toHopMonService.LayDanhSachToHopAsync();
            ViewBag.Subjects = await _monHocService.LayDanhSachMonHocAsync();
            return View("ToHopMon", combinations);
        }

        [HttpPost("to-hop-mon/them")]
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
            return RedirectToAction(nameof(QuanLyToHopMon));
        }

        [HttpPost("to-hop-mon/sua")]
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
            return RedirectToAction(nameof(QuanLyToHopMon));
        }

        [HttpPost("to-hop-mon/xoa")]
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
            return RedirectToAction(nameof(QuanLyToHopMon));
        }

        #endregion

        #region Quản lý ngành học (Nganh CRUD)

        [HttpGet("nganh")]
        public async Task<IActionResult> QuanLyNganh()
        {
            var list = await _nganhService.LayDanhSachNganhAsync();
            return View("Nganh", list);
        }

        [HttpPost("nganh/nhap-excel")]
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
            return RedirectToAction(nameof(QuanLyNganh));
        }

        #endregion
    }
}
