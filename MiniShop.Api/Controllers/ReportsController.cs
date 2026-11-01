using Microsoft.AspNetCore.Mvc;
using MiniShop.Core.Interfaces;

namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IOrderReports _reports;
    public ReportsController(IOrderReports reports) => _reports = reports;

    [HttpGet("monthly-revenue")]
    public async Task<IActionResult> MonthlyRevenue()
        => Ok(await _reports.GetMonthlyRevenueAsync());
}