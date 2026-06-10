using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Globalization;
using System.Text.Json;
using TripAnalytics.API.Data;
using TripAnalytics.API.Domain.Entities;
using TripAnalytics.API.Models;

namespace TripAnalytics.API.Services;

public class TripAggregatorService
{
    private readonly AppDbContext _context;

    public TripAggregatorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AggregateAndSaveAsync(string csvPath, string geoJsonPath)
    {
        if (await _context.TripSummaries.AnyAsync())
        {
            Console.WriteLine("Trip summaries already exist. Skipping.");
            return;
        }

        Console.WriteLine("Loading ZIP polygons...");
        var zipPolygons = LoadZipPolygons(geoJsonPath);
        Console.WriteLine($"Loaded {zipPolygons.Count} ZIP polygons.");

        // TripSummary dictionary'leri
        var pickupCounts = new Dictionary<string, int>();
        var dropoffCounts = new Dictionary<string, int>();
        var fareTotals = new Dictionary<string, double>();
        var distTotals = new Dictionary<string, double>();
        var pickupsByHour = new Dictionary<string, int[]>();
        var dropoffsByHour = new Dictionary<string, int[]>();

        // ZonePairSummary dictionary
        var pairStats = new Dictionary<(string, string), (int Count, double TotalDuration)>();

        foreach (var zip in zipPolygons)
        {
            pickupCounts[zip.PostalCode] = 0;
            dropoffCounts[zip.PostalCode] = 0;
            fareTotals[zip.PostalCode] = 0;
            distTotals[zip.PostalCode] = 0;
            pickupsByHour[zip.PostalCode] = new int[24];
            dropoffsByHour[zip.PostalCode] = new int[24];
        }

        Console.WriteLine("Processing CSV...");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            MissingFieldFound = null
        };

        var factory = new GeometryFactory();
        long processed = 0;

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);

        await foreach (var record in csv.GetRecordsAsync<CsvTripRecord>())
        {
            processed++;
            if (processed % 500_000 == 0)
                Console.WriteLine($"Processed {processed:N0} rows...");

            var pickupZip = FindZip(record.PickupLongitude, record.PickupLatitude, zipPolygons, factory);
            var dropoffZip = FindZip(record.DropoffLongitude, record.DropoffLatitude, zipPolygons, factory);

            // Pickup istatistikleri
            if (pickupZip != null)
            {
                pickupCounts[pickupZip]++;
                fareTotals[pickupZip] += record.FareAmount;
                distTotals[pickupZip] += record.TripDistance;
                pickupsByHour[pickupZip][record.PickupDatetime.Hour]++;
            }

            // Dropoff istatistikleri
            if (dropoffZip != null)
            {
                dropoffCounts[dropoffZip]++;
                dropoffsByHour[dropoffZip][record.DropoffDatetime.Hour]++;
            }

            // ZonePair istatistikleri
            if (pickupZip != null && dropoffZip != null)
            {
                var duration = (record.DropoffDatetime - record.PickupDatetime).TotalMinutes;

                if (duration > 0 && duration < 180)
                {
                    var key = (pickupZip, dropoffZip);
                    if (!pairStats.ContainsKey(key))
                        pairStats[key] = (0, 0);

                    var (count, total) = pairStats[key];
                    pairStats[key] = (count + 1, total + duration);
                }
            }
        }

        Console.WriteLine($"CSV processing complete. Total rows: {processed:N0}");

        var zipZones = await _context.ZipZones.ToDictionaryAsync(z => z.PostalCode);

        // TripSummaries kaydet
        var summaries = zipPolygons
            .Where(z => pickupCounts[z.PostalCode] > 0 || dropoffCounts[z.PostalCode] > 0)
            .Select(z =>
            {
                var pc = z.PostalCode;
                var pickups = pickupCounts[pc];
                var area = zipZones.TryGetValue(pc, out var zone) ? zone.AreaKm2 : 1;

                return new TripSummary
                {
                    PostalCode = pc,
                    PickupCount = pickups,
                    DropoffCount = dropoffCounts[pc],
                    AvgFare = pickups > 0 ? fareTotals[pc] / pickups : 0,
                    AvgDistance = pickups > 0 ? distTotals[pc] / pickups : 0,
                    DensityPerKm2 = area > 0 ? pickups / area : 0,
                    PickupsByHour = pickupsByHour[pc],
                    DropoffsByHour = dropoffsByHour[pc]
                };
            }).ToList();

        Console.WriteLine($"Saving {summaries.Count} trip summaries...");
        await _context.TripSummaries.AddRangeAsync(summaries);
        await _context.SaveChangesAsync();
        Console.WriteLine("Trip summaries saved.");

        // ZonePairSummaries kaydet
        var pairs = pairStats.Select(kv => new ZonePairSummary
        {
            PickupZip = kv.Key.Item1,
            DropoffZip = kv.Key.Item2,
            TripCount = kv.Value.Count,
            AvgDuration = kv.Value.TotalDuration / kv.Value.Count
        }).ToList();

        Console.WriteLine($"Saving {pairs.Count} zone pair summaries...");
        await _context.ZonePairSummaries.AddRangeAsync(pairs);
        await _context.SaveChangesAsync();
        Console.WriteLine("Zone pair summaries saved.");
    }

    private List<ZipPolygon> LoadZipPolygons(string geoJsonPath)
    {
        var json = File.ReadAllText(geoJsonPath);
        var collection = JsonSerializer.Deserialize<GeoJsonFeatureCollection>(json)!;
        var reader = new GeoJsonReader();

        return collection.Features
            .GroupBy(f => f.Properties.PostalCode)
            .Select(g => g.First())
            .Select(f =>
            {
                var geometryJson = JsonSerializer.Serialize(f.Geometry);
                var geometry = reader.Read<Geometry>(geometryJson);

                return new ZipPolygon
                {
                    PostalCode = f.Properties.PostalCode,
                    Geometry = geometry,
                    BoundingBox = geometry.EnvelopeInternal
                };
            }).ToList();
    }

    private string? FindZip(double lon, double lat, List<ZipPolygon> polygons, GeometryFactory factory)
    {
        if (lon < -74.26 || lon > -73.70 || lat < 40.49 || lat > 40.92)
            return null;

        var point = factory.CreatePoint(new Coordinate(lon, lat));

        return polygons
            .Where(z => z.BoundingBox.Contains(lon, lat))
            .FirstOrDefault(z => z.Geometry.Contains(point))
            ?.PostalCode;
    }
}