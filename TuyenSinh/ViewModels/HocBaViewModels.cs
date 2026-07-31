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

    // === Chức năng 2: Đối chiếu Học bạ & Nguyện vọng ===

    public class NguyenVongItem
    {
        public string? SoDDCN { get; set; }
        public int ThuTuNV { get; set; }
        public string? MaXetTuyen { get; set; }
        public string? TenNganh { get; set; }
    }

    public class KetQuaDoiChieuItem
    {
        public int Stt { get; set; }
        public string? SoDDCN { get; set; }
        public string? HoVaTen { get; set; }
        public int ThuTuNV { get; set; }
        public string? MaNganh { get; set; }
        public string? TenNganh { get; set; }
        public string? MaToHop { get; set; }
        public string? NamHoc { get; set; }
        public string? MonThieu { get; set; }
    }

    public class ChiTietNguyenVongLoiItem
    {
        public string Cccd { get; set; } = string.Empty;
        public int ThuTuNV { get; set; }
        public string MaXetTuyen { get; set; } = string.Empty;
        public string TenNganh { get; set; } = string.Empty;
    }

    public class ThongKeTongHopViewModel
    {
        public int TongDongNguyenVong { get; set; }
        public int TongThiSinhDuyNhat { get; set; }
        public int NguyenVongCoToHopDu { get; set; }
        public int NguyenVongThieuMoiToHop { get; set; }
        public int NguyenVongKhongHocBa { get; set; }
        public int NguyenVongKhongDiemCN { get; set; }
        public int NguyenVongBoQua { get; set; }

        public List<ChiTietNguyenVongLoiItem> DanhSachThieuMoiToHop { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachKhongHocBa { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachKhongDiemCN { get; set; } = new();
        public List<ChiTietNguyenVongLoiItem> DanhSachBoQua { get; set; } = new();
    }

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
    }

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

    public class BaoCaoKiemTraDiemSanItem
    {
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public string MaNganh { get; set; }
        public string ToHop { get; set; }
        public decimal DiemXetTuyen { get; set; }
        public decimal DiemSan { get; set; }
        public decimal DiemSanToan { get; set; }
        public string GhiChu { get; set; }
    }
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
