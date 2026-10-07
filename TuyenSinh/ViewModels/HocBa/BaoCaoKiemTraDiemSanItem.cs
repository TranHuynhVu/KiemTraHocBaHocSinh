namespace TuyenSinh.ViewModels.HocBa
{
    public class BaoCaoKiemTraDiemSanItem
    {
        public string HoTen { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public string MaNganh { get; set; } = string.Empty;
        public string ToHop { get; set; } = string.Empty;
        public decimal DiemXetTuyen { get; set; }
        public decimal DiemSan { get; set; }
        public decimal DiemSanToan { get; set; }
        public string GhiChu { get; set; } = string.Empty;
    }
}
