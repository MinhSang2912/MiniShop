using MiniShop.Core.Entities;
using MiniShop.Core.Interfaces;

namespace MiniShop.Infrastructure;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new()
    {
        new Order
        {
            Id = "000000000000000000000001",
            CustomerId = "000000000000000000000001",
            Status = OrderStatus.Pending,
            Total = 99,
            CreatedAt = DateTime.UtcNow
        }
    };

    public Task<Order?> GetByIdAsync(string id)
        => Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

    public Task<PagedResult<Order>> GetByCustomerAsync(string customerId, OrderStatus? status, int page, int pageSize)
    {
        var query = _orders.Where(o => o.CustomerId == customerId && (status == null || o.Status == status))
                           .OrderByDescending(o => o.CreatedAt).ToList();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PagedResult<Order>(items, page, pageSize, query.Count));
    }

    public Task UpdateStatusAsync(string id, OrderStatus status)
    {
        _orders.First(o => o.Id == id).Status = status;
        return Task.CompletedTask;
    }
    public Task<long> CancelExpiredPendingAsync(DateTime olderThanUtc)
    {
        var expired = _orders.Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < olderThanUtc).ToList();
        expired.ForEach(o => o.Status = OrderStatus.Cancelled);
        return Task.FromResult((long)expired.Count);
    }
}