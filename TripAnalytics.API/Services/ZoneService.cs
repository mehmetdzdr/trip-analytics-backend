using TripAnalytics.API.Models;
using TripAnalytics.API.Repositories.Interfaces;
using TripAnalytics.API.Services.Interfaces;

namespace TripAnalytics.API.Services
{
    public class ZoneService : IZoneService
    {
        private readonly IZoneRepository _repository;

        public ZoneService(IZoneRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ZoneSummaryDTO>> GetAllAsync()
        {
            var summaries = await _repository.GetAllWithZoneAsync();

            return summaries.Select(t => new ZoneSummaryDTO
            {
                PostalCode = t.PostalCode,
                Borough = t.ZipZone.Borough,
                Name = t.ZipZone.Name,
                PickupCount = t.PickupCount,
                DropoffCount = t.DropoffCount,
                DensityPerKm2 = t.DensityPerKm2,
                PickupsByHour = t.PickupsByHour,
                DropoffsByHour = t.DropoffsByHour
            }).ToList();
        }

        public async Task<ZoneDetailDTO?> GetByPostalCodeAsync(string postalCode)
        {
            var trip = await _repository.GetByPostalCodeAsync(postalCode);
            if (trip == null) return null;

            return new ZoneDetailDTO
            {
                PostalCode = trip.PostalCode,
                Borough = trip.ZipZone.Borough,
                Name = trip.ZipZone.Name,
                AreaKm2 = trip.ZipZone.AreaKm2,
                PickupCount = trip.PickupCount,
                DropoffCount = trip.DropoffCount,
                AvgFare = trip.AvgFare,
                AvgDistance = trip.AvgDistance,
                DensityPerKm2 = trip.DensityPerKm2,
                PickupsByHour = trip.PickupsByHour,
                DropoffsByHour = trip.DropoffsByHour,
                DaysInDataset = 31
            };
        }

        public async Task<ZonePairDTO?> GetPairAsync(string from, string to)
        {
            var pair = await _repository.GetPairAsync(from, to);
            if (pair == null) return null;

            return new ZonePairDTO
            {
                PickupZip = pair.PickupZip,
                DropoffZip = pair.DropoffZip,
                TripCount = pair.TripCount,
                AvgDuration = pair.AvgDuration
            };
        }

        public async Task<PagedResult<ZoneSummaryDTO>> GetPagedAsync(int page, int pageSize, string? sortBy, string? sortOrder, string? borough, string? search)
        {
            var summaries = await _repository.GetAllWithZoneAsync();

            var query = summaries.Select(t => new ZoneSummaryDTO
            {
                PostalCode = t.PostalCode,
                Borough = t.ZipZone.Borough,
                Name = t.ZipZone.Name,
                PickupCount = t.PickupCount,
                DropoffCount = t.DropoffCount,
                DensityPerKm2 = t.DensityPerKm2,
                PickupsByHour = t.PickupsByHour,
                DropoffsByHour = t.DropoffsByHour
            }).AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(borough) && borough != "All")
                query = query.Where(z => z.Borough == borough);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(z => z.PostalCode.Contains(search) || z.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            // Sort
            query = sortBy?.ToLower() switch
            {
                "dropoffcount" => sortOrder == "asc" ? query.OrderBy(z => z.DropoffCount) : query.OrderByDescending(z => z.DropoffCount),
                "densityperkm2" => sortOrder == "asc" ? query.OrderBy(z => z.DensityPerKm2) : query.OrderByDescending(z => z.DensityPerKm2),
                "name" => sortOrder == "asc" ? query.OrderBy(z => z.Name) : query.OrderByDescending(z => z.Name),
                _ => sortOrder == "asc" ? query.OrderBy(z => z.PickupCount) : query.OrderByDescending(z => z.PickupCount)
            };

            var totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ZoneSummaryDTO>
            {
                Items = items,
                TotalItemCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}