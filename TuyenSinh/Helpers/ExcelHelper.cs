using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Globalization;

namespace TuyenSinh.Helpers
{
    public static class ExcelHelper
    {
        public const string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public static void EnsureLicenseContext()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public static string? ParseString(object? val)
        {
            if (val == null) return null;
            var str = val.ToString()?.Trim();
            return string.IsNullOrEmpty(str) ? null : str;
        }

        public static int? ParseInt(object? val)
        {
            if (val == null) return null;
            var str = val.ToString()?.Trim();
            if (int.TryParse(str, out int res)) return res;
            return null;
        }

        public static decimal? ParseDecimal(object? val)
        {
            if (val == null) return null;
            var str = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(str)) return null;

            str = str.Replace(',', '.');

            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal res))
            {
                return res;
            }

            return null;
        }

        public static decimal ParseDiemBac(object? val)
        {
            if (val == null) return 0m;
            var str = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(str)) return 0m;

            if (str.Contains('/'))
            {
                var parts = str.Split('/');
                str = parts[0].Trim();
            }

            str = str.Replace(',', '.');

            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal res))
            {
                return res;
            }

            return 0m;
        }

        public static DateTime ParseDate(object? val)
        {
            if (val == null) return DateTime.MinValue;

            if (val is DateTime dt) return dt;

            if (val is double dbl)
            {
                try { return DateTime.FromOADate(dbl); } catch { }
            }

            var str = val.ToString()?.Trim();
            if (string.IsNullOrEmpty(str)) return DateTime.MinValue;

            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy", "d/M/yy", "dd/MM/yy" };
            if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }

            if (DateTime.TryParse(str, out var generalDate))
            {
                return generalDate;
            }

            return DateTime.MinValue;
        }

        public static void FormatHeaderRow(ExcelWorksheet sheet, string[] headers, int row = 1)
        {
            EnsureLicenseContext();
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = sheet.Cells[row, col + 1];
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
        }
    }
}
