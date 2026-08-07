namespace TuyenSinh.Models
{
    public class DiemCong
    {
        public int Id { get; set; }
        public string DDCN { get; set; } //  Định danh cá nhân
        public string HoTen { get; set; }
        public DateTime DOB { get; set; }
        public string MaXetTuyen { get; set; } // Mã xét tuyển
        public string MaPTXT { get; set; } // Mã phương thức xét tuyển
        public string MaToHop { get; set; }
        public int LoaiDiemCong { get; set; }
        public decimal Diem { get; set; }
        public int NamHoc { get; set; }
    }
}
