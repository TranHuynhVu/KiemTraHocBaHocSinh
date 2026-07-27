using System.Collections.Generic;

namespace TuyenSinh.Models
{
    public class Nganh
    {
        public int Id { get; set; }
        public string MaNganh { get; set; } = null!;
        public string TenNganh { get; set; } = null!;
        public float HeSoTHPT { get; set; }
        public float HeSoHB { get; set; }
        public string ToHopXetTuyen { get; set; }
        public string NguongDauVao { get; set; }
        public decimal DXT { get; set; }
        public decimal DiemSanToan { get; set; }

        public virtual ICollection<ToHopNganh> ToHopNganhs { get; set; } = new List<ToHopNganh>();
    }
}
