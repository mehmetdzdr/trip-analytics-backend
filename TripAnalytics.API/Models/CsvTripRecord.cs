using CsvHelper.Configuration.Attributes;

namespace TripAnalytics.API.Models
{
    public class CsvTripRecord
    {
        [Name("tpep_pickup_datetime")]
        public DateTime PickupDatetime { get; set; }

        [Name("tpep_dropoff_datetime")]
        public DateTime DropoffDatetime { get; set; }

        [Name("pickup_longitude")]
        public double PickupLongitude { get; set; }

        [Name("pickup_latitude")]
        public double PickupLatitude { get; set; }

        [Name("dropoff_longitude")]
        public double DropoffLongitude { get; set; }

        [Name("dropoff_latitude")]
        public double DropoffLatitude { get; set; }

        [Name("fare_amount")]
        public double FareAmount { get; set; }

        [Name("trip_distance")]
        public double TripDistance { get; set; }
    }
}
