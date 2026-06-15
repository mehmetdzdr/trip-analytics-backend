using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripAnalytics.API.Services.Interfaces;

namespace TripAnalytics.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/zones")]
    public class ZoneController : ControllerBase
    {
        private readonly IZoneService _service;

        public ZoneController(IZoneService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{postalCode}")]
        public async Task<IActionResult> GetByPostalCode(string postalCode)
        {
            var result = await _service.GetByPostalCodeAsync(postalCode);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("pair")]
        public async Task<IActionResult> GetPair([FromQuery] string from, [FromQuery] string to)
        {
            var result = await _service.GetPairAsync(from, to);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "pickupCount",
            [FromQuery] string? sortOrder = "desc",
            [FromQuery] string? borough = null,
            [FromQuery] string? search = null)
            {
                var result = await _service.GetPagedAsync(page, pageSize, sortBy, sortOrder, borough, search);
                return Ok(result);
            }
    }
}
