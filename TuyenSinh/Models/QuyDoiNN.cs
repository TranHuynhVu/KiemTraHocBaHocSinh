namespace TuyenSinh.Models
{
    public class QuyDoiNN
    {
        public int Id { get; set; }
        public int BacNgoaiNguId { get; set; }
        public int LoaiNgoaiNguId { get; set; }
        public decimal DiemNN { get; set; } // Điểm mốc từ / Mức điểm cố định
        public decimal? DiemNNDen { get; set; } // Điểm mốc đến (để trống nếu là 1 mức điểm cố định)
        public decimal DiemQuyDoi { get; set; }
        public BacNgoaiNgu BacNgoaiNgu { get; set; } = null!;
        public LoaiNgoaiNgu LoaiNgoaiNgu { get; set; } = null!;
    }
}
