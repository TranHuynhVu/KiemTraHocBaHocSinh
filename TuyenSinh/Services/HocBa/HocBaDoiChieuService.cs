using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Helpers;
using TuyenSinh.Models;
using TuyenSinh.Services.Excel;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public sealed class HocBaDoiChieuService(
        IFileStorageService fileStorageService,
        IHocBaExcelService excelService,
        IHocBaDiemCalculator diemCalculator,
        ApplicationDbContext context,
        ILogger<HocBaDoiChieuService> logger) : IHocBaDoiChieuService
    {
        private class NguyenVongItem
        {
            public string? SoDDCN { get; set; }
            public int ThuTuNV { get; set; }
            public string? MaXetTuyen { get; set; }
            public string? TenNganh { get; set; }
        }

        public async Task<KetQuaDoiChieu> DoiChieuAsync(string hocBaFileId, string nguyenVongFileId)
        {
            var ketQua = new KetQuaDoiChieu();

            var fileHocBaPath = fileStorageService.GetUploadPath(hocBaFileId);
            var fileNguyenVongPath = fileStorageService.GetUploadPath(nguyenVongFileId);

            if (!fileStorageService.FileExists(hocBaFileId))
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "File học bạ không tồn tại hoặc đã hết hạn.";
                return ketQua;
            }

            if (!fileStorageService.FileExists(nguyenVongFileId))
            {
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "File nguyện vọng không tồn tại hoặc đã hết hạn.";
                return ketQua;
            }

            // 1. Đọc file học bạ
            List<HocBaTHPTImport> danhSachHocBa;
            try
            {
                danhSachHocBa = excelService.ReadHocBaFile(fileHocBaPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi đọc file học bạ đối chiếu {FileId}.", hocBaFileId);
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "Lỗi khi đọc file học bạ: " + ex.Message;
                return ketQua;
            }

            var hocBaTheoCccd = danhSachHocBa
                .GroupBy(r => r.SoDDCN)
                .ToDictionary(g => g.Key!, g => g.ToList());

            // 2. Đọc file nguyện vọng
            List<NguyenVongItem> danhSachNV;
            try
            {
                danhSachNV = ReadNguyenVongFile(fileNguyenVongPath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi đọc file nguyện vọng đối chiếu {FileId}.", nguyenVongFileId);
                ketQua.ThanhCong = false;
                ketQua.ThongBao = "Lỗi khi đọc file nguyện vọng: " + ex.Message;
                return ketQua;
            }

            ketQua.TongNguyenVong = danhSachNV.Count;

            // 3. Nạp danh mục ngành từ CSDL
            var danhSachNganh = await context.Nganhs
                .AsNoTracking()
                .Include(n => n.ToHopNganhs)
                    .ThenInclude(th => th.ToHopMon)
                        .ThenInclude(t => t.MonHocs)
                .ToListAsync();

            var nganhDict = danhSachNganh
                .Where(n => !string.IsNullOrEmpty(n.MaNganh))
                .ToDictionary(n => n.MaNganh!.Trim(), n => n, StringComparer.OrdinalIgnoreCase);

            // 4. Xử lý so khớp từng nguyện vọng
            var maNganhKhongTimThay = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var danhSachTam = new List<KetQuaDoiChieuItem>();
            var thiSinhKhongHocBa = new HashSet<string>();
            var listNVThongKe = new List<(string Cccd, int ThuTuNV, string MaNganh, string TenNganh, string Loai)>();

            foreach (var nv in danhSachNV)
            {
                var cccd = nv.SoDDCN!;
                var thuTuNV = nv.ThuTuNV;
                var maNganh = nv.MaXetTuyen!.Trim();
                var tenNganh = nv.TenNganh ?? string.Empty;

                if (nganhDict.TryGetValue(maNganh, out var nganhEntity))
                {
                    if (!string.IsNullOrEmpty(nganhEntity.TenNganh))
                    {
                        tenNganh = nganhEntity.TenNganh;
                    }

                    if (nganhEntity.HeSoHB == 0)
                    {
                        listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "BoQua"));
                        continue;
                    }
                }
                else
                {
                    maNganhKhongTimThay.Add(maNganh);
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongDiemCN"));
                    continue;
                }

                if (!hocBaTheoCccd.TryGetValue(cccd, out var hocBaThiSinh))
                {
                    thiSinhKhongHocBa.Add(cccd);
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongHocBa"));
                    continue;
                }

                var hoVaTen = hocBaThiSinh.FirstOrDefault()?.HoVaTen ?? string.Empty;
                var lopHB10 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 10);
                var lopHB11 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 11);
                var lopHB12 = hocBaThiSinh.FirstOrDefault(r => r.Lop == 12);
                var cacNamRecord = new[]
                {
                    (Nam: "Lớp 10", Record: lopHB10),
                    (Nam: "Lớp 11", Record: lopHB11),
                    (Nam: "Lớp 12", Record: lopHB12)
                };

                if (nganhEntity.ToHopNganhs == null || !nganhEntity.ToHopNganhs.Any())
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongDiemCN"));
                    continue;
                }

                bool hasAnyValidToHop = false;
                bool hasAnyScoreData = false;

                foreach (var toHopNganh in nganhEntity.ToHopNganhs)
                {
                    var toHop = toHopNganh.ToHopMon;
                    if (toHop?.MonHocs == null || !toHop.MonHocs.Any()) continue;

                    bool toHopDu = true;
                    foreach (var (namHoc, record) in cacNamRecord)
                    {
                        var cacMonThieuTrongNam = new List<string>();
                        foreach (var monHoc in toHop.MonHocs)
                        {
                            decimal? diem = record == null ? null : diemCalculator.GetScore(record, monHoc.FieldName);
                            if (diem != null)
                            {
                                hasAnyScoreData = true;
                            }
                            else
                            {
                                toHopDu = false;
                                cacMonThieuTrongNam.Add(diemCalculator.LayTenHienThiMonHocCN(monHoc.FieldName));
                            }
                        }

                        if (cacMonThieuTrongNam.Count > 0)
                        {
                            danhSachTam.Add(new KetQuaDoiChieuItem
                            {
                                SoDDCN = cccd,
                                HoVaTen = hoVaTen,
                                ThuTuNV = thuTuNV,
                                MaNganh = nganhEntity.MaNganh,
                                TenNganh = nganhEntity.TenNganh,
                                MaToHop = toHop.MaToHop,
                                NamHoc = namHoc,
                                MonThieu = string.Join(", ", cacMonThieuTrongNam)
                            });
                        }
                    }

                    if (toHopDu)
                    {
                        hasAnyValidToHop = true;
                    }
                }

                if (hasAnyValidToHop)
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "CoToHopDu"));
                }
                else if (!hasAnyScoreData)
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "KhongDiemCN"));
                }
                else
                {
                    listNVThongKe.Add((cccd, thuTuNV, maNganh, tenNganh, "ThieuMoiToHop"));
                }
            }

            var ketQuaThieuDiem = danhSachTam
                .GroupBy(x => new { x.SoDDCN, x.HoVaTen, x.ThuTuNV, x.MaNganh, x.TenNganh, x.MaToHop, x.MonThieu })
                .Select(g => new KetQuaDoiChieuItem
                {
                    SoDDCN = g.Key.SoDDCN,
                    HoVaTen = g.Key.HoVaTen,
                    ThuTuNV = g.Key.ThuTuNV,
                    MaNganh = g.Key.MaNganh,
                    TenNganh = g.Key.TenNganh,
                    MaToHop = g.Key.MaToHop,
                    NamHoc = string.Join(", ", g.Select(x => x.NamHoc)),
                    MonThieu = g.Key.MonThieu
                })
                .OrderBy(x => x.SoDDCN)
                .ThenBy(x => x.ThuTuNV)
                .ToList();

            var listNVLoi = listNVThongKe
                .Where(x => x.Loai == "ThieuMoiToHop" || x.Loai == "KhongHocBa" || x.Loai == "KhongDiemCN")
                .ToList();

            var dsThiSinhBiAnhHuong = listNVLoi
                .GroupBy(x => x.Cccd)
                .Select((g, idx) =>
                {
                    var cccd = g.Key;
                    var hoTen = hocBaTheoCccd.TryGetValue(cccd, out var hbList) && hbList.Any()
                        ? hbList.First().HoVaTen
                        : "Chưa có học bạ";

                    var chiTietList = g.Select(x =>
                    {
                        string loaiText = x.Loai switch
                        {
                            "KhongHocBa" => "Không có học bạ",
                            "KhongDiemCN" => "Trống điểm CN",
                            "ThieuMoiToHop" => "Thiếu điểm mọi tổ hợp",
                            _ => x.Loai
                        };
                        return $"NV{x.ThuTuNV}: {x.MaNganh} ({loaiText})";
                    });

                    return new ThiSinhBiAnhHuongItem
                    {
                        Stt = idx + 1,
                        Cccd = cccd,
                        HoVaTen = hoTen ?? "Chưa có học bạ",
                        SoNVLoi = g.Count(),
                        ChiTietNVLoi = string.Join("; ", chiTietList)
                    };
                })
                .ToList();

            var dsNganhBiAnhHuong = listNVLoi
                .GroupBy(x => new { x.MaNganh, x.TenNganh })
                .Select((g, idx) => new NganhBiAnhHuongItem
                {
                    Stt = idx + 1,
                    MaXetTuyen = g.Key.MaNganh,
                    TenNganh = g.Key.TenNganh,
                    SoThiSinhBiAnhHuong = g.Select(x => x.Cccd).Distinct().Count(),
                    SoNVLoi = g.Count()
                })
                .OrderByDescending(x => x.SoNVLoi)
                .ToList();

            var thongKeTongHop = new ThongKeTongHopViewModel
            {
                TongDongNguyenVong = listNVThongKe.Count,
                TongThiSinhDuyNhat = listNVThongKe.Select(x => x.Cccd).Distinct().Count(),
                TongThiSinhBiAnhHuong = dsThiSinhBiAnhHuong.Count,
                TongNganhBiAnhHuong = dsNganhBiAnhHuong.Count,
                NguyenVongCoToHopDu = listNVThongKe.Count(x => x.Loai == "CoToHopDu"),
                NguyenVongThieuMoiToHop = listNVThongKe.Count(x => x.Loai == "ThieuMoiToHop"),
                NguyenVongKhongHocBa = listNVThongKe.Count(x => x.Loai == "KhongHocBa"),
                NguyenVongKhongDiemCN = listNVThongKe.Count(x => x.Loai == "KhongDiemCN"),
                NguyenVongBoQua = listNVThongKe.Count(x => x.Loai == "BoQua"),

                DanhSachThieuMoiToHop = listNVThongKe.Where(x => x.Loai == "ThieuMoiToHop")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                DanhSachKhongHocBa = listNVThongKe.Where(x => x.Loai == "KhongHocBa")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                DanhSachKhongDiemCN = listNVThongKe.Where(x => x.Loai == "KhongDiemCN")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                DanhSachBoQua = listNVThongKe.Where(x => x.Loai == "BoQua")
                    .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),

                DanhSachThiSinhBiAnhHuong = dsThiSinhBiAnhHuong,
                DanhSachNganhBiAnhHuong = dsNganhBiAnhHuong
            };

            var thongKeTheoNganh = listNVThongKe
                .GroupBy(x => new { x.MaNganh, x.TenNganh })
                .Select(g =>
                {
                    int tongNV = g.Count();
                    int soThiSinh = g.Select(x => x.Cccd).Distinct().Count();
                    int nvCoToHopDu = g.Count(x => x.Loai == "CoToHopDu");
                    int nvThieuMoiToHop = g.Count(x => x.Loai == "ThieuMoiToHop");
                    int nvKhongDiemCN = g.Count(x => x.Loai == "KhongDiemCN");
                    int nvKhongHocBa = g.Count(x => x.Loai == "KhongHocBa");
                    int nvBoQua = g.Count(x => x.Loai == "BoQua");
                    int tongThieu = nvThieuMoiToHop + nvKhongDiemCN + nvKhongHocBa;
                    double tyLeThieu = tongNV > 0 ? Math.Round((double)tongThieu / tongNV * 100, 2) : 0;

                    return new ThongKeTheoNganhItemViewModel
                    {
                        MaXetTuyen = g.Key.MaNganh,
                        TenNganh = g.Key.TenNganh,
                        TongNV = tongNV,
                        SoThiSinh = soThiSinh,
                        NVCoToHopDu = nvCoToHopDu,
                        NVThieuMoiToHop = nvThieuMoiToHop,
                        NVKhongDiemCN = nvKhongDiemCN,
                        NVKhongHocBa = nvKhongHocBa,
                        NVBoQua = nvBoQua,
                        TyLeThieu = tyLeThieu,

                        DanhSachThieuMoiToHop = g.Where(x => x.Loai == "ThieuMoiToHop")
                            .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                        DanhSachKhongHocBa = g.Where(x => x.Loai == "KhongHocBa")
                            .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList(),
                        DanhSachKhongDiemCN = g.Where(x => x.Loai == "KhongDiemCN")
                            .Select(x => new ChiTietNguyenVongLoiItem { Cccd = x.Cccd, ThuTuNV = x.ThuTuNV, MaXetTuyen = x.MaNganh, TenNganh = x.TenNganh }).ToList()
                    };
                })
                .OrderByDescending(x => x.TongNV)
                .ToList();

            ketQua.TongLoiKhongTimThayNganh = maNganhKhongTimThay.Count;
            ketQua.DanhSachMaNganhKhongTim = maNganhKhongTimThay.ToList();
            ketQua.DanhSachThieuDiem = ketQuaThieuDiem;

            ketQua.ThongKeTongHop = thongKeTongHop;
            ketQua.ThongKeTheoNganh = thongKeTheoNganh;

            ketQua.ThanhCong = true;
            if (maNganhKhongTimThay.Count > 0)
            {
                ketQua.ThongBao = $"Hoàn tất. Có {maNganhKhongTimThay.Count} mã xét tuyển không tìm thấy trong CSDL: {string.Join(", ", maNganhKhongTimThay)}.";
            }

            return ketQua;
        }

        private static List<NguyenVongItem> ReadNguyenVongFile(string filePath)
        {
            using var pkgNV = new ExcelPackage(new FileInfo(filePath));
            var sheetNV = pkgNV.Workbook.Worksheets[0];
            int totalRowsNV = sheetNV.Dimension.End.Row;

            var danhSachNV = new List<NguyenVongItem>();
            for (int r = 6; r <= totalRowsNV; r++)
            {
                var cccd = ExcelHelper.ParseString(sheetNV.Cells[r, 2].Value);
                var thuTuNV = ExcelHelper.ParseInt(sheetNV.Cells[r, 3].Value) ?? 0;
                var maXetTuyen = ExcelHelper.ParseString(sheetNV.Cells[r, 6].Value);
                var tenNganh = ExcelHelper.ParseString(sheetNV.Cells[r, 7].Value);
                if (!string.IsNullOrWhiteSpace(cccd) && !string.IsNullOrWhiteSpace(maXetTuyen))
                {
                    danhSachNV.Add(new NguyenVongItem
                    {
                        SoDDCN = cccd,
                        ThuTuNV = thuTuNV,
                        MaXetTuyen = maXetTuyen,
                        TenNganh = tenNganh
                    });
                }
            }
            return danhSachNV;
        }
    }
}
