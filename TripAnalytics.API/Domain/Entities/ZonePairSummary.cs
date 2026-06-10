namespace TripAnalytics.API.Domain.Entities
{
    public class ZonePairSummary
    {
        public string PickupZip { get; set; } = null!;
        public string DropoffZip { get; set; } = null!;
        public int TripCount { get; set; }
        public double AvgDuration { get; set; } //mins
    }
}
