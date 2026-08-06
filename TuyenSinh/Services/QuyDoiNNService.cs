using Microsoft.EntityFrameworkCore;
using TuyenSinh.Data;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public class QuyDoiNNService : IQuyDoiNNService
    {
        private readonly ApplicationDbContext _context;

        public QuyDoiNNService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<BacNgoaiNgu>> LayDanhSachBacAsync()
        {
            return await _context.BacNgoaiNgus.AsNoTracking().ToListAsync();
        }

        public async Task<BacNgoaiNgu?> LayBacTheoIdAsync(int id)
        {
            return await _context.BacNgoaiNgus.FindAsync(id);
        }

        public async Task<(bool Success, string Message)> ThemBacAsync(string tenBac, string tenVietTat)
        {
            if (string.IsNullOrWhiteSpace(tenBac) || string.IsNullOrWhiteSpace(tenVietTat))
            {
                return (false, "Tên bậc và tên viết tắt không được để trống.");
            }

            tenBac = tenBac.Trim();
            tenVietTat = tenVietTat.Trim();

            var exists = await _context.BacNgoaiNgus.AnyAsync(x => x.TenBac.ToLower() == tenBac.ToLower() || x.TenVietTat.ToLower() == tenVietTat.ToLower());
            if (exists)
            {
                return (false, "Bậc ngoại ngữ hoặc tên viết tắt này đã tồn tại.");
            }

            var bac = new BacNgoaiNgu
            {
                TenBac = tenBac,
                TenVietTat = tenVietTat
            };

            _context.BacNgoaiNgus.Add(bac);
            await _context.SaveChangesAsync();
            return (true, "Thêm bậc ngoại ngữ thành công.");
        }

        public async Task<(bool Success, string Message)> SuaBacAsync(int id, string tenBac, string tenVietTat)
        {
            if (string.IsNullOrWhiteSpace(tenBac) || string.IsNullOrWhiteSpace(tenVietTat))
            {
                return (false, "Tên bậc và tên viết tắt không được để trống.");
            }

            var bac = await _context.BacNgoaiNgus.FindAsync(id);
            if (bac == null)
            {
                return (false, "Không tìm thấy bậc ngoại ngữ cần sửa.");
            }

            tenBac = tenBac.Trim();
            tenVietTat = tenVietTat.Trim();

            var exists = await _context.BacNgoaiNgus.AnyAsync(x => x.Id != id && (x.TenBac.ToLower() == tenBac.ToLower() || x.TenVietTat.ToLower() == tenVietTat.ToLower()));
            if (exists)
            {
                return (false, "Tên bậc hoặc tên viết tắt trùng lặp với bậc ngoại ngữ khác.");
            }

            bac.TenBac = tenBac;
            bac.TenVietTat = tenVietTat;

            _context.BacNgoaiNgus.Update(bac);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật bậc ngoại ngữ thành công.");
        }

        public async Task<(bool Success, string Message)> XoaBacAsync(int id)
        {
            var bac = await _context.BacNgoaiNgus.FindAsync(id);
            if (bac == null)
            {
                return (false, "Không tìm thấy bậc ngoại ngữ cần xóa.");
            }

            var dangDung = await _context.QuyDoiNNs.AnyAsync(q => q.BacNgoaiNguId == id);
            if (dangDung)
            {
                return (false, "Không thể xóa bậc ngoại ngữ này vì đang được sử dụng trong bảng quy đổi điểm.");
            }

            _context.BacNgoaiNgus.Remove(bac);
            await _context.SaveChangesAsync();
            return (true, "Xóa bậc ngoại ngữ thành công.");
        }

        public async Task<List<LoaiNgoaiNgu>> LayDanhSachLoaiAsync()
        {
            return await _context.LoaiNgoaiNgus.AsNoTracking().ToListAsync();
        }

        public async Task<LoaiNgoaiNgu?> LayLoaiTheoIdAsync(int id)
        {
            return await _context.LoaiNgoaiNgus.FindAsync(id);
        }

        public async Task<(bool Success, string Message)> ThemLoaiAsync(string tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai))
            {
                return (false, "Tên loại ngoại ngữ không được để trống.");
            }

            tenLoai = tenLoai.Trim();
            var exists = await _context.LoaiNgoaiNgus.AnyAsync(x => x.TenLoai.ToLower() == tenLoai.ToLower());
            if (exists)
            {
                return (false, "Loại ngoại ngữ này đã tồn tại.");
            }

            var loai = new LoaiNgoaiNgu
            {
                TenLoai = tenLoai
            };

            _context.LoaiNgoaiNgus.Add(loai);
            await _context.SaveChangesAsync();
            return (true, "Thêm loại ngoại ngữ thành công.");
        }

        public async Task<(bool Success, string Message)> SuaLoaiAsync(int id, string tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai))
            {
                return (false, "Tên loại ngoại ngữ không được để trống.");
            }

            var loai = await _context.LoaiNgoaiNgus.FindAsync(id);
            if (loai == null)
            {
                return (false, "Không tìm thấy loại ngoại ngữ cần sửa.");
            }

            tenLoai = tenLoai.Trim();
            var exists = await _context.LoaiNgoaiNgus.AnyAsync(x => x.Id != id && x.TenLoai.ToLower() == tenLoai.ToLower());
            if (exists)
            {
                return (false, "Tên loại ngoại ngữ trùng lặp với loại khác.");
            }

            loai.TenLoai = tenLoai;
            _context.LoaiNgoaiNgus.Update(loai);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật loại ngoại ngữ thành công.");
        }

        public async Task<(bool Success, string Message)> XoaLoaiAsync(int id)
        {
            var loai = await _context.LoaiNgoaiNgus.FindAsync(id);
            if (loai == null)
            {
                return (false, "Không tìm thấy loại ngoại ngữ cần xóa.");
            }

            var dangDung = await _context.QuyDoiNNs.AnyAsync(q => q.LoaiNgoaiNguId == id);
            if (dangDung)
            {
                return (false, "Không thể xóa loại ngoại ngữ này vì đang được sử dụng trong bảng quy đổi điểm.");
            }

            _context.LoaiNgoaiNgus.Remove(loai);
            await _context.SaveChangesAsync();
            return (true, "Xóa loại ngoại ngữ thành công.");
        }

        public async Task<List<QuyDoiNN>> LayDanhSachQuyDoiAsync()
        {
            return await _context.QuyDoiNNs
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
            return await _context.QuyDoiNNs
                .Include(q => q.BacNgoaiNgu)
                .Include(q => q.LoaiNgoaiNgu)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<(bool Success, string Message)> ThemQuyDoiAsync(int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi)
        {
            var bacExists = await _context.BacNgoaiNgus.AnyAsync(b => b.Id == bacNgoaiNguId);
            if (!bacExists) return (false, "Bậc ngoại ngữ được chọn không hợp lệ.");

            var loaiExists = await _context.LoaiNgoaiNgus.AnyAsync(l => l.Id == loaiNgoaiNguId);
            if (!loaiExists) return (false, "Loại ngoại ngữ được chọn không hợp lệ.");

            if (diemNN < 0 || diemQuyDoi < 0 || diemQuyDoi > 10)
            {
                return (false, "Điểm ngoại ngữ và điểm quy đổi không hợp lệ (Điểm quy đổi từ 0 đến 10).");
            }

            var duplicate = await _context.QuyDoiNNs.AnyAsync(q => q.BacNgoaiNguId == bacNgoaiNguId && q.LoaiNgoaiNguId == loaiNgoaiNguId && q.DiemNN == diemNN);
            if (duplicate)
            {
                return (false, "Quy tắc quy đổi cho Bậc, Loại và mốc điểm này đã tồn tại.");
            }

            var quyDoi = new QuyDoiNN
            {
                BacNgoaiNguId = bacNgoaiNguId,
                LoaiNgoaiNguId = loaiNgoaiNguId,
                DiemNN = diemNN,
                DiemQuyDoi = diemQuyDoi
            };

            _context.QuyDoiNNs.Add(quyDoi);
            await _context.SaveChangesAsync();
            return (true, "Thêm quy tắc quy đổi điểm thành công.");
        }

        public async Task<(bool Success, string Message)> SuaQuyDoiAsync(int id, int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi)
        {
            var quyDoi = await _context.QuyDoiNNs.FindAsync(id);
            if (quyDoi == null)
            {
                return (false, "Không tìm thấy quy tắc quy đổi điểm cần sửa.");
            }

            var bacExists = await _context.BacNgoaiNgus.AnyAsync(b => b.Id == bacNgoaiNguId);
            if (!bacExists) return (false, "Bậc ngoại ngữ được chọn không hợp lệ.");

            var loaiExists = await _context.LoaiNgoaiNgus.AnyAsync(l => l.Id == loaiNgoaiNguId);
            if (!loaiExists) return (false, "Loại ngoại ngữ được chọn không hợp lệ.");

            if (diemNN < 0 || diemQuyDoi < 0 || diemQuyDoi > 10)
            {
                return (false, "Điểm ngoại ngữ và điểm quy đổi không hợp lệ (Điểm quy đổi từ 0 đến 10).");
            }

            var duplicate = await _context.QuyDoiNNs.AnyAsync(q => q.Id != id && q.BacNgoaiNguId == bacNgoaiNguId && q.LoaiNgoaiNguId == loaiNgoaiNguId && q.DiemNN == diemNN);
            if (duplicate)
            {
                return (false, "Quy tắc quy đổi cho Bậc, Loại và mốc điểm này đã tồn tại.");
            }

            quyDoi.BacNgoaiNguId = bacNgoaiNguId;
            quyDoi.LoaiNgoaiNguId = loaiNgoaiNguId;
            quyDoi.DiemNN = diemNN;
            quyDoi.DiemQuyDoi = diemQuyDoi;

            _context.QuyDoiNNs.Update(quyDoi);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật quy tắc quy đổi điểm thành công.");
        }

        public async Task<(bool Success, string Message)> XoaQuyDoiAsync(int id)
        {
            var quyDoi = await _context.QuyDoiNNs.FindAsync(id);
            if (quyDoi == null)
            {
                return (false, "Không tìm thấy quy tắc quy đổi điểm cần xóa.");
            }

            _context.QuyDoiNNs.Remove(quyDoi);
            await _context.SaveChangesAsync();
            return (true, "Xóa quy tắc quy đổi điểm thành công.");
        }

        public async Task<Dictionary<string, List<QuyDoiNN>>> DanhSachDiemQuyDoiAsync()
        {
            var listLoaiNN = await _context.QuyDoiNNs
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
