using DJIMarket.Application.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace DJIMarket.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IAnalyticsService analytics) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string rankBy = "profit",
        CancellationToken cancellationToken = default)
    {
        var toValue = to ?? new DateOnly(2026, 9, 25);
        var fromValue = from ?? toValue.AddDays(-29);
        if (fromValue >= toValue) return BadRequest(new { message = "from must be before to (to is exclusive)." });
        var metric = rankBy.Equals("averageCheck", StringComparison.OrdinalIgnoreCase) ? RankingMetric.AverageCheck : RankingMetric.GrossProfit;
        try
        {
            return Ok(await analytics.GetDashboardAsync(new DashboardQuery(fromValue, toValue, metric), cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
