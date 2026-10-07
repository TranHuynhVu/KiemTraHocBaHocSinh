using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Services.Excel;
using TuyenSinh.ViewModels.SoKhopNgoaiNgu;

namespace TuyenSinh.Services
{
    public sealed class SoKhopNgoaiNguService(
        IFileStorageService fileStorageService,
        IQuyDoiNNService quyDoiNNService,
        ISoKhopExcelService excelService,
        ILogger<SoKhopNgoaiNguService> logger) : ISoKhopNgoaiNguService
    {
        public async Task<SoKhopNgoaiNguThongKeViewModel> Join3ExcelFilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null)
        {
            var pathNV = fileStorageService.GetUploadPath(nvFileId);
            var pathDSTS = fileStorageService.GetUploadPath(dstsFileId);
            var pathNN = fileStorageService.GetUploadPath(nnFileId);

            // Đọc 3 file Excel qua ExcelService
            var (mapNV, totalCountNV) = excelService.ReadNguyenVongFile(pathNV);
            var (mapDSTS, totalCountDSTS) = excelService.ReadDstsFile(pathDSTS);
            var (listNN, _, totalCountNN) = excelService.ReadHopLeNnFile(pathNN);

            var listLoaiNN = await quyDoiNNService.DanhSachDiemQuyDoiAsync();

            var resultItems = new List<KetQuaSoKhopNgoaiNguItem>();

            foreach (var nnItem in listNN)
            {
                var ddcn = nnItem.Ddcn;
                if (string.IsNullOrEmpty(ddcn)) continue;

                mapDSTS.TryGetValue(ddcn, out var dstsInfo);
                mapNV.TryGetValue(ddcn, out var mxtList);

                string sbd = dstsInfo.Sbd ?? nnItem.Sbd;
                string hoTen = dstsInfo.HoTen ?? string.Empty;
                string ngaySinh = dstsInfo.NgaySinh ?? string.Empty;
                string chungChi = nnItem.ChungChi;
                decimal diemBac = nnItem.DiemBac;
                string maXetTuyen = mxtList != null && mxtList.Any()
                    ? string.Join("; ", mxtList.Distinct())
                    : string.Empty;
                decimal? diemQuyDoi = quyDoiNNService.LayDiemQuyDoiNNTheoTenLoai(listLoaiNN, chungChi, diemBac);

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

            // Tìm kiếm
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

            // Đánh số thứ tự
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

        public async Task<ServiceResult<FileDownloadDto>> XuatExcel3FilesAsync(string nvFileId, string dstsFileId, string nnFileId, string? search = null)
        {
            var data = await Join3ExcelFilesAsync(nvFileId, dstsFileId, nnFileId, search);
            if (data.DanhSachKetQua == null || !data.DanhSachKetQua.Any())
            {
                return ServiceResult<FileDownloadDto>.Fail("Không có dữ liệu so khớp để xuất Excel.");
            }

            try
            {
                var fileDto = excelService.Export(data.DanhSachKetQua);
                return ServiceResult<FileDownloadDto>.Ok(fileDto, fileDto.FileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xuất file Excel so khớp ngoại ngữ.");
                return ServiceResult<FileDownloadDto>.Fail("Lỗi khi xuất file Excel: " + ex.Message);
            }
        }
    }
}