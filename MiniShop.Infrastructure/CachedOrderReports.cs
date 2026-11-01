using Microsoft.Extensions.Caching.Memory;
using MiniShop.Core.Interfaces;

namespace MiniShop.Infrastructure;

public class CachedOrderReports : IOrderReports
{
    private const string Key = "reports:monthly-revenue";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly IOrderReports _inner;
    private readonly IMemoryCache _cache;

    public CachedOrderReports(IOrderReports inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync()
    {
        if (_cache.TryGetValue(Key, out List<MonthlyRevenue>? cached) && cached is not null)
            return cached;

        await Gate.WaitAsync();
        try
        {
            // Kiểm tra lại: request khác có thể đã nạp cache trong lúc ta chờ
            if (_cache.TryGetValue(Key, out cached) && cached is not null)
                return cached;

            var data = await _inner.GetMonthlyRevenueAsync();
            _cache.Set(Key, data, Ttl);
            return data;
        }
        finally
        {
            Gate.Release();
        }
    }
}