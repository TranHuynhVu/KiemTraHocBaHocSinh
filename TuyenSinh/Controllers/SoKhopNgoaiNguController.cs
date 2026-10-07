using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Services;
using TuyenSinh.ViewModels.SoKhopNgoaiNgu;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/so-khop-ngoai-ngu")]
    public class SoKhopNgoaiNguController(
        ISoKhopNgoaiNguService soKhopNgoaiNguService,
        IFileStorageService fileStorageService) : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost("thuc-hien-so-khop")]
        public async Task<IActionResult> ThucHienSoKhop(IFormFile fileNV, IFormFile fileDSTS, IFormFile fileNN)
        {
            if (fileNV == null || fileNV.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn File Nguyện vọng thí sinh.";
                return RedirectToAction("Index");
            }
            if (fileDSTS == null || fileDSTS.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn File Danh sách thí sinh.";
                return RedirectToAction("Index");
            }
            if (fileNN == null || fileNN.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn File Hợp lệ Ngoại ngữ.";
                return RedirectToAction("Index");
            }

            try
            {
                var nvFileId = await fileStorageService.LuuFileTamThoiAsync(fileNV);
                var dstsFileId = await fileStorageService.LuuFileTamThoiAsync(fileDSTS);
                var nnFileId = await fileStorageService.LuuFileTamThoiAsync(fileNN);

                ViewBag.NvFileId = nvFileId;
                ViewBag.DstsFileId = dstsFileId;
                ViewBag.NnFileId = nnFileId;

                return View("KetQuaDoiChieu", new SoKhopNgoaiNguThongKeViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi nạp các tệp Excel: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet("lay-ket-qua-so-khop")]
        public async Task<IActionResult> LayKetQuaSoKhop(string nvFileId, string dstsFileId, string nnFileId, string? search)
        {
            if (string.IsNullOrEmpty(nvFileId) || string.IsNullOrEmpty(dstsFileId) || string.IsNullOrEmpty(nnFileId))
            {
                return Json(new { success = false, message = "Các tệp đối chiếu không hợp lệ hoặc đã hết hạn." });
            }

            try
            {
                var data = await soKhopNgoaiNguService.Join3ExcelFilesAsync(nvFileId, dstsFileId, nnFileId, search);
                return Json(new
                {
                    success = true,
                    tongHopLeNN = data.TongHopLeNN,
                    tongDanhSachThiSinh = data.TongDanhSachThiSinh,
                    tongNguyenVong = data.TongNguyenVong,
                    tongSoKhop = data.TongSoKhop,
                    data = data.DanhSachKetQua
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xử lý dữ liệu: " + ex.Message });
            }
        }

        [HttpGet("xuat-excel")]
        public async Task<IActionResult> XuatExcel(string nvFileId, string dstsFileId, string nnFileId, string? search)
        {
            var result = await soKhopNgoaiNguService.XuatExcel3FilesAsync(nvFileId, dstsFileId, nnFileId, search);
            if (!result.Success || result.Data == null)
            {
                return BadRequest(result.Message);
            }
            return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        }
    }
}
