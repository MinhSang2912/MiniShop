using MiniShop.Core.Interfaces;

namespace MiniShop.Core.Services;

public class OrderExpirationService
{
    private static readonly TimeSpan MaxPendingAge = TimeSpan.FromHours(24);

    private readonly IOrderRepository _repo;
    private readonly TimeProvider _time;

    public OrderExpirationService(IOrderRepository repo, TimeProvider time)
    {
        _repo = repo;
        _time = time;
    }

    public Task<long> ExpireAsync()
        => _repo.CancelExpiredPendingAsync(_time.GetUtcNow().UtcDateTime - MaxPendingAge);
}