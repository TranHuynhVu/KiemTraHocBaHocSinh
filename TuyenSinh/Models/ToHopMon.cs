using System.Collections.Generic;

namespace TuyenSinh.Models
{
    public class ToHopMon
    {
        public int Id { get; set; }
        public string MaToHop { get; set; } = null!; // e.g. A00, A01
        public string TenToHop { get; set; } = null!; // e.g. Toán, Vật lý, Hóa học

        // Many-to-many relationship
        public virtual ICollection<MonHoc> MonHocs { get; set; } = new List<MonHoc>();
    }
}
