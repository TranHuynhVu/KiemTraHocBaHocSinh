namespace TuyenSinh.Models
{
    public class ToHopNganh
    {
        public int Id { get; set; }
        public int MaNganhId { get; set; }
        public int ToHopId { get; set; }

        public virtual Nganh Nganh { get; set; } = null!;
        public virtual ToHopMon ToHopMon { get; set; } = null!;
    }
}
