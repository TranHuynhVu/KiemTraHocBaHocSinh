namespace TuyenSinh.Models
{
    public class QuyDoiNN
    {
        public int Id { get; set; }
        public int BacNgoaiNguId { get; set; }
        public int LoaiNgoaiNguId { get; set; }
        public decimal DiemNN { get; set; } // Điểm mốc tối thiểu
        public decimal DiemQuyDoi { get; set; }
        public BacNgoaiNgu BacNgoaiNgu { get; set; } = null!;
        public LoaiNgoaiNgu LoaiNgoaiNgu { get; set; } = null!;
    }
}
