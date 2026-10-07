using System.Collections.Generic;
using System.IO;
using TuyenSinh.Common;
using TuyenSinh.Models;

namespace TuyenSinh.Services.Excel
{
    public interface IDiemCongExcelService
    {
        (List<DiemCong> Records, int SkippedCount) ReadImportStream(Stream stream, int namHoc);
        FileDownloadDto Export(IReadOnlyList<DiemCong> data, int? namHoc);
        FileDownloadDto CreateTemplate();
    }
}
