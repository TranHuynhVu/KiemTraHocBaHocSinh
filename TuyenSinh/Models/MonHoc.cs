using System.Collections.Generic;

namespace TuyenSinh.Models
{
    public class MonHoc
    {
        public int Id { get; set; }
        public string TenMonHoc { get; set; } = null!;
        public string FieldName { get; set; } = null!; 

        public virtual ICollection<ToHopMon> ToHopMons { get; set; } = new List<ToHopMon>();
    }
}
