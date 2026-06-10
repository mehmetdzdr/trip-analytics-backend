using TripAnalytics.API.Domain.Entities;
using TripAnalytics.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace TripAnalytics.API.Services
{
    public class GeoJsonService
    {
        private readonly AppDbContext _context;

        public GeoJsonService(AppDbContext context) {
            _context = context;
        }

        public async Task LoadAndSaveAsync(string filePath)
        {
            var count = await _context.ZipZones.CountAsync();
            Console.WriteLine($"ZipZones count before check: {count}");

            if (await _context.ZipZones.AnyAsync())
            {
                Console.WriteLine("Zip zones already exist. Skipping.");
                return;
            }

            var json = await File.ReadAllTextAsync(filePath);

            var collection = JsonSerializer.Deserialize<GeoJsonFeatureCollection>(json);

            if (collection == null)
            {
                throw new InvalidOperationException("Failed to deserialize GeoJSON data.");
            }

            var zipZones = collection.Features
                .GroupBy(f => f.Properties.PostalCode)
                .Select(g => g.First())
                .Select(f => new ZipZone
            {
                PostalCode = f.Properties.PostalCode,
                Borough = f.Properties.Borough,
                Name = f.Properties.PO_NAME,
                AreaKm2 = f.Properties.Shape_Area / 1_000_000
            }).ToList();

            Console.WriteLine($"Inserting {zipZones.Count} zip zones...");
            await _context.ZipZones.AddRangeAsync(zipZones);
            await _context.SaveChangesAsync();
            Console.WriteLine("Insert completed.");
        }
    }
}
