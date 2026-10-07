using System.Collections.Generic;

namespace TuyenSinh.ViewModels.HocBa
{
    public class KetQuaKiemTraHocBa
    {
        public bool ThanhCong { get; set; }
        public string? ThongBao { get; set; }
        public List<BaoCaoThieuNamHocItem> DanhSachThieuNamHoc { get; set; } = new();
        public List<BaoCaoThieuDiemItem> DanhSachThieuDiem { get; set; } = new();
    }
}
