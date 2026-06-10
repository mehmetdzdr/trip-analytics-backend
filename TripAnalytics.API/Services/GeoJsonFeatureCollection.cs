using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripAnalytics.API.Services
{
    public class GeoJsonFeatureCollection
    {
        [JsonPropertyName("features")]
        public List<GeoJsonFeature> Features { get; set; } = new();

        public class GeoJsonFeature
        {
            [JsonPropertyName("properties")]
            public GeoJsonProperties Properties { get; set; } = null!;

            [JsonPropertyName("geometry")] 
            public JsonElement Geometry { get; set; }
        }

        public class GeoJsonProperties
        {
            [JsonPropertyName("postalCode")]
            public string PostalCode { get; set; } = null!;
            [JsonPropertyName("borough")]
            public string Borough { get; set; } = null!;
            [JsonPropertyName("PO_NAME")]
            public string PO_NAME { get; set; } = null!;
            [JsonPropertyName("Shape_Area")]
            public double Shape_Area { get; set; }

        }
    }
}
