using Microsoft.EntityFrameworkCore;
using TripAnalytics.API.Data;
using TripAnalytics.API.Domain.Entities;
using TripAnalytics.API.Repositories.Interfaces;

namespace TripAnalytics.API.Repositories
{
    public class ZoneRepository : IZoneRepository
    {
        private readonly AppDbContext _context;

        public ZoneRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TripSummary>> GetAllWithZoneAsync()
        {
            return await _context.TripSummaries
                .Include(t => t.ZipZone)
                .ToListAsync();
        }

        public async Task<TripSummary?> GetByPostalCodeAsync(string postalCode)
        {
            return await _context.TripSummaries
                .Include(t => t.ZipZone)
                .FirstOrDefaultAsync(t => t.PostalCode == postalCode);
        }

        public async Task<ZonePairSummary?> GetPairAsync(string from, string to)
        {
            return await _context.ZonePairSummaries
                .FirstOrDefaultAsync(p => p.PickupZip == from && p.DropoffZip == to);
        }
    }
}
