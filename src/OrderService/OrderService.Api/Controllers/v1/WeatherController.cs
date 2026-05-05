using BuildingBlocks.Controllers;
using BuildingBlocks.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Clients;
using OrderService.Application.Common.Interfaces;

namespace OrderService.Api.Controllers.v1;

/// <summary>
/// External API demo endpoint.
/// Shows best-practice pattern for calling a third-party API via typed HttpClient + Refit.
/// </summary>
public sealed class WeatherController : BaseApiController
{
    private readonly IWeatherQueryService _weatherQueryService;

    public WeatherController(IWeatherQueryService weatherQueryService)
    {
        _weatherQueryService = weatherQueryService;
    }

    /// <summary>
    /// Gets current weather from WeatherAPI.
    /// Example: GET /api/v1/weather/current?q=London&amp;aqi=no
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<WeatherCurrentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrent([FromQuery] string q, [FromQuery] string aqi = "no", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiResponse<object>.FailureResponse(400, "Query parameter 'q' is required."));
        }

        var data = await _weatherQueryService.GetCurrentAsync(q, aqi, cancellationToken);
        return Ok(ApiResponse<WeatherCurrentResponse>.SuccessResponse(data, "Weather data fetched successfully."));
    }
}
