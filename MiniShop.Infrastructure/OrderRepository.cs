using MiniShop.Core.Entities;
using MiniShop.Core.Interfaces;
using MongoDB.Driver;

namespace MiniShop.Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly IMongoCollection<Order> _orders;
    public OrderRepository(IMongoDatabase db) => _orders = db.GetCollection<Order>("orders");

    public async Task<Order?> GetByIdAsync(string id)
        => await _orders.Find(o => o.Id == id).FirstOrDefaultAsync();

    public async Task<PagedResult<Order>> GetByCustomerAsync(string customerId, OrderStatus? status, int page, int pageSize)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.CustomerId, customerId);
        if (status.HasValue)
            filter &= Builders<Order>.Filter.Eq(o => o.Status, status.Value);

        var total = await _orders.CountDocumentsAsync(filter);
        var items = await _orders.Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedResult<Order>(items, page, pageSize, total);
    }

    public Task UpdateStatusAsync(string id, OrderStatus status)
        => _orders.UpdateOneAsync(o => o.Id == id, Builders<Order>.Update.Set(o => o.Status, status));

    public async Task<long> CancelExpiredPendingAsync(DateTime olderThanUtc)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.Status, OrderStatus.Pending)
                   & Builders<Order>.Filter.Lt(o => o.CreatedAt, olderThanUtc);

        var result = await _orders.UpdateManyAsync(
            filter, Builders<Order>.Update.Set(o => o.Status, OrderStatus.Cancelled));

        return result.ModifiedCount;
    }
}