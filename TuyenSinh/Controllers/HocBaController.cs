using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Services;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/hoc-ba")]
    public class HocBaController(
        IHocBaService hocBaService,
        IFileStorageService fileStorageService) : Controller
    {
        [HttpGet("")]
        public IActionResult KiemTraHocBa()
        {
            return View("Index");
        }

        [HttpPost("tai-len")]
        public async Task<IActionResult> TaiLenHocBa(IFormFile file)
        {
            try
            {
                var excelId = await fileStorageService.LuuFileTamThoiAsync(file);
                return Json(new
                {
                    success = true,
                    excelId = excelId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("xem-truoc")]
        public IActionResult XemTruocHocBa(string excelId)
        {
            if (string.IsNullOrEmpty(excelId))
            {
                return RedirectToAction(nameof(KiemTraHocBa));
            }
            ViewBag.ExcelId = excelId;
            return View("Preview");
        }

        [HttpGet("lay-du-lieu-xem-truoc")]
        public async Task<IActionResult> LayDuLieuXemTruoc(string excelId)
        {
            var data = await hocBaService.GetPreviewDataAsync(excelId, null);
            if (data == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu xem trước." });
            }
            return Json(new { success = true, data = data });
        }

        [HttpPost("thuc-hien-kiem-tra")]
        public async Task<IActionResult> ThucHienKiemTraHocBa(string excelId)
        {
            var result = await hocBaService.CheckHocBaAsync(excelId);
            if (result.ThanhCong)
            {
                return Json(new
                {
                    success = true,
                    danhSachThieuNamHoc = result.DanhSachThieuNamHoc,
                    danhSachThieuDiem = result.DanhSachThieuDiem,
                });
            }
            return Json(new { success = false, message = result.ThongBao });
        }

        [HttpGet("xuat-excel-thieu-diem")]
        public async Task<IActionResult> XuatExcelThieuDiemToHop(string excelId)
        {
            var result = await hocBaService.XuatExcelThieuDiemToHopAsync(excelId);
            if (!result.Success || result.Data == null)
            {
                return BadRequest(result.Message);
            }
            return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        }

        [HttpGet("doi-chieu")]
        public IActionResult DoiChieuHocBaNguyenVong()
        {
            return View("DoiChieu");
        }

        [HttpPost("thuc-hien-doi-chieu")]
        public async Task<IActionResult> ThucHienDoiChieu(IFormFile fileHocBa, IFormFile fileNguyenVong)
        {
            if (fileHocBa == null || fileHocBa.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file học bạ.";
                return RedirectToAction(nameof(DoiChieuHocBaNguyenVong));
            }
            if (fileNguyenVong == null || fileNguyenVong.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file nguyện vọng.";
                return RedirectToAction(nameof(DoiChieuHocBaNguyenVong));
            }

            try
            {
                var hocBaFileId = await fileStorageService.LuuFileTamThoiAsync(fileHocBa);
                var nguyenVongFileId = await fileStorageService.LuuFileTamThoiAsync(fileNguyenVong);

                ViewBag.HocBaFileId = hocBaFileId;
                ViewBag.NguyenVongFileId = nguyenVongFileId;

                return View("KetQuaDoiChieu", new KetQuaDoiChieu());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra trong quá trình nạp tệp: " + ex.Message;
                return RedirectToAction(nameof(DoiChieuHocBaNguyenVong));
            }
        }

        [HttpGet("lay-ket-qua-doi-chieu")]
        public async Task<IActionResult> LayKetQuaDoiChieu(string hocBaFileId, string nguyenVongFileId)
        {
            if (string.IsNullOrEmpty(hocBaFileId) || string.IsNullOrEmpty(nguyenVongFileId))
                return Json(new { success = false, message = "Yêu cầu không hợp lệ." });

            var result = await hocBaService.DoiChieuHocBaVaNguyenVongAsync(hocBaFileId, nguyenVongFileId);

            return Json(new
            {
                success = true,
                tongNguyenVong = result.TongNguyenVong,
                tongLoiKhongTimThayNganh = result.TongLoiKhongTimThayNganh,
                danhSachMaNganhKhongTim = result.DanhSachMaNganhKhongTim,
                data = result.DanhSachThieuDiem,
                thongKeTongHop = result.ThongKeTongHop,
                thongKeTheoNganh = result.ThongKeTheoNganh
            });
        }

        [HttpGet("xuat-excel-ket-qua-doi-chieu")]
        public async Task<IActionResult> XuatExcelKetQuaDoiChieu(string hocBaFileId, string nguyenVongFileId)
        {
            var result = await hocBaService.XuatExcelKetQuaDoiChieuAsync(hocBaFileId, nguyenVongFileId);
            if (!result.Success || result.Data == null)
            {
                return BadRequest(result.Message);
            }
            return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        }

        [HttpGet("kiem-tra-diem-san")]
        public async Task<IActionResult> KiemTraDiemSan()
        {
            var dsNganh = await hocBaService.LayDanhSachNganhAsync();
            ViewBag.DanhSachNganh = dsNganh;
            return View("KiemTraDiemSan");
        }

        [HttpPost("thuc-hien-kiem-tra-diem-san")]
        public async Task<IActionResult> ThucHienKiemTraDiemSan(string maNganh, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file kiểm tra điểm sàn.";
                return RedirectToAction(nameof(KiemTraDiemSan));
            }

            try
            {
                var fileId = await fileStorageService.LuuFileTamThoiAsync(file);
                ViewBag.FileId = fileId;
                ViewBag.MaNganh = maNganh ?? "";

                var dsNganh = await hocBaService.LayDanhSachNganhAsync();
                var nganhChon = dsNganh.FirstOrDefault(n => n.MaNganh == maNganh);
                ViewBag.TenNganh = nganhChon != null ? nganhChon.TenNganh : "Tất cả các ngành";

                return View("KetQuaKiemTraDiemSan", new KetQuaKiemTraDiemSan());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi nạp file: " + ex.Message;
                return RedirectToAction(nameof(KiemTraDiemSan));
            }
        }

        [HttpGet("lay-ket-qua-kiem-tra-diem-san")]
        public async Task<IActionResult> LayKetQuaKiemTraDiemSan(string maNganh, string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return Json(new { success = false, message = "Yêu cầu không hợp lệ." });
            }

            var result = await hocBaService.KiemTraDiemSan(maNganh, fileId);

            return Json(new
            {
                success = result.ThanhCong,
                message = result.ThongBao,
                tongSoThiSinh = result.TongSoThiSinh,
                soThiSinhDat = result.SoThiSinhDat,
                soThiSinhKhongDat = result.SoThiSinhKhongDat,
                data = result.DanhSachKiemTraDiemSan
            });
        }

        [HttpGet("xuat-excel-kiem-tra-diem-san")]
        public async Task<IActionResult> XuatExcelKiemTraDiemSan(string maNganh, string fileId)
        {
            var result = await hocBaService.XuatExcelKiemTraDiemSanAsync(maNganh, fileId);
            if (!result.Success || result.Data == null)
            {
                return BadRequest(result.Message);
            }
            return File(result.Data.Content, result.Data.ContentType, result.Data.FileName);
        }
    }
}
