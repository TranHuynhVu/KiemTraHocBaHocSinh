using System;

namespace TuyenSinh.ViewModels
{
    public class HocBaTHPTImport
    {
        // Thông tin chung
        public int? STT { get; set; }
        public string? SoDDCN { get; set; }
        public string? HoVaTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public int? Lop { get; set; }
        public int? ChuongTrinhHoc { get; set; }

        // Điểm trung bình
        public decimal? DiemTrungBinhNam { get; set; }
        public decimal? DiemTongKetHKI { get; set; }
        public decimal? DiemTongKetHKII { get; set; }
        public decimal? DiemTongKetCN { get; set; }

        // Học lực
        public string? HocLucHKI { get; set; }
        public string? HocLucHKII { get; set; }
        public string? HocLucCN { get; set; }

        // Hạnh kiểm
        public string? HanhKiemHKI { get; set; }
        public string? HanhKiemHKII { get; set; }
        public string? HanhKiemCN { get; set; }

        // Kết quả học tập
        public string? KetQuaHocTapHKI { get; set; }
        public string? KetQuaHocTapHKII { get; set; }
        public string? KetQuaHocTapCN { get; set; }

        // Kết quả rèn luyện
        public string? KetQuaRenLuyenHKI { get; set; }
        public string? KetQuaRenLuyenHKII { get; set; }
        public string? KetQuaRenLuyenCN { get; set; }

        // Toán
        public decimal? ToanHKI { get; set; }
        public decimal? ToanHKII { get; set; }
        public decimal? ToanCN { get; set; }

        // Ngữ văn
        public decimal? VanHKI { get; set; }
        public decimal? VanHKII { get; set; }
        public decimal? VanCN { get; set; }

        // Vật lý
        public decimal? VatLyHKI { get; set; }
        public decimal? VatLyHKII { get; set; }
        public decimal? VatLyCN { get; set; }

        // Hóa học
        public decimal? HoaHocHKI { get; set; }
        public decimal? HoaHocHKII { get; set; }
        public decimal? HoaHocCN { get; set; }

        // Sinh học
        public decimal? SinhHocHKI { get; set; }
        public decimal? SinhHocHKII { get; set; }
        public decimal? SinhHocCN { get; set; }

        // Lịch sử
        public decimal? LichSuHKI { get; set; }
        public decimal? LichSuHKII { get; set; }
        public decimal? LichSuCN { get; set; }

        // Địa lý
        public decimal? DiaLyHKI { get; set; }
        public decimal? DiaLyHKII { get; set; }
        public decimal? DiaLyCN { get; set; }

        // GDCD
        public decimal? GDCDHKI { get; set; }
        public decimal? GDCDHKII { get; set; }
        public decimal? GDCDCN { get; set; }

        // KTPL
        public decimal? KTPLHKI { get; set; }
        public decimal? KTPLHKII { get; set; }
        public decimal? KTPLCN { get; set; }

        // Tin học
        public decimal? TinHocHKI { get; set; }
        public decimal? TinHocHKII { get; set; }
        public decimal? TinHocCN { get; set; }

        // Công nghệ Công nghiệp
        public decimal? CNCNHKI { get; set; }
        public decimal? CNCNHKII { get; set; }
        public decimal? CNCNCN { get; set; }

        // Công nghệ Nông nghiệp
        public decimal? CNNNHKI { get; set; }
        public decimal? CNNNHKII { get; set; }
        public decimal? CNNNCN { get; set; }

        // Ngoại ngữ
        public decimal? NgoaiNguHKI { get; set; }
        public decimal? NgoaiNguHKII { get; set; }
        public decimal? NgoaiNguCN { get; set; }
        public string? MonNgoaiNgu { get; set; }

        // Tự chọn song ngữ
        public decimal? TuChonSongNguHKI { get; set; }
        public decimal? TuChonSongNguHKII { get; set; }
        public decimal? TuChonSongNguCN { get; set; }

        // Quốc phòng - An ninh
        public decimal? QPANHKI { get; set; }
        public decimal? QPANHKII { get; set; }
        public decimal? QPANCN { get; set; }

        // Tiếng dân tộc
        public decimal? TiengDanTocHKI { get; set; }
        public decimal? TiengDanTocHKII { get; set; }
        public decimal? TiengDanTocCN { get; set; }

        // Ngoại ngữ 2
        public decimal? NgoaiNgu2HKI { get; set; }
        public decimal? NgoaiNgu2HKII { get; set; }
        public decimal? NgoaiNgu2CN { get; set; }
        public string? MonNgoaiNgu2 { get; set; }

        // Toán Pháp
        public decimal? ToanPhapHKI { get; set; }
        public decimal? ToanPhapHKII { get; set; }
        public decimal? ToanPhapCN { get; set; }
    }
}
