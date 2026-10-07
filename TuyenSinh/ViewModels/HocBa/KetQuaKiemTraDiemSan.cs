using System.Collections.Generic;

namespace TuyenSinh.ViewModels.HocBa
{
    public class KetQuaKiemTraDiemSan
    {
        public bool ThanhCong { get; set; }
        public string? ThongBao { get; set; }
        public int TongSoThiSinh { get; set; }
        public int SoThiSinhDat { get; set; }
        public int SoThiSinhKhongDat { get; set; }
        public List<BaoCaoKiemTraDiemSanItem> DanhSachKiemTraDiemSan { get; set; } = new();
    }
}
