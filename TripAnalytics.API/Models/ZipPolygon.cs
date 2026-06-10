using NetTopologySuite.Geometries;

namespace TripAnalytics.API.Models
{
    public class ZipPolygon
    {
        public string PostalCode { get; set; } = null!;
        public Geometry Geometry { get; set; } = null!;
        public Envelope BoundingBox { get; set; } = null!;
    }
}
