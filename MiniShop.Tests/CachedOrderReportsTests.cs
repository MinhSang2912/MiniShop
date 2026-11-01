using Microsoft.Extensions.Caching.Memory;
using MiniShop.Core.Interfaces;
using MiniShop.Infrastructure;
using Moq;

namespace MiniShop.Tests;

public class CachedOrderReportsTests
{
    [Fact]
    public async Task GetMonthlyRevenueAsync_CalledTwice_HitsInnerOnce()
    {
        var inner = new Mock<IOrderReports>();
        inner.Setup(r => r.GetMonthlyRevenueAsync())
             .ReturnsAsync(new List<MonthlyRevenue> { new(2026, 1, 100m, 1) });
        var cached = new CachedOrderReports(inner.Object, new MemoryCache(new MemoryCacheOptions()));

        await cached.GetMonthlyRevenueAsync();
        await cached.GetMonthlyRevenueAsync();

        inner.Verify(r => r.GetMonthlyRevenueAsync(), Times.Once);
    }
}