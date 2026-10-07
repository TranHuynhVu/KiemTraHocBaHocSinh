using System.Collections.Generic;
using TuyenSinh.Common;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.Excel
{
    public interface IHocBaExcelService
    {
        List<HocBaTHPTImport> ReadHocBaFile(string filePath, int? limit = null);
        List<KetQuaNguyenVongImport> ReadKetQuaNguyenVongExcel(string filePath);
        FileDownloadDto ExportThieuDiemToHop(KetQuaKiemTraHocBa ketQua);
        FileDownloadDto ExportKetQuaDoiChieu(KetQuaDoiChieu ketQua);
        FileDownloadDto ExportKiemTraDiemSan(KetQuaKiemTraDiemSan ketQua);
    }
}
