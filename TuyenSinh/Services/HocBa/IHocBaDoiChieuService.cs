using System.Threading.Tasks;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public interface IHocBaDoiChieuService
    {
        Task<KetQuaDoiChieu> DoiChieuAsync(string hocBaFileId, string nguyenVongFileId);
    }
}
