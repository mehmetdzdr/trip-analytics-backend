using TripAnalytics.API.Domain.Entities;

namespace TripAnalytics.API.Repositories.Interfaces
{
    public interface IZoneRepository
    {
        Task<List<TripSummary>> GetAllWithZoneAsync();
        Task<TripSummary?> GetByPostalCodeAsync(string postalCode);
        Task<ZonePairSummary?> GetPairAsync(string from, string to);

    }
}
