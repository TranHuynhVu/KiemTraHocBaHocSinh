using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TuyenSinh.Common;
using TuyenSinh.Helpers;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.Excel
{
    public sealed class HocBaExcelService : IHocBaExcelService
    {
        public List<HocBaTHPTImport> ReadHocBaFile(string filePath, int? limit = null)
        {
            var list = new List<HocBaTHPTImport>();
            if (!File.Exists(filePath)) return list;

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[0];
            if (sheet?.Dimension == null) return list;

            int totalRows = sheet.Dimension.End.Row;
            int startRow = 4; // Dữ liệu bắt đầu từ dòng 4
            int endRow = limit.HasValue ? Math.Min(startRow + limit.Value - 1, totalRows) : totalRows;

            for (int r = startRow; r <= endRow; r++)
            {
                var item = new HocBaTHPTImport
                {
                    STT = ExcelHelper.ParseInt(sheet.Cells[r, 1].Value),
                    SoDDCN = ExcelHelper.ParseString(sheet.Cells[r, 2].Value),
                    HoVaTen = ExcelHelper.ParseString(sheet.Cells[r, 3].Value),
                    NgaySinh = ExcelHelper.ParseDateTime(sheet.Cells[r, 4].Value),
                    GioiTinh = ExcelHelper.ParseString(sheet.Cells[r, 5].Value),
                    Lop = ExcelHelper.ParseInt(sheet.Cells[r, 6].Value),
                    ChuongTrinhHoc = ExcelHelper.ParseInt(sheet.Cells[r, 7].Value),

                    DiemTrungBinhNam = ExcelHelper.ParseDecimal(sheet.Cells[r, 8].Value),
                    DiemTongKetHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 9].Value),
                    DiemTongKetHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 10].Value),
                    DiemTongKetCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 11].Value),

                    HocLucHKI = ExcelHelper.ParseString(sheet.Cells[r, 12].Value),
                    HocLucHKII = ExcelHelper.ParseString(sheet.Cells[r, 13].Value),
                    HocLucCN = ExcelHelper.ParseString(sheet.Cells[r, 14].Value),

                    HanhKiemHKI = ExcelHelper.ParseString(sheet.Cells[r, 15].Value),
                    HanhKiemHKII = ExcelHelper.ParseString(sheet.Cells[r, 16].Value),
                    HanhKiemCN = ExcelHelper.ParseString(sheet.Cells[r, 17].Value),

                    KetQuaHocTapHKI = ExcelHelper.ParseString(sheet.Cells[r, 18].Value),
                    KetQuaHocTapHKII = ExcelHelper.ParseString(sheet.Cells[r, 19].Value),
                    KetQuaHocTapCN = ExcelHelper.ParseString(sheet.Cells[r, 20].Value),

                    KetQuaRenLuyenHKI = ExcelHelper.ParseString(sheet.Cells[r, 21].Value),
                    KetQuaRenLuyenHKII = ExcelHelper.ParseString(sheet.Cells[r, 22].Value),
                    KetQuaRenLuyenCN = ExcelHelper.ParseString(sheet.Cells[r, 23].Value),

                    ToanHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 24].Value),
                    ToanHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 25].Value),
                    ToanCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 26].Value),

                    VanHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 27].Value),
                    VanHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 28].Value),
                    VanCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 29].Value),

                    VatLyHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 30].Value),
                    VatLyHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 31].Value),
                    VatLyCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 32].Value),

                    HoaHocHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 33].Value),
                    HoaHocHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 34].Value),
                    HoaHocCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 35].Value),

                    SinhHocHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 36].Value),
                    SinhHocHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 37].Value),
                    SinhHocCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 38].Value),

                    LichSuHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 39].Value),
                    LichSuHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 40].Value),
                    LichSuCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 41].Value),

                    DiaLyHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 42].Value),
                    DiaLyHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 43].Value),
                    DiaLyCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 44].Value),

                    GDCDHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 45].Value),
                    GDCDHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 46].Value),
                    GDCDCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 47].Value),

                    KTPLHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 48].Value),
                    KTPLHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 49].Value),
                    KTPLCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 50].Value),

                    TinHocHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 51].Value),
                    TinHocHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 52].Value),
                    TinHocCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 53].Value),

                    CNCNHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 54].Value),
                    CNCNHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 55].Value),
                    CNCNCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 56].Value),

                    CNNNHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 57].Value),
                    CNNNHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 58].Value),
                    CNNNCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 59].Value),

                    NgoaiNguHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 60].Value),
                    NgoaiNguHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 61].Value),
                    NgoaiNguCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 62].Value),
                    MonNgoaiNgu = ExcelHelper.ParseString(sheet.Cells[r, 63].Value),

                    TuChonSongNguHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 64].Value),
                    TuChonSongNguHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 65].Value),
                    TuChonSongNguCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 66].Value),

                    QPANHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 67].Value),
                    QPANHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 68].Value),
                    QPANCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 69].Value),

                    TiengDanTocHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 70].Value),
                    TiengDanTocHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 71].Value),
                    TiengDanTocCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 72].Value),

                    NgoaiNgu2HKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 73].Value),
                    NgoaiNgu2HKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 74].Value),
                    NgoaiNgu2CN = ExcelHelper.ParseDecimal(sheet.Cells[r, 75].Value),
                    MonNgoaiNgu2 = ExcelHelper.ParseString(sheet.Cells[r, 76].Value),

                    ToanPhapHKI = ExcelHelper.ParseDecimal(sheet.Cells[r, 77].Value),
                    ToanPhapHKII = ExcelHelper.ParseDecimal(sheet.Cells[r, 78].Value),
                    ToanPhapCN = ExcelHelper.ParseDecimal(sheet.Cells[r, 79].Value),
                };

                if (!string.IsNullOrWhiteSpace(item.SoDDCN) && !string.IsNullOrWhiteSpace(item.HoVaTen))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public List<KetQuaNguyenVongImport> ReadKetQuaNguyenVongExcel(string filePath)
        {
            var list = new List<KetQuaNguyenVongImport>();
            if (!File.Exists(filePath)) return list;

            using var package = new ExcelPackage(new FileInfo(filePath));
            var sheet = package.Workbook.Worksheets[0];
            if (sheet?.Dimension == null) return list;

            int totalRows = sheet.Dimension.End.Row;
            int startRow = 2; // Dòng 1 là tiêu đề

            for (int r = startRow; r <= totalRows; r++)
            {
                var item = new KetQuaNguyenVongImport
                {
                    HoTen = ExcelHelper.ParseString(sheet.Cells[r, 1].Value) ?? string.Empty,
                    CCCD = ExcelHelper.ParseString(sheet.Cells[r, 2].Value) ?? string.Empty,
                    NgaySinh = ExcelHelper.ParseDateTime(sheet.Cells[r, 3].Value) ?? default,
                    NamTN = ExcelHelper.ParseInt(sheet.Cells[r, 4].Value) ?? 0,
                    DTUT = ExcelHelper.ParseString(sheet.Cells[r, 5].Value),
                    KVUT = ExcelHelper.ParseString(sheet.Cells[r, 6].Value),
                    HocLuc = ExcelHelper.ParseString(sheet.Cells[r, 7].Value),
                    DiemXetTN = ExcelHelper.ParseString(sheet.Cells[r, 8].Value),
                    ThuTuNV = ExcelHelper.ParseInt(sheet.Cells[r, 9].Value),
                    MaTruong = ExcelHelper.ParseString(sheet.Cells[r, 10].Value),
                    MaNganh = ExcelHelper.ParseString(sheet.Cells[r, 11].Value),
                    PTXT = ExcelHelper.ParseString(sheet.Cells[r, 12].Value),
                    ToHop = ExcelHelper.ParseString(sheet.Cells[r, 13].Value),

                    Mon1 = ExcelHelper.ParseString(sheet.Cells[r, 14].Value),
                    TrongSoMon1 = ExcelHelper.ParseDecimal(sheet.Cells[r, 15].Value),
                    DiemMon1HB = ExcelHelper.ParseDecimal(sheet.Cells[r, 16].Value),
                    DiemMon1THPT = ExcelHelper.ParseDecimal(sheet.Cells[r, 17].Value),

                    Mon2 = ExcelHelper.ParseString(sheet.Cells[r, 18].Value),
                    TrongSoMon2 = ExcelHelper.ParseDecimal(sheet.Cells[r, 19].Value),
                    DiemMon2HB = ExcelHelper.ParseDecimal(sheet.Cells[r, 20].Value),
                    DiemMon2THPT = ExcelHelper.ParseDecimal(sheet.Cells[r, 21].Value),

                    Mon3 = ExcelHelper.ParseString(sheet.Cells[r, 22].Value),
                    TrongSoMon3 = ExcelHelper.ParseDecimal(sheet.Cells[r, 23].Value),
                    DiemMon3HB = ExcelHelper.ParseDecimal(sheet.Cells[r, 24].Value),
                    DiemMon3THPT = ExcelHelper.ParseDecimal(sheet.Cells[r, 25].Value),

                    TrongSoHB = ExcelHelper.ParseDecimal(sheet.Cells[r, 26].Value),
                    TrongSoTHPT = ExcelHelper.ParseDecimal(sheet.Cells[r, 27].Value),
                    DiemCong = ExcelHelper.ParseDecimal(sheet.Cells[r, 28].Value),
                    DiemUuTien = ExcelHelper.ParseDecimal(sheet.Cells[r, 29].Value),
                    DS = ExcelHelper.ParseDecimal(sheet.Cells[r, 30].Value),
                    TDHB = ExcelHelper.ParseDecimal(sheet.Cells[r, 31].Value),
                    TDTHPT = ExcelHelper.ParseDecimal(sheet.Cells[r, 32].Value),
                    TD = ExcelHelper.ParseDecimal(sheet.Cells[r, 33].Value),
                    DiemXetTuyen = ExcelHelper.ParseDecimal(sheet.Cells[r, 34].Value),
                    KQKiemTraNguong = ExcelHelper.ParseString(sheet.Cells[r, 35].Value)?.Trim().Equals("Đạt", StringComparison.OrdinalIgnoreCase) ?? false,
                    GhiChu = ExcelHelper.ParseString(sheet.Cells[r, 36].Value)
                };

                if (!string.IsNullOrWhiteSpace(item.CCCD) && !string.IsNullOrWhiteSpace(item.HoTen))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public FileDownloadDto ExportThieuDiemToHop(KetQuaKiemTraHocBa ketQua)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Thí sinh thiếu điểm");

            string[] headers1 = ["STT", "Số ĐDCN (CCCD)", "Họ và tên", "Năm lỗi", "Tổ hợp", "Môn bị thiếu điểm"];
            ExcelHelper.FormatHeaderRow(worksheet, headers1);

            int row = 2;
            foreach (var item in ketQua.DanhSachThieuDiem)
            {
                worksheet.Cells[row, 1].Value = item.Stt;
                worksheet.Cells[row, 2].Value = item.Cccd;
                worksheet.Cells[row, 3].Value = item.HoVaTen;
                worksheet.Cells[row, 4].Value = item.NamLoi;
                worksheet.Cells[row, 5].Value = item.ToHop;
                worksheet.Cells[row, 6].Value = item.MonThieu;
                row++;
            }

            if (ketQua.DanhSachThieuDiem.Count > 0 && worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            // Sheet 2: Danh sách sinh viên thiếu điểm (Không trùng)
            var ws2 = package.Workbook.Worksheets.Add("Danh sách sinh viên thiếu điểm");
            string[] headers2 = ["STT", "Số ĐDCN (CCCD)", "Họ và Tên", "Tổ hợp thiếu điểm"];
            ExcelHelper.FormatHeaderRow(ws2, headers2);

            var dsThiSinhUnique = ketQua.DanhSachThieuDiem
                .GroupBy(x => x.Cccd)
                .Select((g, idx) => new
                {
                    Stt = idx + 1,
                    Cccd = g.Key,
                    HoVaTen = g.First().HoVaTen,
                    ToHopThieu = string.Join("; ", g.Select(x => x.ToHop).Distinct())
                })
                .ToList();

            int row2 = 2;
            foreach (var item in dsThiSinhUnique)
            {
                ws2.Cells[row2, 1].Value = item.Stt;
                ws2.Cells[row2, 2].Value = item.Cccd;
                ws2.Cells[row2, 3].Value = item.HoVaTen;
                ws2.Cells[row2, 4].Value = item.ToHopThieu;
                row2++;
            }

            if (dsThiSinhUnique.Count > 0 && ws2.Dimension != null)
            {
                ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
            }

            return new FileDownloadDto(package.GetAsByteArray(), "ThiSinh_ThieuDiem_ToHop.xlsx");
        }

        public FileDownloadDto ExportKetQuaDoiChieu(KetQuaDoiChieu ketQua)
        {
            using var package = new ExcelPackage();

            // Sheet 1: Đối chiếu HB - NV
            var ws = package.Workbook.Worksheets.Add("Đối chiếu HB - NV");
            string[] headers = ["STT", "Số ĐDCN (CCCD)", "Họ và Tên", "TT Nguyện Vọng", "Mã Ngành", "Tên Ngành", "Mã Tổ Hợp", "Năm Học", "Môn Thiếu"];
            ExcelHelper.FormatHeaderRow(ws, headers);

            int row = 2;
            foreach (var item in ketQua.DanhSachThieuDiem)
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

            if (ketQua.DanhSachThieuDiem.Count > 0 && ws.Dimension != null)
            {
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }

            // Sheet 2: Danh sách sinh viên thiếu điểm (Không trùng)
            var ws2 = package.Workbook.Worksheets.Add("Danh sách sinh viên thiếu điểm");
            string[] headers2 = ["STT", "Số ĐDCN (CCCD)", "Họ và Tên", "Số NV thiếu điểm", "Chi tiết NV & Tổ hợp thiếu"];
            ExcelHelper.FormatHeaderRow(ws2, headers2);

            var dsThiSinhUnique = ketQua.DanhSachThieuDiem
                .GroupBy(x => x.SoDDCN)
                .Select((g, idx) => new
                {
                    Stt = idx + 1,
                    SoDDCN = g.Key,
                    HoVaTen = g.First().HoVaTen,
                    SoNVThieu = g.Select(x => x.ThuTuNV).Distinct().Count(),
                    ChiTietNV = string.Join("; ", g.Select(x => $"NV{x.ThuTuNV}: {x.MaNganh} ({x.MaToHop})").Distinct())
                })
                .ToList();

            int row2 = 2;
            foreach (var item in dsThiSinhUnique)
            {
                ws2.Cells[row2, 1].Value = item.Stt;
                ws2.Cells[row2, 2].Value = item.SoDDCN;
                ws2.Cells[row2, 3].Value = item.HoVaTen;
                ws2.Cells[row2, 4].Value = item.SoNVThieu;
                ws2.Cells[row2, 5].Value = item.ChiTietNV;
                row2++;
            }

            if (dsThiSinhUnique.Count > 0 && ws2.Dimension != null)
            {
                ws2.Cells[ws2.Dimension.Address].AutoFitColumns();
            }

            return new FileDownloadDto(package.GetAsByteArray(), "DoiChieu_HocBa_NguyenVong.xlsx");
        }

        public FileDownloadDto ExportKiemTraDiemSan(KetQuaKiemTraDiemSan ketQua)
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Kiểm tra điểm sàn");

            string[] headers = ["STT", "Họ và Tên", "Số ĐDCN (CCCD)", "Mã Ngành", "Tổ Hợp", "Điểm Xét Tuyển", "Điểm Sàn Ngành", "Điểm Sàn Toán", "Ghi Chú Lỗi"];
            ExcelHelper.FormatHeaderRow(ws, headers);

            int row = 2;
            int stt = 1;
            foreach (var item in ketQua.DanhSachKiemTraDiemSan)
            {
                ws.Cells[row, 1].Value = stt++;
                ws.Cells[row, 2].Value = item.HoTen;
                ws.Cells[row, 3].Value = item.CCCD;
                ws.Cells[row, 4].Value = item.MaNganh;
                ws.Cells[row, 5].Value = item.ToHop;
                ws.Cells[row, 6].Value = item.DiemXetTuyen;
                ws.Cells[row, 7].Value = item.DiemSan;
                ws.Cells[row, 8].Value = item.DiemSanToan;
                ws.Cells[row, 9].Value = item.GhiChu;
                row++;
            }

            if (ws.Dimension != null)
            {
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
            }

            return new FileDownloadDto(package.GetAsByteArray(), "KetQua_KiemTra_DiemSan.xlsx");
        }
    }
}
