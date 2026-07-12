using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using TuyenSinh.Services;
using OfficeOpenXml;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HocBaController : Controller
    {
        private readonly IHocBaService _hocBaService;

        public HocBaController(IHocBaService hocBaService)
        {
            _hocBaService = hocBaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
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

        [HttpGet]
        public IActionResult Preview(string excelId)
        {
            if (string.IsNullOrEmpty(excelId))
            {
                return RedirectToAction("Index");
            }
            ViewBag.ExcelId = excelId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPreviewData(string excelId)
        {
            var data = await _hocBaService.GetPreviewDataAsync(excelId, null);
            if (data == null)
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu xem trước." });
            }
            return Json(new { success = true, data = data });
        }

        [HttpPost]
        public async Task<IActionResult> CheckHocBa(string excelId)
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

        [HttpGet]
        public async Task<IActionResult> ExportMissingScores(string excelId)
        {
            if (string.IsNullOrEmpty(excelId))
            {
                return BadRequest("Mã tệp Excel không hợp lệ.");
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
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(229, 241, 255)); // Light blue matching --primary-light
                    range.Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(0, 122, 255)); // Primary color
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
    }
}
