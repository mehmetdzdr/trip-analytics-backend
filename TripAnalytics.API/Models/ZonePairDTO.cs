namespace TripAnalytics.API.Models
{
    public class ZonePairDTO
    {
        public string PickupZip { get; set; } = null!;
        public string DropoffZip { get; set; } = null!;
        public int TripCount { get; set; }
        public double AvgDuration { get; set; }
    }
}
