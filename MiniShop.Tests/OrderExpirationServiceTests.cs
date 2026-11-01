using MiniShop.Core.Interfaces;
using MiniShop.Core.Services;
using Moq;

namespace MiniShop.Tests;

public class OrderExpirationServiceTests
{
    private sealed class FixedTime : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FixedTime(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }

    [Fact]
    public async Task ExpireAsync_UsesCutoff24HoursBeforeNow()
    {
        var now = new DateTimeOffset(2026, 10, 31, 12, 0, 0, TimeSpan.Zero);
        var repo = new Mock<IOrderRepository>();
        repo.Setup(r => r.CancelExpiredPendingAsync(It.IsAny<DateTime>())).ReturnsAsync(7L);
        var service = new OrderExpirationService(repo.Object, new FixedTime(now));

        var count = await service.ExpireAsync();

        Assert.Equal(7, count);
        repo.Verify(r => r.CancelExpiredPendingAsync(new DateTime(2026, 10, 30, 12, 0, 0, DateTimeKind.Utc)), Times.Once);
    }
}