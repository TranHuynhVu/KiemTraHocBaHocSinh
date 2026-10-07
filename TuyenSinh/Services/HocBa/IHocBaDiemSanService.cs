using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Models;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public interface IHocBaDiemSanService
    {
        Task<KetQuaKiemTraDiemSan> KiemTraDiemSanAsync(string maNganh, string fileId);
        Task<List<Nganh>> LayDanhSachNganhAsync();
    }
}
