using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Models;
using TuyenSinh.Services.Excel;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public sealed class HocBaDiemSanService(
        IFileStorageService fileStorageService,
        IHocBaExcelService excelService,
        ApplicationDbContext context,
        ILogger<HocBaDiemSanService> logger) : IHocBaDiemSanService
    {
        public async Task<List<Nganh>> LayDanhSachNganhAsync()
        {
            return await context.Nganhs
                .AsNoTracking()
                .OrderBy(n => n.MaNganh)
                .ToListAsync();
        }

        public async Task<KetQuaKiemTraDiemSan> KiemTraDiemSanAsync(string maNganh, string fileId)
        {
            var result = new KetQuaKiemTraDiemSan();

            if (string.IsNullOrEmpty(fileId))
            {
                result.ThanhCong = false;
                result.ThongBao = "Mã tệp Excel không hợp lệ.";
                return result;
            }

            var filePath = File.Exists(fileId) ? fileId : fileStorageService.GetUploadPath(fileId);

            if (!File.Exists(filePath))
            {
                result.ThanhCong = false;
                result.ThongBao = "Tệp Excel không tồn tại trên hệ thống.";
                return result;
            }

            List<KetQuaNguyenVongImport> danhSach;
            try
            {
                danhSach = excelService.ReadKetQuaNguyenVongExcel(filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi đọc file kết quả nguyện vọng kiểm tra điểm sàn {FileId}.", fileId);
                result.ThanhCong = false;
                result.ThongBao = "Có lỗi xảy ra khi đọc tệp Excel: " + ex.Message;
                return result;
            }

            var danhSachNganh = await context.Nganhs
                .AsNoTracking()
                .Include(n => n.ToHopNganhs)
                    .ThenInclude(th => th.ToHopMon)
                .ToListAsync();

            var nganhDict = danhSachNganh
                .Where(n => !string.IsNullOrEmpty(n.MaNganh))
                .ToDictionary(n => n.MaNganh.Trim(), n => n, StringComparer.OrdinalIgnoreCase);

            // Nếu người dùng chọn 1 ngành cụ thể, chỉ kiểm tra các học sinh thuộc ngành đó
            if (!string.IsNullOrWhiteSpace(maNganh))
            {
                danhSach = danhSach
                    .Where(r => !string.IsNullOrWhiteSpace(r.MaNganh) && r.MaNganh.Trim().Equals(maNganh.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var danhSachKiemTra = new List<BaoCaoKiemTraDiemSanItem>();

            foreach (var item in danhSach)
            {
                var ghiChuList = new List<string>();

                // 1. Kiểm tra MaNganh có tồn tại trong hệ thống không
                Nganh? nganh = null;
                if (string.IsNullOrWhiteSpace(item.MaNganh))
                {
                    ghiChuList.Add("Mã ngành trống");
                }
                else if (!nganhDict.TryGetValue(item.MaNganh.Trim(), out nganh))
                {
                    ghiChuList.Add($"Mã ngành '{item.MaNganh}' không tồn tại trong hệ thống");
                }
                else
                {
                    // 2. Kiểm tra ToHop có trong ngành đó không
                    if (string.IsNullOrWhiteSpace(item.ToHop))
                    {
                        ghiChuList.Add("Mã tổ hợp trống");
                    }
                    else
                    {
                        bool toHopValid = nganh.ToHopNganhs.Any(th => th.ToHopMon != null &&
                            th.ToHopMon.MaToHop.Trim().Equals(item.ToHop.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (!toHopValid)
                        {
                            ghiChuList.Add($"Tổ hợp '{item.ToHop}' không thuộc ngành {nganh.MaNganh}");
                        }
                    }
                }

                string mon1Ten = string.IsNullOrWhiteSpace(item.Mon1) ? "Môn 1" : item.Mon1;
                string mon2Ten = string.IsNullOrWhiteSpace(item.Mon2) ? "Môn 2" : item.Mon2;
                string mon3Ten = string.IsNullOrWhiteSpace(item.Mon3) ? "Môn 3" : item.Mon3;

                // Nếu HeSoHB > 0 hoặc không tìm thấy ngành, mới yêu cầu kiểm tra điểm học bạ
                bool kiemTraHocBa = nganh == null || nganh.HeSoHB > 0;

                if (kiemTraHocBa)
                {
                    if (item.DiemMon1HB == null) ghiChuList.Add($"Thiếu điểm HB môn {mon1Ten}");
                    if (item.DiemMon2HB == null) ghiChuList.Add($"Thiếu điểm HB môn {mon2Ten}");
                    if (item.DiemMon3HB == null) ghiChuList.Add($"Thiếu điểm HB môn {mon3Ten}");
                }

                if (item.DiemMon1THPT == null) ghiChuList.Add($"Thiếu điểm THPT môn {mon1Ten}");
                if (item.DiemMon2THPT == null) ghiChuList.Add($"Thiếu điểm THPT môn {mon2Ten}");
                if (item.DiemMon3THPT == null) ghiChuList.Add($"Thiếu điểm THPT môn {mon3Ten}");

                // 4. Kiểm tra điểm xét tuyển của học sinh với DXT của ngành
                if (nganh != null)
                {
                    if (nganh.DXT > 0 && (item.DiemXetTuyen == null || item.DiemXetTuyen < nganh.DXT))
                    {
                        ghiChuList.Add($"Điểm xét tuyển ({item.DiemXetTuyen ?? 0}) dưới điểm sàn ngành ({nganh.DXT})");
                    }

                    // 5. Kiểm tra điểm sàn Toán (nếu DiemSanToan > 0)
                    if (nganh.DiemSanToan > 0)
                    {
                        if (item.DiemMon1THPT == null || item.DiemMon1THPT < nganh.DiemSanToan)
                        {
                            ghiChuList.Add($"Điểm THPT môn Toán ({item.DiemMon1THPT ?? 0}) dưới điểm sàn môn Toán ({nganh.DiemSanToan})");
                        }
                    }
                }

                if (ghiChuList.Count > 0)
                {
                    danhSachKiemTra.Add(new BaoCaoKiemTraDiemSanItem
                    {
                        HoTen = item.HoTen,
                        CCCD = item.CCCD,
                        MaNganh = item.MaNganh ?? "",
                        ToHop = item.ToHop ?? "",
                        DiemXetTuyen = item.DiemXetTuyen ?? 0,
                        DiemSan = nganh?.DXT ?? 0,
                        DiemSanToan = nganh?.DiemSanToan ?? 0,
                        GhiChu = string.Join("; ", ghiChuList)
                    });
                }
            }

            result.ThanhCong = true;
            result.ThongBao = "Kiểm tra điểm sàn hoàn tất.";
            result.TongSoThiSinh = danhSach.Count;
            result.SoThiSinhKhongDat = danhSachKiemTra.Count;
            result.SoThiSinhDat = Math.Max(0, result.TongSoThiSinh - result.SoThiSinhKhongDat);
            result.DanhSachKiemTraDiemSan = danhSachKiemTra;

            return result;
        }
    }
}
