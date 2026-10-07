using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface IQuyDoiNNService
    {
        // Quản lý Bậc Ngoại Ngữ
        Task<List<BacNgoaiNgu>> LayDanhSachBacAsync();
        Task<BacNgoaiNgu?> LayBacTheoIdAsync(int id);
        Task<ServiceResult> ThemBacAsync(string tenBac, string tenVietTat);
        Task<ServiceResult> SuaBacAsync(int id, string tenBac, string tenVietTat);
        Task<ServiceResult> XoaBacAsync(int id);

        // Quản lý Loại Ngoại Ngữ
        Task<List<LoaiNgoaiNgu>> LayDanhSachLoaiAsync();
        Task<LoaiNgoaiNgu?> LayLoaiTheoIdAsync(int id);
        Task<ServiceResult> ThemLoaiAsync(string tenLoai);
        Task<ServiceResult> SuaLoaiAsync(int id, string tenLoai);
        Task<ServiceResult> XoaLoaiAsync(int id);

        // Quản lý Điểm Quy Đổi Ngoại Ngữ
        Task<List<QuyDoiNN>> LayDanhSachQuyDoiAsync();
        Task<QuyDoiNN?> LayQuyDoiTheoIdAsync(int id);
        Task<ServiceResult> ThemQuyDoiAsync(int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi);
        Task<ServiceResult> SuaQuyDoiAsync(int id, int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi);
        Task<ServiceResult> XoaQuyDoiAsync(int id);

        // Tra cứu & Lấy điểm quy đổi ngoại ngữ
        Task<Dictionary<string, List<QuyDoiNN>>> DanhSachDiemQuyDoiAsync();
        decimal? LayDiemQuyDoiNNTheoTenLoai(Dictionary<string, List<QuyDoiNN>> danhSachQuyDoi, string tenLoai, decimal diemNN);
    }
}
