using Bogus;
using MiniShop.Core.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MiniShop.Infrastructure;

public class DataSeeder
{
    private readonly IMongoDatabase _db;
    public DataSeeder(IMongoDatabase db) => _db = db;

    public async Task SeedAsync(int orderCount = 100_000)
    {
        var customers = _db.GetCollection<Customer>("customers");
        var products = _db.GetCollection<Product>("products");
        var orders = _db.GetCollection<Order>("orders");

        if (await orders.EstimatedDocumentCountAsync() > 0) return;

        var rnd = new Randomizer(42); // seed cố định: dữ liệu giống nhau mỗi lần

        var customerList = new Faker<Customer>()
            .UseSeed(1)
            .RuleFor(c => c.Id, _ => ObjectId.GenerateNewId().ToString())
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.City, f => f.Address.City())
            .Generate(1_000);

        var categories = new[] { "Laptop", "Phone", "Audio", "Camera", "Accessory" };
        var productList = new Faker<Product>()
            .UseSeed(2)
            .RuleFor(p => p.Id, _ => ObjectId.GenerateNewId().ToString())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Category, f => f.PickRandom(categories))
            .RuleFor(p => p.Price, f => Math.Round(f.Random.Decimal(10, 2000), 2))
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 500))
            .Generate(500);

        await customers.InsertManyAsync(customerList);
        await products.InsertManyAsync(productList);

        var statuses = Enum.GetValues<OrderStatus>();
        var from = DateTime.UtcNow.AddYears(-2);
        const int batchSize = 10_000;

        for (var done = 0; done < orderCount; done += batchSize)
        {
            var batch = new List<Order>(batchSize);
            for (var i = 0; i < Math.Min(batchSize, orderCount - done); i++)
            {
                var items = Enumerable.Range(0, rnd.Int(1, 4)).Select(_ =>
                {
                    var p = rnd.ArrayElement(productList.ToArray());
                    return new OrderItem { ProductId = p.Id, Qty = rnd.Int(1, 5), Price = p.Price };
                }).ToList();

                batch.Add(new Order
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    CustomerId = rnd.ArrayElement(customerList.ToArray()).Id,
                    Items = items,
                    Status = rnd.ArrayElement(statuses),
                    Total = items.Sum(x => x.Price * x.Qty),
                    CreatedAt = from.AddMinutes(rnd.Int(0, 2 * 365 * 24 * 60))
                });
            }
            await orders.InsertManyAsync(batch, new InsertManyOptions { IsOrdered = false });
        }
    }
}