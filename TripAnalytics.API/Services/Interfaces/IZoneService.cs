using TripAnalytics.API.Models;

namespace TripAnalytics.API.Services.Interfaces
{
    public interface IZoneService
    {
        Task<List<ZoneSummaryDTO>> GetAllAsync();
        Task<ZoneDetailDTO?> GetByPostalCodeAsync(string postalCode);
        Task<ZonePairDTO?> GetPairAsync(string from, string to);
    }
}
