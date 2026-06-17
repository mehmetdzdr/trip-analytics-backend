using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TripAnalytics.API.Domain.Entities;
using TripAnalytics.API.Repositories.Interfaces;
using TripAnalytics.API.Services;

namespace TripAnalytics.Tests.Services
{
    public class ZoneServiceTests
    {
        private readonly Mock<IZoneRepository> _repositoryMock;
        private readonly ZoneService _zoneService;

        public ZoneServiceTests()
        {
            _repositoryMock = new Mock<IZoneRepository>();
            _zoneService = new ZoneService(_repositoryMock.Object);
        }

        //HELPER
        private static TripSummary CreateTripSummary(
           string postalCode, string borough, string name,
           int pickupCount, int dropoffCount, double densityPerKm2)
        {
            return new TripSummary
            {
                PostalCode = postalCode,
                PickupCount = pickupCount,
                DropoffCount = dropoffCount,
                AvgFare = 10.0,
                AvgDistance = 2.5,
                DensityPerKm2 = densityPerKm2,
                PickupsByHour = new int[24],
                DropoffsByHour = new int[24],
                ZipZone = new ZipZone
                {
                    PostalCode = postalCode,
                    Borough = borough,
                    Name = name,
                    AreaKm2 = 5.0
                }
            };
        }

        [Fact]
        public async Task GetAllAsync_MapsRepositoryDataToDto()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 100, 80, 50.5)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetAllAsync();

            Assert.Single(result);
            Assert.Equal("10001", result[0].PostalCode);
            Assert.Equal("Manhattan", result[0].Borough);
            Assert.Equal("Chelsea", result[0].Name);
            Assert.Equal(100, result[0].PickupCount);
            Assert.Equal(80, result[0].DropoffCount);
            Assert.Equal(50.5, result[0].DensityPerKm2);
        }

        [Fact]
        public async Task GetByPostalCodeAsync_NotFound_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByPostalCodeAsync("99999")).ReturnsAsync((TripSummary?)null);

            var result = await _zoneService.GetByPostalCodeAsync("99999");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByPostalCodeAsync_Found_ReturnsDetailDto()
        {
            var summary = CreateTripSummary("10001", "Manhattan", "Chelsea", 100, 80, 50.5);
            _repositoryMock.Setup(r => r.GetByPostalCodeAsync("10001")).ReturnsAsync(summary);

            var result = await _zoneService.GetByPostalCodeAsync("10001");

            Assert.NotNull(result);
            Assert.Equal("10001", result!.PostalCode);
            Assert.Equal("Manhattan", result.Borough);
            Assert.Equal("Chelsea", result.Name);
            Assert.Equal(5.0, result.AreaKm2);
            Assert.Equal(31, result.DaysInDataset);
        }

        [Fact]
        public async Task GetPairAsync_NotFound_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetPairAsync("10001", "10002")).ReturnsAsync((ZonePairSummary?)null);

            var result = await _zoneService.GetPairAsync("10001", "10002");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetPairAsync_Found_ReturnsPairDto()
        {
            var pair = new ZonePairSummary
            {
                PickupZip = "10001",
                DropoffZip = "10002",
                TripCount = 42,
                AvgDuration = 15.5
            };
            _repositoryMock.Setup(r => r.GetPairAsync("10001", "10002")).ReturnsAsync(pair);

            var result = await _zoneService.GetPairAsync("10001", "10002");

            Assert.NotNull(result);
            Assert.Equal("10001", result!.PickupZip);
            Assert.Equal("10002", result.DropoffZip);
            Assert.Equal(42, result.TripCount);
            Assert.Equal(15.5, result.AvgDuration);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersByBorough()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 100, 80, 50.5),
                CreateTripSummary("11201", "Brooklyn", "DUMBO", 60, 40, 30.0)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetPagedAsync(1, 10, null, null, "Manhattan", null);

            Assert.Equal(1, result.TotalItemCount);
            Assert.Equal("10001", result.Items[0].PostalCode);
        }

        [Fact]
        public async Task GetPagedAsync_BoroughAll_DoesNotFilter()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 100, 80, 50.5),
                CreateTripSummary("11201", "Brooklyn", "DUMBO", 60, 40, 30.0)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetPagedAsync(1, 10, null, null, "All", null);

            Assert.Equal(2, result.TotalItemCount);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersBySearch_MatchesPostalCode()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 100, 80, 50.5),
                CreateTripSummary("11201", "Brooklyn", "DUMBO", 60, 40, 30.0)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetPagedAsync(1, 10, null, null, null, "10001");

            Assert.Equal(1, result.TotalItemCount);
            Assert.Equal("10001", result.Items[0].PostalCode);
        }

        [Fact]
        public async Task GetPagedAsync_FiltersBySearch_MatchesNameCaseInsensitive()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 100, 80, 50.5),
                CreateTripSummary("11201", "Brooklyn", "DUMBO", 60, 40, 30.0)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetPagedAsync(1, 10, null, null, null, "chelsea");

            Assert.Equal(1, result.TotalItemCount);
            Assert.Equal("Chelsea", result.Items[0].Name);
        }

        [Fact]
        public async Task GetPagedAsync_DefaultSort_OrdersByPickupCountDescending()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 50, 80, 50.5),
                CreateTripSummary("11201", "Brooklyn", "DUMBO", 100, 40, 30.0)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetPagedAsync(1, 10, null, null, null, null);

            Assert.Equal("11201", result.Items[0].PostalCode);
            Assert.Equal("10001", result.Items[1].PostalCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByDropoffCountAscending()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 50, 80, 50.5),
                CreateTripSummary("11201", "Brooklyn", "DUMBO", 100, 40, 30.0)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetPagedAsync(1, 10, "dropoffcount", "asc", null, null);

            Assert.Equal("11201", result.Items[0].PostalCode);
            Assert.Equal("10001", result.Items[1].PostalCode);
        }

        
        [Fact]
        public async Task GetPagedAsync_PaginatesResultsCorrectly()
        {
            var summaries = new List<TripSummary>
            {
                CreateTripSummary("10001", "Manhattan", "Chelsea", 300, 80, 50.5),
                CreateTripSummary("11201", "Brooklyn", "DUMBO", 200, 40, 30.0),
                CreateTripSummary("10002", "Manhattan", "SoHo", 100, 60, 40.0)
            };
            _repositoryMock.Setup(r => r.GetAllWithZoneAsync()).ReturnsAsync(summaries);

            var result = await _zoneService.GetPagedAsync(2, 2, null, null, null, null);

            Assert.Equal(3, result.TotalItemCount);
            Assert.Single(result.Items);
            Assert.Equal("10002", result.Items[0].PostalCode);
        }


    }
}
