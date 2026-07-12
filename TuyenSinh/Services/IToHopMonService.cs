using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IToHopMonService
    {
        Task<List<ToHopMon>> LayDanhSachToHopAsync();
        Task<(bool Success, string Message)> ThemToHopAsync(string maToHop, string tenToHop, List<int> selectedSubjectIds);
        Task<(bool Success, string Message)> SuaToHopAsync(int id, string maToHop, string tenToHop, List<int> selectedSubjectIds);
        Task<(bool Success, string Message)> XoaToHopAsync(int id);
    }
}
