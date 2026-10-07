using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public interface IHocBaCheckService
    {
        Task<List<HocBaTHPTImport>?> GetPreviewDataAsync(string excelId, int? limit = null);
        Task<KetQuaKiemTraHocBa> CheckHocBaAsync(string excelId);
    }
}
