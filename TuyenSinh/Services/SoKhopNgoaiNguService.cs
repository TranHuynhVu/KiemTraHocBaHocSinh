using Hangfire;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TuyenSinh.Data;
using TuyenSinh.Helpers;
using TuyenSinh.ViewModels;

namespace TuyenSinh.Services
{
    public class SoKhopNgoaiNguService : ISoKhopNgoaiNguService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ApplicationDbContext _context;
        private readonly IQuyDoiNNService _quyDoiNNService;

        public SoKhopNgoaiNguService(IFileStorageService fileStorageService, ApplicationDbContext context, IQuyDoiNNService quyDoiNNService)
        {
            _fileStorageService = fileStorageService;
            _context = context;
            _quyDoiNNService = quyDoiNNService;
        }

        public async Task<SoKhopNgoaiNguThongKeViewModel> Join3ExcelFilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null)
        {
            ExcelHelper.EnsureLicenseContext();

            var pathNV = _fileStorageService.GetUploadPath(nvFileId);
            var pathDSTS = _fileStorageService.GetUploadPath(dstsFileId);
            var pathNN = _fileStorageService.GetUploadPath(nnFileId);

            // Read File 1: Nguyện Vọng
            var (mapNV, totalCountNV) = ReadNguyenVongFile(pathNV);

            // Read File 2: Danh Sách Thí Sinh
            var (mapDSTS, totalCountDSTS) = ReadDstsFile(pathDSTS);

            // Read File 3: Hợp Lệ Ngoại Ngữ
            var (listNN, mapNN, totalCountNN) = ReadHopLeNnFile(pathNN);

            var listLoaiNN = await _quyDoiNNService.DanhSachDiemQuyDoiAsync();

            var resultItems = new List<KetQuaSoKhopNgoaiNguItem>();

            foreach (var nnItem in listNN)
            {
                var ddcn = nnItem.Ddcn;
                if (string.IsNullOrEmpty(ddcn)) continue;

                mapDSTS.TryGetValue(ddcn, out var dstsInfo);
                mapNV.TryGetValue(ddcn, out var mxtList);

                string sbd = dstsInfo.Sbd ?? nnItem.Sbd ?? string.Empty;
                string hoTen = dstsInfo.HoTen ?? string.Empty;
                string ngaySinh = dstsInfo.NgaySinh ?? string.Empty;
                string chungChi = nnItem.ChungChi ?? string.Empty;
                decimal diemBac = nnItem.DiemBac;
                string maXetTuyen = mxtList != null && mxtList.Any()
                    ? string.Join("; ", mxtList.Distinct())
                    : string.Empty;
                decimal? diemQuyDoi = _quyDoiNNService.LayDiemQuyDoiNNTheoTenLoai(listLoaiNN, chungChi, diemBac);

                resultItems.Add(new KetQuaSoKhopNgoaiNguItem
                {
                    SoBaoDanh = sbd,
                    HoTen = hoTen,
                    NgaySinh = ngaySinh,
                    Ddcn = ddcn,
                    ChungChiNgoaiNgu = chungChi,
                    DiemNN = diemBac,
                    MaXetTuyen = maXetTuyen,
                    DiemQuyDoi = diemQuyDoi
                });
            }

            // Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                resultItems = resultItems.Where(x =>
                    x.Ddcn.ToLower().Contains(s) ||
                    x.HoTen.ToLower().Contains(s) ||
                    x.SoBaoDanh.ToLower().Contains(s) ||
                    x.MaXetTuyen.ToLower().Contains(s) ||
                    x.ChungChiNgoaiNgu.ToLower().Contains(s)
                ).ToList();
            }

            // Assign STT
            for (int i = 0; i < resultItems.Count; i++)
            {
                resultItems[i].Stt = i + 1;
            }

            return new SoKhopNgoaiNguThongKeViewModel
            {
                TongHopLeNN = totalCountNN,
                TongDanhSachThiSinh = totalCountDSTS,
                TongNguyenVong = totalCountNV,
                TongSoKhop = resultItems.Count,
                DanhSachKetQua = resultItems
            };
        }

        public async Task<(bool Success, string Message, byte[]? FileContents)> XuatExcel3FilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null)
        {
            var data = await Join3ExcelFilesAsync(nvFileId, dstsFileId, nnFileId, search);
            if (data.DanhSachKetQua == null || !data.DanhSachKetQua.Any())
                return (false, "Không có dữ liệu so khớp để xuất Excel.", null);

            ExcelHelper.EnsureLicenseContext();
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("KetQua_SoKhop_3File");

            // Header styling
            string[] headers = new[] { "STT", "Số báo danh", "Họ Tên", "Ngày sinh", "ĐDCN", "Chứng chỉ ngoại ngữ", "Điểm / Bậc chứng chỉ", "Điểm quy đổi môn TA", "Mã xét tuyển" };
            ExcelHelper.FormatHeaderRow(sheet, headers);

            int row = 2;
            foreach (var item in data.DanhSachKetQua)
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

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            return (true, "Thành công", package.GetAsByteArray());
        }

        private (Dictionary<string, List<string>> MapNV, int TotalCount) ReadNguyenVongFile(string filePath)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int totalRows = 0;

            if (!File.Exists(filePath)) return (result, 0);

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null || sheet.Dimension == null) return (result, 0);

            int endRow = sheet.Dimension.End.Row;

            // Fixed column positions: Header at Row 5, Data starts at Row 6
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

        private (Dictionary<string, (string? Sbd, string? HoTen, string? NgaySinh)> MapDSTS, int TotalCount) ReadDstsFile(string filePath)
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

        private (List<(string Ddcn, string Sbd, string ChungChi, decimal DiemBac)> ListNN, Dictionary<string, (string Sbd, string ChungChi, decimal DiemBac)> MapNN, int TotalCount) ReadHopLeNnFile(string filePath)
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
    }
}