using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Services.Excel;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public sealed class HocBaCheckService(
        IFileStorageService fileStorageService,
        IHocBaExcelService excelService,
        IHocBaDiemCalculator diemCalculator,
        ApplicationDbContext context,
        ILogger<HocBaCheckService> logger) : IHocBaCheckService
    {
        public Task<List<HocBaTHPTImport>?> GetPreviewDataAsync(string excelId, int? limit = null)
        {
            if (string.IsNullOrEmpty(excelId) || !fileStorageService.FileExists(excelId))
            {
                return Task.FromResult<List<HocBaTHPTImport>?>(null);
            }

            var filePath = fileStorageService.GetUploadPath(excelId);
            try
            {
                var records = excelService.ReadHocBaFile(filePath, limit);
                return Task.FromResult<List<HocBaTHPTImport>?>(records);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi lấy dữ liệu xem trước học bạ từ file {ExcelId}.", excelId);
                return Task.FromResult<List<HocBaTHPTImport>?>(null);
            }
        }

        public async Task<KetQuaKiemTraHocBa> CheckHocBaAsync(string excelId)
        {
            var result = new KetQuaKiemTraHocBa();

            if (string.IsNullOrEmpty(excelId))
            {
                result.ThanhCong = false;
                result.ThongBao = "Không tìm thấy mã tập Excel.";
                return result;
            }

            if (!fileStorageService.FileExists(excelId))
            {
                result.ThanhCong = false;
                result.ThongBao = "Tập Excel không tồn tại trên hệ thống.";
                return result;
            }

            var filePath = fileStorageService.GetUploadPath(excelId);

            List<HocBaTHPTImport> records;
            try
            {
                records = excelService.ReadHocBaFile(filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Có lỗi xảy ra khi đọc tập Excel học bạ {ExcelId}.", excelId);
                result.ThanhCong = false;
                result.ThongBao = "Có lỗi xảy ra khi đọc tập Excel: " + ex.Message;
                return result;
            }

            var grouped = records.GroupBy(r => r.SoDDCN).ToList();

            var baoCaoThieuNamHoc = new List<BaoCaoThieuNamHocItem>();
            var baoCaoThieuDiem = new List<BaoCaoThieuDiemItem>();

            var danhSachToHop = await context.ToHopMons.Include(t => t.MonHocs).AsNoTracking().ToListAsync();

            int thieuNamHocStt = 1;
            int thieuDiemStt = 1;

            foreach (var group in grouped)
            {
                var cccd = group.Key;
                var firstRecord = group.First();
                var name = firstRecord.HoVaTen;

                // Kiểm tra 1: Thiếu năm học (Lớp 10, 11, 12)
                var cacLop = group.Select(r => r.Lop).Where(l => l.HasValue).Select(l => l!.Value).Distinct().OrderBy(g => g).ToList();
                bool has10 = cacLop.Contains(10);
                bool has11 = cacLop.Contains(11);
                bool has12 = cacLop.Contains(12);

                if (!has10 || !has11 || !has12)
                {
                    var cacLopHienCo = string.Join(", ", cacLop);
                    var danhSachLopThieu = new List<string>();
                    if (!has10) danhSachLopThieu.Add("Lớp 10");
                    if (!has11) danhSachLopThieu.Add("Lớp 11");
                    if (!has12) danhSachLopThieu.Add("Lớp 12");
                    var namThieu = string.Join(", ", danhSachLopThieu);

                    baoCaoThieuNamHoc.Add(new BaoCaoThieuNamHocItem
                    {
                        Stt = thieuNamHocStt++,
                        Cccd = cccd,
                        HoVaTen = name,
                        NamHienCo = cacLopHienCo,
                        NamThieu = namThieu
                    });
                }

                // Kiểm tra 2: Thiếu điểm theo các tổ hợp môn
                foreach (var gradeRecord in group)
                {
                    int currentGrade = gradeRecord.Lop ?? 0;
                    if (currentGrade != 10 && currentGrade != 11 && currentGrade != 12) continue;

                    foreach (var toHop in danhSachToHop)
                    {
                        var cacMonThieuTrongToHop = new List<string>();

                        foreach (var subject in toHop.MonHocs)
                        {
                            var score = diemCalculator.GetScore(gradeRecord, subject.FieldName);
                            if (score == null)
                            {
                                var displayName = diemCalculator.LayTenHienThiMonHocCN(subject.FieldName);
                                cacMonThieuTrongToHop.Add(displayName);
                            }
                        }

                        if (cacMonThieuTrongToHop.Count > 0)
                        {
                            baoCaoThieuDiem.Add(new BaoCaoThieuDiemItem
                            {
                                Stt = thieuDiemStt++,
                                Cccd = cccd,
                                HoVaTen = name,
                                NamLoi = "Lớp " + currentGrade,
                                ToHop = toHop.MaToHop,
                                MonThieu = string.Join(", ", cacMonThieuTrongToHop)
                            });
                        }
                    }
                }
            }

            var gopBaoCaoThieuDiem = baoCaoThieuDiem
                .GroupBy(x => new { x.Cccd, x.HoVaTen, x.ToHop, x.MonThieu })
                .Select((g, idx) => new BaoCaoThieuDiemItem
                {
                    Stt = idx + 1,
                    Cccd = g.Key.Cccd,
                    HoVaTen = g.Key.HoVaTen,
                    ToHop = g.Key.ToHop,
                    NamLoi = string.Join(", ", g.Select(x => x.NamLoi)),
                    MonThieu = g.Key.MonThieu
                })
                .ToList();

            result.ThanhCong = true;
            result.DanhSachThieuNamHoc = baoCaoThieuNamHoc;
            result.DanhSachThieuDiem = gopBaoCaoThieuDiem;

            return result;
        }
    }
}
