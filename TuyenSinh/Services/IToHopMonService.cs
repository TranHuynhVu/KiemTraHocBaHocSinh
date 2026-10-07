using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IToHopMonService
    {
        Task<List<ToHopMon>> LayDanhSachToHopAsync();
        Task<ServiceResult> ThemToHopAsync(string maToHop, string tenToHop, List<int> selectedSubjectIds);
        Task<ServiceResult> SuaToHopAsync(int id, string maToHop, string tenToHop, List<int> selectedSubjectIds);
        Task<ServiceResult> XoaToHopAsync(int id);
    }
}
