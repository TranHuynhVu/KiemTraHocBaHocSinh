using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public sealed class MonHocService(ApplicationDbContext context) : IMonHocService
    {
        public async Task<List<MonHoc>> LayDanhSachMonHocAsync()
        {
            return await context.MonHocs.AsNoTracking().ToListAsync();
        }

        public async Task<ServiceResult> ThemMonHocAsync(string tenMonHoc, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(tenMonHoc) || string.IsNullOrWhiteSpace(fieldName))
            {
                return ServiceResult.Fail("Tên môn học và Tên trường trong Excel không được để trống.");
            }

            var subject = new MonHoc
            {
                TenMonHoc = tenMonHoc.Trim(),
                FieldName = fieldName.Trim()
            };

            context.MonHocs.Add(subject);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Thêm môn học thành công.");
        }

        public async Task<ServiceResult> SuaMonHocAsync(int id, string tenMonHoc, string fieldName)
        {
            var subject = await context.MonHocs.FindAsync(id);
            if (subject == null)
            {
                return ServiceResult.Fail("Không tìm thấy môn học.");
            }

            if (string.IsNullOrWhiteSpace(tenMonHoc) || string.IsNullOrWhiteSpace(fieldName))
            {
                return ServiceResult.Fail("Tên môn học và Tên trường trong Excel không được để trống.");
            }

            subject.TenMonHoc = tenMonHoc.Trim();
            subject.FieldName = fieldName.Trim();

            context.Update(subject);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Cập nhật môn học thành công.");
        }

        public async Task<ServiceResult> XoaMonHocAsync(int id)
        {
            var subject = await context.MonHocs.FindAsync(id);
            if (subject == null)
            {
                return ServiceResult.Fail("Không tìm thấy môn học.");
            }

            context.MonHocs.Remove(subject);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Xóa môn học thành công.");
        }
    }
}
