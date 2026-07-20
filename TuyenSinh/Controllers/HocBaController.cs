using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using TuyenSinh.Services;
using OfficeOpenXml;
using TuyenSinh.ViewModels;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/hoc-ba")]
    public class HocBaController : Controller
    {
        private readonly IHocBaService _hocBaService;

        public HocBaController(IHocBaService hocBaService)
        {
            _hocBaService = hocBaService;
        }

        [HttpGet("")]
        public IActionResult KiemTraHocBa()
        {
            return View("Index");
        }

        [HttpPost("tai-len")]
        public async Task<IActionResult> TaiLenHocBa(IFormFile file)
        {
            var result = await _hocBaService.UploadAndPreviewAsync(file);
            if (result.Success)
            {
                return Json(new
                {
                    success = true,
                    excelId = result.ExcelId
                });
            }
            return Json(new { success = false, message = result.Message });
        }

        [HttpGet("xem-truoc")]
        public IActionResult XemTruocHocBa(string excelId)
        {
            if (string.IsNullOrEmpty(excelId))
            {
                return RedirectToAction("KiemTraHocBa");
            }
            ViewBag.ExcelId = excelId;
            return View("Preview");
        }

        [HttpGet("lay-du-lieu-xem-truoc")]
        public async Task<IActionResult> LayDuLieuXemTruoc(string excelId)
        {
            var data = await _hocBaService.GetPreviewDataAsync(excelId, null);
            if (data == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu xem trước." });
            }
            return Json(new { success = true, data = data });
        }

        [HttpPost("thuc-hien-kiem-tra")]
        public async Task<IActionResult> ThucHienKiemTraHocBa(string excelId)
        {
            var result = await _hocBaService.CheckHocBaAsync(excelId);
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
            if (string.IsNullOrEmpty(excelId))
            {
                return BadRequest("Excel không hợp lệ.");
            }

            var result = await _hocBaService.CheckHocBaAsync(excelId);
            if (!result.ThanhCong)
            {
                return BadRequest(result.ThongBao);
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Thí sinh thiếu điểm");

                // Headers
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Số ĐDCN (CCCD)";
                worksheet.Cells[1, 3].Value = "Họ và tên";
                worksheet.Cells[1, 4].Value = "Năm lỗi";
                worksheet.Cells[1, 5].Value = "Tổ hợp";
                worksheet.Cells[1, 6].Value = "Môn bị thiếu điểm";

                // Styling headers
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(229, 241, 255));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 122, 255));
                }

                // Data
                int row = 2;
                foreach (var item in result.DanhSachThieuDiem)
                {
                    worksheet.Cells[row, 1].Value = item.Stt;
                    worksheet.Cells[row, 2].Value = item.Cccd;
                    worksheet.Cells[row, 3].Value = item.HoVaTen;
                    worksheet.Cells[row, 4].Value = item.NamLoi;
                    worksheet.Cells[row, 5].Value = item.ToHop;
                    worksheet.Cells[row, 6].Value = item.MonThieu;
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ThiSinh_ThieuDiem_ToHop.xlsx");
            }
        }

        [HttpGet("doi-chieu")]
        public IActionResult DoiChieuHocBaNguyenVong()
        {
            return View("DoiChieu");
        }

        [HttpPost("ket-qua-doi-chieu")]
        public async Task<IActionResult> NopFileDoiChieu(IFormFile fileHocBa, IFormFile fileNguyenVong)
        {
            if (fileHocBa == null || fileHocBa.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file học bạ.";
                return RedirectToAction("DoiChieuHocBaNguyenVong");
            }
            if (fileNguyenVong == null || fileNguyenVong.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file nguyện vọng.";
                return RedirectToAction("DoiChieuHocBaNguyenVong");
            }

            try
            {
                var hocBaFileId = await _hocBaService.LuuFileTamThoiAsync(fileHocBa);
                var nguyenVongFileId = await _hocBaService.LuuFileTamThoiAsync(fileNguyenVong);

                ViewBag.HocBaFileId = hocBaFileId;
                ViewBag.NguyenVongFileId = nguyenVongFileId;

                return View("KetQuaDoiChieu", new KetQuaDoiChieu());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình nạp tệp: " + ex.Message;
                return RedirectToAction("DoiChieuHocBaNguyenVong");
            }
        }

        [HttpGet("lay-ket-qua-doi-chieu")]
        public async Task<IActionResult> LayKetQuaDoiChieu(string hocBaFileId, string nguyenVongFileId)
        {
            if (string.IsNullOrEmpty(hocBaFileId) || string.IsNullOrEmpty(nguyenVongFileId))
                return Json(new { success = false, message = "Yêu cầu không hợp lệ." });

            var result = await _hocBaService.DoiChieuHocBaVaNguyenVongAsync(hocBaFileId, nguyenVongFileId);

            return Json(new
            {
                success = true,
                tongNguyenVong = result.TongNguyenVong,
                tongLoiKhongTimThayNganh = result.TongLoiKhongTimThayNganh,
                danhSachMaNganhKhongTim = result.DanhSachMaNganhKhongTim,
                data = result.DanhSachThieuDiem
            });
        }

        [HttpGet("xuat-excel-ket-qua-doi-chieu")]
        public async Task<IActionResult> XuatExcelKetQuaDoiChieu(string hocBaFileId, string nguyenVongFileId)
        {
            if (string.IsNullOrEmpty(hocBaFileId) || string.IsNullOrEmpty(nguyenVongFileId))
                return BadRequest("Yêu cầu không hợp lệ.");

            var result = await _hocBaService.DoiChieuHocBaVaNguyenVongAsync(hocBaFileId, nguyenVongFileId);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Đối chiếu HB - NV");

            // Headers
            string[] headers = { "STT", "Số ĐDCN (CCCD)", "Họ và Tên", "TT Nguyện Vọng", "Mã Ngành", "Tên Ngành", "Mã Tổ Hợp", "Năm Học", "Môn Thiếu" };
            for (int c = 0; c < headers.Length; c++)
                ws.Cells[1, c + 1].Value = headers[c];

            using (var range = ws.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(229, 241, 255));
                range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 122, 255));
            }

            int row = 2;
            foreach (var item in result.DanhSachThieuDiem)
            {
                ws.Cells[row, 1].Value = item.Stt;
                ws.Cells[row, 2].Value = item.SoDDCN;
                ws.Cells[row, 3].Value = item.HoVaTen;
                ws.Cells[row, 4].Value = item.ThuTuNV;
                ws.Cells[row, 5].Value = item.MaNganh;
                ws.Cells[row, 6].Value = item.TenNganh;
                ws.Cells[row, 7].Value = item.MaToHop;
                ws.Cells[row, 8].Value = item.NamHoc;
                ws.Cells[row, 9].Value = item.MonThieu;
                row++;
            }
            ws.Cells[ws.Dimension.Address].AutoFitColumns();

            var bytes = package.GetAsByteArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DoiChieu_HocBa_NguyenVong.xlsx");
        }
    }
}
