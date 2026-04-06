using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/invoices/analytics")]
[Authorize(Roles = "FinanceManager,Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly InvoiceAnalyticsService _service;

    public AnalyticsController(InvoiceAnalyticsService service)
    {
        _service = service;
    }

    [HttpGet("outstanding")]
    public async Task<IActionResult> GetOutstanding()
        => Ok(await _service.GetOutstanding());

    [HttpGet("revenue-summary")]
    public async Task<IActionResult> GetRevenue()
        => Ok(await _service.GetRevenue());

    [HttpGet("dso")]
    public async Task<IActionResult> GetDSO()
        => Ok(await _service.GetDSO());

    [HttpGet("aging")]
    public async Task<IActionResult> GetAging()
        => Ok(await _service.GetAging());
}