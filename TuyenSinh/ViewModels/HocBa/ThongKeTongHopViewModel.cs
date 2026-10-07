using System.Collections.Generic;

namespace TuyenSinh.ViewModels.HocBa
{
    public class ThongKeTongHopViewModel
    {
        public int TongDongNguyenVong { get; set; }
        public int TongThiSinhDuyNhat { get; set; }
        public int TongThiSinhBiAnhHuong { get; set; }
        public int TongNganhBiAnhHuong { get; set; }
        public int NguyenVongCoToHopDu { get; set; }
        public int NguyenVongThieuMoiToHop { get; set; }
        public int NguyenVongKhongHocBa { get; set; }
        public int NguyenVongKhongDiemCN { get; set; }
        public int NguyenVongBoQua { get; set; }

        public List<ChiTietNguyenVongLoiItem> DanhSachThieuMoiToHop { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachKhongHocBa { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachKhongDiemCN { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachBoQua { get; set; } = new();

        public List<ThiSinhBiAnhHuongItem> DanhSachThiSinhBiAnhHuong { get; set; } = new();
        public List<NganhBiAnhHuongItem> DanhSachNganhBiAnhHuong { get; set; } = new();
    }
}
