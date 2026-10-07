using System.Collections.Generic;
using TuyenSinh.Common;
using TuyenSinh.ViewModels.SoKhopNgoaiNgu;

namespace TuyenSinh.Services.Excel
{
    public interface ISoKhopExcelService
    {
        (Dictionary<string, List<string>> MapNV, int TotalCount) ReadNguyenVongFile(string filePath);
        (Dictionary<string, (string? Sbd, string? HoTen, string? NgaySinh)> MapDSTS, int TotalCount) ReadDstsFile(string filePath);
        (List<(string Ddcn, string Sbd, string ChungChi, decimal DiemBac)> ListNN, Dictionary<string, (string Sbd, string ChungChi, decimal DiemBac)> MapNN, int TotalCount) ReadHopLeNnFile(string filePath);
        FileDownloadDto Export(IReadOnlyList<KetQuaSoKhopNgoaiNguItem> items);
    }
}
