using Hangfire;
using MiniShop.Core.Services;

namespace MiniShop.Api.Jobs;

public class ExpireOrdersJob
{
    private readonly OrderExpirationService _service;
    private readonly ILogger<ExpireOrdersJob> _logger;

    public ExpireOrdersJob(OrderExpirationService service, ILogger<ExpireOrdersJob> logger)
    {
        _service = service;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task RunAsync()
    {
        var count = await _service.ExpireAsync();
        _logger.LogInformation("Expired {Count} pending orders", count);
    }
}