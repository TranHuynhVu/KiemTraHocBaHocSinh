using System;
using System.Collections.Generic;

namespace TuyenSinh.ViewModels
{
    public class KetQuaSoKhopNgoaiNguItem
    {
        public int Stt { get; set; }
        public string SoBaoDanh { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string NgaySinh { get; set; } = string.Empty;
        public string Ddcn { get; set; } = string.Empty;
        public string ChungChiNgoaiNgu { get; set; } = string.Empty;
        public string DiemBacChungChi { get; set; } = string.Empty;
        public string MaXetTuyen { get; set; } = string.Empty;
        public string MatchStatus { get; set; } = "Khớp ĐDCN";
    }

    public class SoKhopNgoaiNguThongKeViewModel
    {
        public int TongHopLeNN { get; set; }
        public int TongDanhSachThiSinh { get; set; }
        public int TongNguyenVong { get; set; }
        public int TongSoKhop { get; set; }
        public List<KetQuaSoKhopNgoaiNguItem> DanhSachKetQua { get; set; } = new List<KetQuaSoKhopNgoaiNguItem>();
    }
}
