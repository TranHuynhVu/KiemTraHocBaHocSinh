using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TuyenSinh.Common;
using TuyenSinh.Helpers;
using TuyenSinh.Models;

namespace TuyenSinh.Services.Excel
{
    public sealed class DiemCongExcelService : IDiemCongExcelService
    {
        public (List<DiemCong> Records, int SkippedCount) ReadImportStream(Stream stream, int namHoc)
        {
            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null)
            {
                return (new List<DiemCong>(), 0);
            }

            int startRow = 2; // Dòng 1 là Tiêu đề
            int endRow = sheet.Dimension.End.Row;
            int totalSkipped = 0;
            var newRecords = new List<DiemCong>();

            for (int r = startRow; r <= endRow; r++)
            {
                var ddcn = ExcelHelper.ParseString(sheet.Cells[r, 2].Value);
                var hoTen = ExcelHelper.ParseString(sheet.Cells[r, 3].Value);

                if (string.IsNullOrWhiteSpace(ddcn) || string.IsNullOrWhiteSpace(hoTen))
                {
                    totalSkipped++;
                    continue;
                }

                var dobVal = sheet.Cells[r, 4].Value;
                DateTime dob = ExcelHelper.ParseDate(dobVal);

                var maXetTuyen = ExcelHelper.ParseString(sheet.Cells[r, 5].Value) ?? string.Empty;
                var maPTXT = ExcelHelper.ParseString(sheet.Cells[r, 6].Value) ?? string.Empty;
                var maToHop = ExcelHelper.ParseString(sheet.Cells[r, 7].Value) ?? string.Empty;
                var loaiDiemCong = ExcelHelper.ParseInt(sheet.Cells[r, 8].Value) ?? 0;
                var diem = ExcelHelper.ParseDecimal(sheet.Cells[r, 9].Value) ?? 0m;

                newRecords.Add(new DiemCong
                {
                    DDCN = ddcn.Trim(),
                    HoTen = hoTen.Trim(),
                    DOB = dob,
                    MaXetTuyen = maXetTuyen.Trim(),
                    MaPTXT = maPTXT.Trim(),
                    MaToHop = maToHop.Trim(),
                    LoaiDiemCong = loaiDiemCong,
                    Diem = diem,
                    NamHoc = namHoc
                });
            }

            return (newRecords, totalSkipped);
        }

        public FileDownloadDto Export(IReadOnlyList<DiemCong> data, int? namHoc)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("DiemCong");

            string[] headers = ["STT", "ĐDCN", "Họ Tên", "Ngày sinh", "Mã xét tuyển", "Mã PTXT", "Mã tổ hợp", "Loại điểm cộng", "Điểm Cộng", "Năm Học"];
            ExcelHelper.FormatHeaderRow(sheet, headers);

            int row = 2;
            int stt = 1;
            foreach (var item in data)
            {
                sheet.Cells[row, 1].Value = stt++;
                sheet.Cells[row, 2].Value = item.DDCN;
                sheet.Cells[row, 3].Value = item.HoTen;
                sheet.Cells[row, 4].Value = item.DOB == DateTime.MinValue ? "" : item.DOB.ToString("dd/MM/yyyy");
                sheet.Cells[row, 5].Value = item.MaXetTuyen;
                sheet.Cells[row, 6].Value = item.MaPTXT;
                sheet.Cells[row, 7].Value = item.MaToHop;
                sheet.Cells[row, 8].Value = item.LoaiDiemCong;
                sheet.Cells[row, 9].Value = item.Diem;
                sheet.Cells[row, 10].Value = item.NamHoc;

                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                sheet.Cells[row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row++;
            }

            if (sheet.Dimension != null)
            {
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            }

            string fileName = namHoc.HasValue ? $"DanhSach_DiemCong_{namHoc.Value}.xlsx" : "DanhSach_DiemCong_TatCa.xlsx";
            return new FileDownloadDto(package.GetAsByteArray(), fileName);
        }

        public FileDownloadDto CreateTemplate()
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Mau_DiemCong");

            string[] headers = ["STT", "ĐDCN", "Họ Tên", "Ngày sinh", "Mã xét tuyển", "Mã PTXT", "Mã tổ hợp", "Loại điểm cộng", "Điểm Cộng"];
            ExcelHelper.FormatHeaderRow(sheet, headers);

            // Dữ liệu mẫu
            sheet.Cells[2, 1].Value = 1;
            sheet.Cells[2, 2].Value = "051208004175";
            sheet.Cells[2, 3].Value = "PHẠM THÀNH LONG";
            sheet.Cells[2, 4].Value = "01/12/2008";
            sheet.Cells[2, 5].Value = "7510103";
            sheet.Cells[2, 6].Value = "407";
            sheet.Cells[2, 7].Value = "";
            sheet.Cells[2, 8].Value = 2;
            sheet.Cells[2, 9].Value = 1.25;

            sheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Cells[2, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            if (sheet.Dimension != null)
            {
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            }

            return new FileDownloadDto(package.GetAsByteArray(), "Mau_DiemCong.xlsx");
        }
    }
}
