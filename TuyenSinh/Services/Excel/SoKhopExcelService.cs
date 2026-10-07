using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TuyenSinh.Common;
using TuyenSinh.Helpers;
using TuyenSinh.ViewModels.SoKhopNgoaiNgu;

namespace TuyenSinh.Services.Excel
{
    public sealed class SoKhopExcelService : ISoKhopExcelService
    {
        public (Dictionary<string, List<string>> MapNV, int TotalCount) ReadNguyenVongFile(string filePath)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int totalRows = 0;

            if (!File.Exists(filePath)) return (result, 0);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null) return (result, 0);

            int endRow = sheet.Dimension.End.Row;

            // Vị trí cột: Tiêu đề ở dòng 5, Dữ liệu từ dòng 6
            int headerRow = 5;
            int colDdcn = 2;
            int colMaXetTuyen = 6;

            for (int r = headerRow + 1; r <= endRow; r++)
            {
                var ddcn = ExcelHelper.ParseString(sheet.Cells[r, colDdcn].Value);
                var mxt = ExcelHelper.ParseString(sheet.Cells[r, colMaXetTuyen].Value);

                if (string.IsNullOrWhiteSpace(ddcn)) continue;
                totalRows++;

                if (!string.IsNullOrWhiteSpace(mxt))
                {
                    if (!result.TryGetValue(ddcn, out var list))
                    {
                        list = new List<string>();
                        result[ddcn] = list;
                    }
                    list.Add(mxt);
                }
            }

            return (result, totalRows);
        }

        public (Dictionary<string, (string? Sbd, string? HoTen, string? NgaySinh)> MapDSTS, int TotalCount) ReadDstsFile(string filePath)
        {
            var result = new Dictionary<string, (string? Sbd, string? HoTen, string? NgaySinh)>(StringComparer.OrdinalIgnoreCase);
            int totalRows = 0;

            if (!File.Exists(filePath)) return (result, 0);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null) return (result, 0);

            int endRow = sheet.Dimension.End.Row;
            int colSbd = 2, colHoTen = 3, colDdcn = 4, colNgaySinh = 5;

            for (int r = 2; r <= endRow; r++)
            {
                var ddcn = ExcelHelper.ParseString(sheet.Cells[r, colDdcn].Value);
                var sbd = ExcelHelper.ParseString(sheet.Cells[r, colSbd].Value);
                var hoTen = ExcelHelper.ParseString(sheet.Cells[r, colHoTen].Value);
                var ngaySinh = ExcelHelper.ParseString(sheet.Cells[r, colNgaySinh].Value);

                if (string.IsNullOrWhiteSpace(ddcn) && string.IsNullOrWhiteSpace(sbd) && string.IsNullOrWhiteSpace(hoTen)) continue;
                totalRows++;

                if (!string.IsNullOrWhiteSpace(ddcn))
                {
                    result[ddcn] = (sbd, hoTen, ngaySinh);
                }
            }

            return (result, totalRows);
        }

        public (List<(string Ddcn, string Sbd, string ChungChi, decimal DiemBac)> ListNN, Dictionary<string, (string Sbd, string ChungChi, decimal DiemBac)> MapNN, int TotalCount) ReadHopLeNnFile(string filePath)
        {
            var list = new List<(string Ddcn, string Sbd, string ChungChi, decimal DiemBac)>();
            var map = new Dictionary<string, (string Sbd, string ChungChi, decimal DiemBac)>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(filePath)) return (list, map, 0);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null) return (list, map, 0);

            int endRow = sheet.Dimension.End.Row;
            int colSbd = 2, colDdcn = 3, colCc = 4, colDiem = 5;

            for (int r = 2; r <= endRow; r++)
            {
                var ddcn = ExcelHelper.ParseString(sheet.Cells[r, colDdcn].Value);
                var sbd = ExcelHelper.ParseString(sheet.Cells[r, colSbd].Value);
                var cc = ExcelHelper.ParseString(sheet.Cells[r, colCc].Value);
                var diemVal = sheet.Cells[r, colDiem].Value;
                var diem = ExcelHelper.ParseDiemBac(diemVal);

                if (string.IsNullOrWhiteSpace(ddcn) && string.IsNullOrWhiteSpace(sbd)) continue;

                if (!string.IsNullOrWhiteSpace(ddcn))
                {
                    var ccStr = cc ?? string.Empty;
                    var sbdStr = sbd ?? string.Empty;
                    list.Add((ddcn, sbdStr, ccStr, diem));
                    map[ddcn] = (sbdStr, ccStr, diem);
                }
            }

            return (list, map, list.Count);
        }

        public FileDownloadDto Export(IReadOnlyList<KetQuaSoKhopNgoaiNguItem> items)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("KetQua_SoKhop_3File");

            string[] headers = ["STT", "Số báo danh", "Họ Tên", "Ngày sinh", "ĐDCN", "Chứng chỉ ngoại ngữ", "Điểm / Bậc chứng chỉ", "Điểm quy đổi môn TA", "Mã xét tuyển"];
            ExcelHelper.FormatHeaderRow(sheet, headers);

            int row = 2;
            foreach (var item in items)
            {
                sheet.Cells[row, 1].Value = item.Stt;
                sheet.Cells[row, 2].Value = item.SoBaoDanh;
                sheet.Cells[row, 3].Value = item.HoTen;
                sheet.Cells[row, 4].Value = item.NgaySinh;
                sheet.Cells[row, 5].Value = item.Ddcn;
                sheet.Cells[row, 6].Value = item.ChungChiNgoaiNgu;
                sheet.Cells[row, 7].Value = item.DiemNN;
                sheet.Cells[row, 8].Value = item.DiemQuyDoi.HasValue ? item.DiemQuyDoi.Value : "-";
                sheet.Cells[row, 9].Value = item.MaXetTuyen;

                sheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row++;
            }

            if (sheet.Dimension != null)
            {
                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            }

            return new FileDownloadDto(
                package.GetAsByteArray(),
                "KetQua_SoKhop_3File_DSHopLeNN_vs_DSTS_vs_NV.xlsx");
        }
    }
}
