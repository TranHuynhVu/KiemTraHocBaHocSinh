using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IMonHocService
    {
        Task<List<MonHoc>> LayDanhSachMonHocAsync();
        Task<(bool Success, string Message)> ThemMonHocAsync(string tenMonHoc, string fieldName);
        Task<(bool Success, string Message)> SuaMonHocAsync(int id, string tenMonHoc, string fieldName);
        Task<(bool Success, string Message)> XoaMonHocAsync(int id);
    }
}
