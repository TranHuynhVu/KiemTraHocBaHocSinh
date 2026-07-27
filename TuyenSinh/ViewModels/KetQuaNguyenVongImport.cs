namespace TuyenSinh.ViewModels
{
    public class KetQuaNguyenVongImport
    {
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public DateTime NgaySinh { get; set; }
        public int NamTN { get; set; }
        public string? DTUT { get; set; }
        public string? KVUT { get; set; } 
        public string? HocLuc { get; set; }
        public string? DiemXetTN { get; set; }
        public int? ThuTuNV { get; set; }
        public string? MaTruong { get; set; }
        public string? MaNganh { get; set; }
        public string? PTXT { get; set; } // Phương thức xét tuyển
        public string? ToHop { get; set; } // Tổ hợp xét tuyển

        public string? Mon1 { get; set; } // Môn 1
        public decimal? TrongSoMon1 { get; set; } // Trọng số môn 1
        public decimal? DiemMon1HB { get; set; } // Điểm môn 1 học bạ
        public decimal? DiemMon1THPT { get; set; } // Điểm môn 1 thi THPT

        public string? Mon2 { get; set; } // Môn 2
        public decimal? TrongSoMon2 { get; set; } // Trọng số môn 2
        public decimal? DiemMon2HB { get; set; } // Điểm môn 2 học bạ
        public decimal? DiemMon2THPT { get; set; } // Điểm môn 2 thi THPT

        public string? Mon3 { get; set; } // Môn 3
        public decimal? TrongSoMon3 { get; set; } // Trọng số môn 3
        public decimal? DiemMon3HB { get; set; } // Điểm môn 3 học bạ
        public decimal? DiemMon3THPT { get; set; } // Điểm môn 3 thi THPT

        public decimal? TrongSoHB { get; set; } // Trọng số học bạ
        public decimal? TrongSoTHPT { get; set; } // Trọng số thi THPT
        public decimal? DiemCong { get; set; } // Điểm cộng
        public decimal? DiemUuTien { get; set; } // Điểm ưu tiên
        public decimal? DS { get; set; }
        public decimal? TDHB { get; set; } // Tổng điểm học bạ
        public decimal? TDTHPT { get; set; } // Tổng điểm thi THPT
        public decimal? TD { get; set; } // Tổng điểm
        public decimal? DiemXetTuyen { get; set; } // Điểm xét tuyển
        public bool KQKiemTraNguong { get; set; } // Kết quả kiểm tra ngưỡng
        public string? GhiChu { get; set; } // Ghi chú
    }
}
