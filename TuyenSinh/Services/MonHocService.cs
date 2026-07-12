using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public class MonHocService : IMonHocService
    {
        private readonly ApplicationDbContext _context;

        public MonHocService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MonHoc>> LayDanhSachMonHocAsync()
        {
            return await _context.MonHocs.ToListAsync();
        }

        public async Task<(bool Success, string Message)> ThemMonHocAsync(string tenMonHoc, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(tenMonHoc) || string.IsNullOrWhiteSpace(fieldName))
            {
                return (false, "Tên môn học và Tên trường trong Excel không được để trống.");
            }

            var subject = new MonHoc
            {
                TenMonHoc = tenMonHoc.Trim(),
                FieldName = fieldName.Trim()
            };

            _context.MonHocs.Add(subject);
            await _context.SaveChangesAsync();
            return (true, "Thêm môn học thành công.");
        }

        public async Task<(bool Success, string Message)> SuaMonHocAsync(int id, string tenMonHoc, string fieldName)
        {
            var subject = await _context.MonHocs.FindAsync(id);
            if (subject == null)
            {
                return (false, "Không tìm thấy môn học.");
            }

            if (string.IsNullOrWhiteSpace(tenMonHoc) || string.IsNullOrWhiteSpace(fieldName))
            {
                return (false, "Tên môn học và Tên trường trong Excel không được để trống.");
            }

            subject.TenMonHoc = tenMonHoc.Trim();
            subject.FieldName = fieldName.Trim();

            _context.Update(subject);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật môn học thành công.");
        }

        public async Task<(bool Success, string Message)> XoaMonHocAsync(int id)
        {
            var subject = await _context.MonHocs.FindAsync(id);
            if (subject == null)
            {
                return (false, "Không tìm thấy môn học.");
            }

            _context.MonHocs.Remove(subject);
            await _context.SaveChangesAsync();
            return (true, "Xóa môn học thành công.");
        }
    }
}
