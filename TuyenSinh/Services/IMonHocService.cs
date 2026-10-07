using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IMonHocService
    {
        Task<List<MonHoc>> LayDanhSachMonHocAsync();
        Task<ServiceResult> ThemMonHocAsync(string tenMonHoc, string fieldName);
        Task<ServiceResult> SuaMonHocAsync(int id, string tenMonHoc, string fieldName);
        Task<ServiceResult> XoaMonHocAsync(int id);
    }
}
