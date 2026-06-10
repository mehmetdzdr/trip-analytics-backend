namespace TripAnalytics.API.Models
{
    public class ZoneSummaryDTO
    {
        public string PostalCode { get; set; } = null!;
        public string Borough { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int PickupCount { get; set; }
        public int DropoffCount { get; set; }
        public double DensityPerKm2 { get; set; }
        public int[] PickupsByHour { get; set; } = null!;  
        public int[] DropoffsByHour { get; set; } = null!;  
    }
}
