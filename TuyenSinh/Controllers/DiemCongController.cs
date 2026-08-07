using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Models;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/diem-cong")]
    public class DiemCongController : Controller
    {
        private readonly IDiemCongService _diemCongService;

        public DiemCongController(IDiemCongService diemCongService)
        {
            _diemCongService = diemCongService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? namHoc)
        {
            var dsNamHoc = await _diemCongService.LayDanhSachNamHocAsync();

            int selectedYear = namHoc ?? (dsNamHoc.Count > 0 ? dsNamHoc[0] : System.DateTime.Now.Year);

            var danhSach = await _diemCongService.LayDanhSachDiemCongAsync(selectedYear);

            ViewBag.DanhSachNamHoc = dsNamHoc;
            ViewBag.NamHocHienTai = selectedYear;

            return View(danhSach);
        }

        [HttpPost("them")]
        public async Task<IActionResult> Them(DiemCong model)
        {
            var result = await _diemCongService.ThemDiemCongAsync(model);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { namHoc = model.NamHoc });
        }

        [HttpPost("sua")]
        public async Task<IActionResult> Sua(DiemCong model)
        {
            var result = await _diemCongService.SuaDiemCongAsync(model);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { namHoc = model.NamHoc });
        }

        [HttpPost("xoa")]
        public async Task<IActionResult> Xoa(int id, int? namHoc)
        {
            var result = await _diemCongService.XoaDiemCongAsync(id);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { namHoc });
        }

        [HttpPost("xoa-theo-nam")]
        public async Task<IActionResult> XoaTheoNam(int namHoc)
        {
            var result = await _diemCongService.XoaTheoNamAsync(namHoc);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { namHoc });
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile fileExcel, int namHoc, bool overwriteExisting = false)
        {
            var result = await _diemCongService.ImportExcelAsync(fileExcel, namHoc, overwriteExisting);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { namHoc });
        }

        [HttpGet("xuat-excel")]
        public async Task<IActionResult> XuatExcel(int? namHoc)
        {
            var result = await _diemCongService.XuatExcelAsync(namHoc);
            if (!result.Success || result.FileContents == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index), new { namHoc });
            }

            return File(result.FileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Message);
        }

        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            var result = await _diemCongService.TaoFileMauExcelAsync();
            if (!result.Success || result.FileContents == null)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return File(result.FileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Message);
        }
    }
}
