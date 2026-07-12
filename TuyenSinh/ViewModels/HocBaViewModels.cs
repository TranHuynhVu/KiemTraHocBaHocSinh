using System.Collections.Generic;

namespace TuyenSinh.ViewModels
{
    public class BaoCaoThieuNamHocItem
    {
        public int Stt { get; set; }
        public string? Cccd { get; set; }
        public string? HoVaTen { get; set; }
        public string? NamHienCo { get; set; }
        public string? NamThieu { get; set; }
    }

    public class BaoCaoThieuDiemItem
    {
        public int Stt { get; set; }
        public string? Cccd { get; set; }
        public string? HoVaTen { get; set; }
        public string? NamLoi { get; set; }
        public string? ToHop { get; set; }
        public string? MonThieu { get; set; }
    }

    public class HocBaPreviewItem
    {
        public int? Stt { get; set; }
        public string? SoDDCN { get; set; }
        public string? HoVaTen { get; set; }
        public string? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public int? Lop { get; set; }
        public int? ChuongTrinhHoc { get; set; }
        public decimal? DiemTrungBinhNam { get; set; }
        public decimal? ToanCN { get; set; }
        public decimal? VanCN { get; set; }
        public decimal? VatLyCN { get; set; }
        public decimal? HoaHocCN { get; set; }
        public decimal? SinhHocCN { get; set; }
        public decimal? NgoaiNguCN { get; set; }
    }

    public class KetQuaKiemTraHocBa
    {
        public bool ThanhCong { get; set; }
        public string? ThongBao { get; set; }
        public List<BaoCaoThieuNamHocItem> DanhSachThieuNamHoc { get; set; } = new();
        public List<BaoCaoThieuDiemItem> DanhSachThieuDiem { get; set; } = new();
    }
}
