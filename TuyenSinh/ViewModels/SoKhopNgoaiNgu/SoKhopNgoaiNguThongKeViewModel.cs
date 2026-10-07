using System.Collections.Generic;

namespace TuyenSinh.ViewModels.SoKhopNgoaiNgu
{
    public class SoKhopNgoaiNguThongKeViewModel
    {
        public int TongHopLeNN { get; set; }
        public int TongDanhSachThiSinh { get; set; }
        public int TongNguyenVong { get; set; }
        public int TongSoKhop { get; set; }
        public List<KetQuaSoKhopNgoaiNguItem> DanhSachKetQua { get; set; } = new();
    }
}
