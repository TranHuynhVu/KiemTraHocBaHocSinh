using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/diem-cong")]
    public class DiemCongController(IDiemCongService diemCongService) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index(int? namHoc)
        {
            var dsNamHoc = await diemCongService.LayDanhSachNamHocAsync();

            int selectedYear = namHoc ?? (dsNamHoc.Count > 0 ? dsNamHoc[0] : System.DateTime.Now.Year);

            var danhSach = await diemCongService.LayDanhSachDiemCongAsync(selectedYear);

            ViewBag.DanhSachNamHoc = dsNamHoc;
            ViewBag.NamHocHienTai = selectedYear;

            return View(danhSach);
        }

        [HttpPost("them")]
        public async Task<IActionResult> Them(DiemCong model)
        {
            var result = await diemCongService.ThemDiemCongAsync(model);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index), new { namHoc = model.NamHoc });
        }

        [HttpPost("sua")]
        public async Task<IActionResult> Sua(DiemCong model)
        {
            var result = await diemCongService.SuaDiemCongAsync(model);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index), new { namHoc = model.NamHoc });
        }

        [HttpPost("xoa")]
        public async Task<IActionResult> Xoa(int id, int? namHoc)
        {
            var result = await diemCongService.XoaDiemCongAsync(id);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index), new { namHoc });
        }

        [HttpPost("xoa-theo-nam")]
        public async Task<IActionResult> XoaTheoNam(int namHoc)
        {
            var result = await diemCongService.XoaTheoNamAsync(namHoc);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index), new { namHoc });
        }

        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile fileExcel, int namHoc, bool overwriteExisting = false)
        {
            var result = await diemCongService.ImportExcelAsync(fileExcel, namHoc, overwriteExisting);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index), new { namHoc });
        }

        [HttpGet("xuat-excel")]
        public async Task<IActionResult> XuatExcel(int? namHoc)
        {
            var result = await diemCongService.XuatExcelAsync(namHoc);
            if (!result.Success || result.Data == null)
            {
                result.SetNotification(TempData);
                return RedirectToAction(nameof(Index), new { namHoc });
            }

            return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        }

        [HttpGet("download-template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            var result = await diemCongService.TaoFileMauExcelAsync();
            if (!result.Success || result.Data == null)
            {
                result.SetNotification(TempData);
                return RedirectToAction(nameof(Index));
            }

            return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        }
    }
}
