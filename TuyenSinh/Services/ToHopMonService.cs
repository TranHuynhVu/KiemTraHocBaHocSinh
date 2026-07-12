using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public class ToHopMonService : IToHopMonService
    {
        private readonly ApplicationDbContext _context;

        public ToHopMonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ToHopMon>> LayDanhSachToHopAsync()
        {
            return await _context.ToHopMons.Include(t => t.MonHocs).ToListAsync();
        }

        public async Task<(bool Success, string Message)> ThemToHopAsync(string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            if (string.IsNullOrWhiteSpace(maToHop) || string.IsNullOrWhiteSpace(tenToHop))
            {
                return (false, "Mã tổ hợp và Tên tổ hợp không được để trống.");
            }

            if (selectedSubjectIds == null || selectedSubjectIds.Count == 0)
            {
                return (false, "Vui lòng chọn ít nhất một môn học cho tổ hợp này.");
            }

            var toHopMon = new ToHopMon
            {
                MaToHop = maToHop.Trim().ToUpper(),
                TenToHop = tenToHop.Trim()
            };

            var subjects = await _context.MonHocs.Where(m => selectedSubjectIds.Contains(m.Id)).ToListAsync();
            foreach (var subject in subjects)
            {
                toHopMon.MonHocs.Add(subject);
            }

            _context.ToHopMons.Add(toHopMon);
            await _context.SaveChangesAsync();
            return (true, "Thêm tổ hợp môn thành công.");
        }

        public async Task<(bool Success, string Message)> SuaToHopAsync(int id, string maToHop, string tenToHop, List<int> selectedSubjectIds)
        {
            var toHopMon = await _context.ToHopMons.Include(t => t.MonHocs).FirstOrDefaultAsync(t => t.Id == id);
            if (toHopMon == null)
            {
                return (false, "Không tìm thấy tổ hợp môn.");
            }

            if (string.IsNullOrWhiteSpace(maToHop) || string.IsNullOrWhiteSpace(tenToHop))
            {
                return (false, "Mã tổ hợp và Tên tổ hợp không được để trống.");
            }

            if (selectedSubjectIds == null || selectedSubjectIds.Count == 0)
            {
                return (false, "Vui lòng chọn ít nhất một môn học cho tổ hợp này.");
            }

            toHopMon.MaToHop = maToHop.Trim().ToUpper();
            toHopMon.TenToHop = tenToHop.Trim();

            toHopMon.MonHocs.Clear();
            var subjects = await _context.MonHocs.Where(m => selectedSubjectIds.Contains(m.Id)).ToListAsync();
            foreach (var subject in subjects)
            {
                toHopMon.MonHocs.Add(subject);
            }

            _context.Update(toHopMon);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật tổ hợp môn thành công.");
        }

        public async Task<(bool Success, string Message)> XoaToHopAsync(int id)
        {
            var toHopMon = await _context.ToHopMons.FindAsync(id);
            if (toHopMon == null)
            {
                return (false, "Không tìm thấy tổ hợp môn.");
            }

            _context.ToHopMons.Remove(toHopMon);
            await _context.SaveChangesAsync();
            return (true, "Xóa tổ hợp môn thành công.");
        }
    }
}
