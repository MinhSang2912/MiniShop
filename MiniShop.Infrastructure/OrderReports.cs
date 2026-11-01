using MiniShop.Core.Entities;
using MiniShop.Core.Interfaces;
using MongoDB.Driver;

namespace MiniShop.Infrastructure;

public class OrderReports : IOrderReports
{
    private readonly IMongoCollection<Order> _orders;
    public OrderReports(IMongoDatabase db) => _orders = db.GetCollection<Order>("orders");

    public Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync() =>
        _orders.Aggregate()
            .Match(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Shipped)
            .Group(
                o => new { o.CreatedAt.Year, o.CreatedAt.Month },
                g => new MonthlyRevenue(g.Key.Year, g.Key.Month, g.Sum(x => x.Total), g.Count()))
            .SortBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();
}