using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IQuyDoiNNService
    {
        // Quản lý Bậc Ngoại Ngữ
        Task<List<BacNgoaiNgu>> LayDanhSachBacAsync();
        Task<BacNgoaiNgu?> LayBacTheoIdAsync(int id);
        Task<(bool Success, string Message)> ThemBacAsync(string tenBac, string tenVietTat);
        Task<(bool Success, string Message)> SuaBacAsync(int id, string tenBac, string tenVietTat);
        Task<(bool Success, string Message)> XoaBacAsync(int id);

        // Quản lý Loại Ngoại Ngữ
        Task<List<LoaiNgoaiNgu>> LayDanhSachLoaiAsync();
        Task<LoaiNgoaiNgu?> LayLoaiTheoIdAsync(int id);
        Task<(bool Success, string Message)> ThemLoaiAsync(string tenLoai);
        Task<(bool Success, string Message)> SuaLoaiAsync(int id, string tenLoai);
        Task<(bool Success, string Message)> XoaLoaiAsync(int id);

        // Quản lý Điểm Quy Đổi Ngoại Ngữ
        Task<List<QuyDoiNN>> LayDanhSachQuyDoiAsync();
        Task<QuyDoiNN?> LayQuyDoiTheoIdAsync(int id);
        Task<(bool Success, string Message)> ThemQuyDoiAsync(int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi);
        Task<(bool Success, string Message)> SuaQuyDoiAsync(int id, int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi);
        Task<(bool Success, string Message)> XoaQuyDoiAsync(int id);

        // Tra cứu & Lấy điểm quy đổi ngoại ngữ
        Task<Dictionary<string, List<QuyDoiNN>>> DanhSachDiemQuyDoiAsync();
        decimal? LayDiemQuyDoiNNTheoTenLoai(Dictionary<string, List<QuyDoiNN>> danhSachQuyDoi, string tenLoai, decimal diemNN);
    }
}
