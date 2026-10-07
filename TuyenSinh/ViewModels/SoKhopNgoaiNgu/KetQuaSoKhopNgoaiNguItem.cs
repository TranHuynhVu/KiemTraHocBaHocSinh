namespace TuyenSinh.ViewModels.SoKhopNgoaiNgu
{
    public class KetQuaSoKhopNgoaiNguItem
    {
        public int Stt { get; set; }
        public string SoBaoDanh { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinh { get; set; } = string.Empty;
        public string Ddcn { get; set; } = string.Empty;
        public string ChungChiNgoaiNgu { get; set; } = string.Empty;
        public decimal DiemNN { get; set; }
        public string MaXetTuyen { get; set; } = string.Empty;
        public decimal? DiemQuyDoi { get; set; }
    }
}
