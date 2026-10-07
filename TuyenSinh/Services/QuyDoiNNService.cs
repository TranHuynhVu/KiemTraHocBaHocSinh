using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public sealed class QuyDoiNNService(ApplicationDbContext context) : IQuyDoiNNService
    {
        public async Task<List<BacNgoaiNgu>> LayDanhSachBacAsync()
        {
            return await context.BacNgoaiNgus.AsNoTracking().ToListAsync();
        }

        public async Task<BacNgoaiNgu?> LayBacTheoIdAsync(int id)
        {
            return await context.BacNgoaiNgus.FindAsync(id);
        }

        public async Task<ServiceResult> ThemBacAsync(string tenBac, string tenVietTat)
        {
            if (string.IsNullOrWhiteSpace(tenBac) || string.IsNullOrWhiteSpace(tenVietTat))
            {
                return ServiceResult.Fail("Tên bậc và tên viết tắt không được để trống.");
            }

            tenBac = tenBac.Trim();
            tenVietTat = tenVietTat.Trim();

            var exists = await context.BacNgoaiNgus.AnyAsync(x => x.TenBac.ToLower() == tenBac.ToLower() || x.TenVietTat.ToLower() == tenVietTat.ToLower());
            if (exists)
            {
                return ServiceResult.Fail("Bậc ngoại ngữ hoặc tên viết tắt này đã tồn tại.");
            }

            var bac = new BacNgoaiNgu
            {
                TenBac = tenBac,
                TenVietTat = tenVietTat
            };

            context.BacNgoaiNgus.Add(bac);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Thêm bậc ngoại ngữ thành công.");
        }

        public async Task<ServiceResult> SuaBacAsync(int id, string tenBac, string tenVietTat)
        {
            if (string.IsNullOrWhiteSpace(tenBac) || string.IsNullOrWhiteSpace(tenVietTat))
            {
                return ServiceResult.Fail("Tên bậc và tên viết tắt không được để trống.");
            }

            var bac = await context.BacNgoaiNgus.FindAsync(id);
            if (bac == null)
            {
                return ServiceResult.Fail("Không tìm thấy bậc ngoại ngữ cần sửa.");
            }

            tenBac = tenBac.Trim();
            tenVietTat = tenVietTat.Trim();

            var exists = await context.BacNgoaiNgus.AnyAsync(x => x.Id != id && (x.TenBac.ToLower() == tenBac.ToLower() || x.TenVietTat.ToLower() == tenVietTat.ToLower()));
            if (exists)
            {
                return ServiceResult.Fail("Tên bậc hoặc tên viết tắt trùng lặp với bậc ngoại ngữ khác.");
            }

            bac.TenBac = tenBac;
            bac.TenVietTat = tenVietTat;

            context.BacNgoaiNgus.Update(bac);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Cập nhật bậc ngoại ngữ thành công.");
        }

        public async Task<ServiceResult> XoaBacAsync(int id)
        {
            var bac = await context.BacNgoaiNgus.FindAsync(id);
            if (bac == null)
            {
                return ServiceResult.Fail("Không tìm thấy bậc ngoại ngữ cần xóa.");
            }

            var dangDung = await context.QuyDoiNNs.AnyAsync(q => q.BacNgoaiNguId == id);
            if (dangDung)
            {
                return ServiceResult.Fail("Không thể xóa bậc ngoại ngữ này vì đang được sử dụng trong bảng quy đổi điểm.");
            }

            context.BacNgoaiNgus.Remove(bac);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Xóa bậc ngoại ngữ thành công.");
        }

        public async Task<List<LoaiNgoaiNgu>> LayDanhSachLoaiAsync()
        {
            return await context.LoaiNgoaiNgus.AsNoTracking().ToListAsync();
        }

        public async Task<LoaiNgoaiNgu?> LayLoaiTheoIdAsync(int id)
        {
            return await context.LoaiNgoaiNgus.FindAsync(id);
        }

        public async Task<ServiceResult> ThemLoaiAsync(string tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai))
            {
                return ServiceResult.Fail("Tên loại ngoại ngữ không được để trống.");
            }

            tenLoai = tenLoai.Trim();
            var exists = await context.LoaiNgoaiNgus.AnyAsync(x => x.TenLoai.ToLower() == tenLoai.ToLower());
            if (exists)
            {
                return ServiceResult.Fail("Loại ngoại ngữ này đã tồn tại.");
            }

            var loai = new LoaiNgoaiNgu
            {
                TenLoai = tenLoai
            };

            context.LoaiNgoaiNgus.Add(loai);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Thêm loại ngoại ngữ thành công.");
        }

        public async Task<ServiceResult> SuaLoaiAsync(int id, string tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai))
            {
                return ServiceResult.Fail("Tên loại ngoại ngữ không được để trống.");
            }

            var loai = await context.LoaiNgoaiNgus.FindAsync(id);
            if (loai == null)
            {
                return ServiceResult.Fail("Không tìm thấy loại ngoại ngữ cần sửa.");
            }

            tenLoai = tenLoai.Trim();
            var exists = await context.LoaiNgoaiNgus.AnyAsync(x => x.Id != id && x.TenLoai.ToLower() == tenLoai.ToLower());
            if (exists)
            {
                return ServiceResult.Fail("Tên loại ngoại ngữ trùng lặp với loại khác.");
            }

            loai.TenLoai = tenLoai;
            context.LoaiNgoaiNgus.Update(loai);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Cập nhật loại ngoại ngữ thành công.");
        }

        public async Task<ServiceResult> XoaLoaiAsync(int id)
        {
            var loai = await context.LoaiNgoaiNgus.FindAsync(id);
            if (loai == null)
            {
                return ServiceResult.Fail("Không tìm thấy loại ngoại ngữ cần xóa.");
            }

            var dangDung = await context.QuyDoiNNs.AnyAsync(q => q.LoaiNgoaiNguId == id);
            if (dangDung)
            {
                return ServiceResult.Fail("Không thể xóa loại ngoại ngữ này vì đang được sử dụng trong bảng quy đổi điểm.");
            }

            context.LoaiNgoaiNgus.Remove(loai);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Xóa loại ngoại ngữ thành công.");
        }

        public async Task<List<QuyDoiNN>> LayDanhSachQuyDoiAsync()
        {
            return await context.QuyDoiNNs
                .Include(q => q.BacNgoaiNgu)
                .Include(q => q.LoaiNgoaiNgu)
                .AsNoTracking()
                .OrderBy(q => q.BacNgoaiNguId)
                .ThenBy(q => q.LoaiNgoaiNguId)
                .ThenBy(q => q.DiemNN)
                .ToListAsync();
        }

        public async Task<QuyDoiNN?> LayQuyDoiTheoIdAsync(int id)
        {
            return await context.QuyDoiNNs
                .Include(q => q.BacNgoaiNgu)
                .Include(q => q.LoaiNgoaiNgu)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<ServiceResult> ThemQuyDoiAsync(int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi)
        {
            var bacExists = await context.BacNgoaiNgus.AnyAsync(b => b.Id == bacNgoaiNguId);
            if (!bacExists) return ServiceResult.Fail("Bậc ngoại ngữ được chọn không hợp lệ.");

            var loaiExists = await context.LoaiNgoaiNgus.AnyAsync(l => l.Id == loaiNgoaiNguId);
            if (!loaiExists) return ServiceResult.Fail("Loại ngoại ngữ được chọn không hợp lệ.");

            if (diemNN < 0 || diemQuyDoi < 0 || diemQuyDoi > 10)
            {
                return ServiceResult.Fail("Điểm ngoại ngữ và điểm quy đổi không hợp lệ (Điểm quy đổi từ 0 đến 10).");
            }

            var duplicate = await context.QuyDoiNNs.AnyAsync(q => q.BacNgoaiNguId == bacNgoaiNguId && q.LoaiNgoaiNguId == loaiNgoaiNguId && q.DiemNN == diemNN);
            if (duplicate)
            {
                return ServiceResult.Fail("Quy tắc quy đổi cho Bậc, Loại và mốc điểm này đã tồn tại.");
            }

            var quyDoi = new QuyDoiNN
            {
                BacNgoaiNguId = bacNgoaiNguId,
                LoaiNgoaiNguId = loaiNgoaiNguId,
                DiemNN = diemNN,
                DiemQuyDoi = diemQuyDoi
            };

            context.QuyDoiNNs.Add(quyDoi);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Thêm quy tắc quy đổi điểm thành công.");
        }

        public async Task<ServiceResult> SuaQuyDoiAsync(int id, int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi)
        {
            var quyDoi = await context.QuyDoiNNs.FindAsync(id);
            if (quyDoi == null)
            {
                return ServiceResult.Fail("Không tìm thấy quy tắc quy đổi điểm cần sửa.");
            }

            var bacExists = await context.BacNgoaiNgus.AnyAsync(b => b.Id == bacNgoaiNguId);
            if (!bacExists) return ServiceResult.Fail("Bậc ngoại ngữ được chọn không hợp lệ.");

            var loaiExists = await context.LoaiNgoaiNgus.AnyAsync(l => l.Id == loaiNgoaiNguId);
            if (!loaiExists) return ServiceResult.Fail("Loại ngoại ngữ được chọn không hợp lệ.");

            if (diemNN < 0 || diemQuyDoi < 0 || diemQuyDoi > 10)
            {
                return ServiceResult.Fail("Điểm ngoại ngữ và điểm quy đổi không hợp lệ (Điểm quy đổi từ 0 đến 10).");
            }

            var duplicate = await context.QuyDoiNNs.AnyAsync(q => q.Id != id && q.BacNgoaiNguId == bacNgoaiNguId && q.LoaiNgoaiNguId == loaiNgoaiNguId && q.DiemNN == diemNN);
            if (duplicate)
            {
                return ServiceResult.Fail("Quy tắc quy đổi cho Bậc, Loại và mốc điểm này đã tồn tại.");
            }

            quyDoi.BacNgoaiNguId = bacNgoaiNguId;
            quyDoi.LoaiNgoaiNguId = loaiNgoaiNguId;
            quyDoi.DiemNN = diemNN;
            quyDoi.DiemQuyDoi = diemQuyDoi;

            context.QuyDoiNNs.Update(quyDoi);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Cập nhật quy tắc quy đổi điểm thành công.");
        }

        public async Task<ServiceResult> XoaQuyDoiAsync(int id)
        {
            var quyDoi = await context.QuyDoiNNs.FindAsync(id);
            if (quyDoi == null)
            {
                return ServiceResult.Fail("Không tìm thấy quy tắc quy đổi điểm cần xóa.");
            }

            context.QuyDoiNNs.Remove(quyDoi);
            await context.SaveChangesAsync();
            return ServiceResult.Ok("Xóa quy tắc quy đổi điểm thành công.");
        }

        public async Task<Dictionary<string, List<QuyDoiNN>>> DanhSachDiemQuyDoiAsync()
        {
            var listLoaiNN = await context.QuyDoiNNs
                                   .AsNoTracking()
                                   .Include(q => q.LoaiNgoaiNgu)
                                   .OrderBy(q => q.LoaiNgoaiNguId)
                                   .ThenBy(q => q.DiemNN)
                                   .GroupBy(q => q.LoaiNgoaiNgu.TenLoai)
                                   .ToDictionaryAsync(g => g.Key, g => g.OrderBy(q => q.DiemNN).ToList(), StringComparer.OrdinalIgnoreCase);
            return listLoaiNN;
        }

        public decimal? LayDiemQuyDoiNNTheoTenLoai(Dictionary<string, List<QuyDoiNN>> danhSachQuyDoi, string tenLoai, decimal diemNN)
        {
            if (string.IsNullOrWhiteSpace(tenLoai) || danhSachQuyDoi == null)
            {
                return null;
            }

            var trimmedLoai = tenLoai.Trim();

            // Tìm key linh hoạt (chính xác hoặc tên chứng chỉ chứa key, VD: "Tiếng Anh - IELTS" khớp với "IELTS")
            var key = danhSachQuyDoi.Keys.FirstOrDefault(k => 
                trimmedLoai.Contains(k, StringComparison.OrdinalIgnoreCase) 
            );

            if (key != null && danhSachQuyDoi.TryGetValue(key, out var dsQuyDoi) && dsQuyDoi != null)
            {
                // Tìm mốc điểm tối thiểu cao nhất mà điểm thí sinh đạt hoặc vượt qua (score >= DiemNN)
                var rule = dsQuyDoi.Where(q => diemNN >= q.DiemNN)
                               .OrderByDescending(q => q.DiemNN)
                               .FirstOrDefault();

                if (rule != null)
                {
                    return rule.DiemQuyDoi;
                }
            }

            return null;
        }
    }
}
