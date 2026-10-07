using System.Collections.Generic;

namespace TuyenSinh.ViewModels.HocBa
{
    public class ThongKeTheoNganhItemViewModel
    {
        public string MaXetTuyen { get; set; } = string.Empty;
        public string TenNganh { get; set; } = string.Empty;
        public int TongNV { get; set; }
        public int SoThiSinh { get; set; }
        public int NVCoToHopDu { get; set; }
        public int NVThieuMoiToHop { get; set; }
        public int NVKhongDiemCN { get; set; }
        public int NVKhongHocBa { get; set; }
        public int NVBoQua { get; set; }
        public double TyLeThieu { get; set; }

        // Danh sách chi tiết cho 3 loại lỗi (để hiển thị modal & xuất Excel theo ngành)
        public List<ChiTietNguyenVongLoiItem> DanhSachThieuMoiToHop { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachKhongHocBa { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachKhongDiemCN { get; set; } = new();
    }
}
