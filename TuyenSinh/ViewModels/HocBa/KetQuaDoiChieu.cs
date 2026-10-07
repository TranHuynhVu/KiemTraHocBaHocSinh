using System.Collections.Generic;

namespace TuyenSinh.ViewModels.HocBa
{
    public class KetQuaDoiChieu
    {
        public bool ThanhCong { get; set; }
        public string? ThongBao { get; set; }
        public int TongNguyenVong { get; set; }
        public int TongLoiKhongTimThayNganh { get; set; }
        public List<string> DanhSachMaNganhKhongTim { get; set; } = new();
        public List<KetQuaDoiChieuItem> DanhSachThieuDiem { get; set; } = new();

        public ThongKeTongHopViewModel ThongKeTongHop { get; set; } = new();
        public List<ThongKeTheoNganhItemViewModel> ThongKeTheoNganh { get; set; } = new();
    }
}
