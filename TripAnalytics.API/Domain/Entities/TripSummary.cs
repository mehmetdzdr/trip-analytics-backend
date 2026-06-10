namespace TripAnalytics.API.Domain.Entities
{
    public class TripSummary
    {
        public string PostalCode { get; set; } = null!;
        public int PickupCount { get; set; }
        public int DropoffCount { get; set; }
        public double AvgFare { get; set; }
        public double AvgDistance { get; set; }
        public double DensityPerKm2 { get; set; }
        public int[] PickupsByHour { get; set; } = new int[24];
        public int[] DropoffsByHour { get; set; } = new int[24];
        public ZipZone ZipZone { get; set; } = null!;
    }
}
