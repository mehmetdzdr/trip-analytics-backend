namespace TripAnalytics.API.Domain.Entities
{
    public class ZipZone
    {
        public string PostalCode { get; set; } = null!;
        public string Borough { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double AreaKm2 { get; set; }
    }
}
