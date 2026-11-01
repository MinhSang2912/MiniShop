using MiniShop.Core.Entities;

namespace MiniShop.Core.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task<PagedResult<Order>> GetByCustomerAsync(string customerId, OrderStatus? status, int page, int pageSize);
    Task UpdateStatusAsync(string id, OrderStatus status);
    Task<long> CancelExpiredPendingAsync(DateTime olderThanUtc);
}