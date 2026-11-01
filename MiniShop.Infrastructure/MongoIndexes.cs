using MiniShop.Core.Entities;
using MongoDB.Driver;

namespace MiniShop.Infrastructure;

public static class MongoIndexes
{
    public static async Task EnsureAsync(IMongoDatabase db)
    {
        var orders = db.GetCollection<Order>("orders");
        var keys = Builders<Order>.IndexKeys;

        // ESR: Equality (CustomerId, Status) -> Sort (CreatedAt)
        await orders.Indexes.CreateOneAsync(new CreateIndexModel<Order>(
            keys.Ascending(x => x.CustomerId)
                .Ascending(x => x.Status)
                .Descending(x => x.CreatedAt),
            new CreateIndexOptions { Name = "ix_customer_status_created" }));


        await orders.Indexes.CreateOneAsync(new CreateIndexModel<Order>(
            keys.Ascending(x => x.Status).Ascending(x => x.CreatedAt),
            new CreateIndexOptions { Name = "ix_status_created" }));
    }
}