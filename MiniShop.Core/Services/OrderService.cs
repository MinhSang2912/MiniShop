using MiniShop.Core.Entities;
using MiniShop.Core.Exceptions;
using MiniShop.Core.Interfaces;

namespace MiniShop.Core.Services;

public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;

    public Task<PagedResult<Order>> GetByCustomerAsync(string customerId, OrderStatus? status, int page = 1, int pageSize = 20)
        => _repo.GetByCustomerAsync(customerId, status, Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));

    public async Task<Order> CancelAsync(string id)
    {
        var order = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Order {id} not found.");
        order.Cancel();
        await _repo.UpdateStatusAsync(id, order.Status);
        return order;
    }
}