using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public sealed class ToHopMonService(ApplicationDbContext context) : IToHopMonService
    {
        public async Task<List<ToHopMon>> LayDanhSachToHopAsync()
        {
            return await context.ToHopMons.Include(t => t.MonHocs).AsNoTracking().ToListAsync();
        }

        public async Task<ServiceResult> ThemToHopAsync(string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            if (string.IsNullOrWhiteSpace(maToHop) || string.IsNullOrWhiteSpace(tenToHop))
            {
                return ServiceResult.Fail("Mã tổ hợp và Tên tổ hợp không được để trống.");
            }

            if (selectedSubjectIds == null || selectedSubjectIds.Count == 0)
            {
                return ServiceResult.Fail("Vui lòng chọn ít nhất một môn học cho tổ hợp này.");
            }

            var toHopMon = new ToHopMon
            {
                MaToHop = maToHop.Trim().ToUpper(),
                TenToHop = tenToHop.Trim()
            };

            var subjects = await context.MonHocs.Where(m => selectedSubjectIds.Contains(m.Id)).ToListAsync();
            foreach (var subject in subjects)
            {
                toHopMon.MonHocs.Add(subject);
            }

            context.ToHopMons.Add(toHopMon);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Thêm tổ hợp môn thành công.");
        }

        public async Task<ServiceResult> SuaToHopAsync(int id, string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            var toHopMon = await context.ToHopMons.Include(t => t.MonHocs).FirstOrDefaultAsync(t => t.Id == id);
            if (toHopMon == null)
            {
                return ServiceResult.Fail("Không tìm thấy tổ hợp môn.");
            }

            if (string.IsNullOrWhiteSpace(maToHop) || string.IsNullOrWhiteSpace(tenToHop))
            {
                return ServiceResult.Fail("Mã tổ hợp và Tên tổ hợp không được để trống.");
            }

            if (selectedSubjectIds == null || selectedSubjectIds.Count == 0)
            {
                return ServiceResult.Fail("Vui lòng chọn ít nhất một môn học cho tổ hợp này.");
            }

            toHopMon.MaToHop = maToHop.Trim().ToUpper();
            toHopMon.TenToHop = tenToHop.Trim();

            toHopMon.MonHocs.Clear();
            var subjects = await context.MonHocs.Where(m => selectedSubjectIds.Contains(m.Id)).ToListAsync();
            foreach (var subject in subjects)
            {
                toHopMon.MonHocs.Add(subject);
            }

            context.Update(toHopMon);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Cập nhật tổ hợp môn thành công.");
        }

        public async Task<ServiceResult> XoaToHopAsync(int id)
        {
            var toHopMon = await context.ToHopMons.FindAsync(id);
            if (toHopMon == null)
            {
                return ServiceResult.Fail("Không tìm thấy tổ hợp môn.");
            }

            context.ToHopMons.Remove(toHopMon);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Xóa tổ hợp môn thành công.");
        }
    }
}
