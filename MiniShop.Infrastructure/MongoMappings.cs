using MiniShop.Core;
using MiniShop.Core.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace MiniShop.Infrastructure;

public static class MongoMappings
{
    private static bool _done;

    public static void Register()
    {
        if (_done) return;
        _done = true;

        var objectIdString = new StringSerializer(BsonType.ObjectId);
        var money = new DecimalSerializer(BsonType.Decimal128);

        BsonClassMap.RegisterClassMap<Customer>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetSerializer(objectIdString)
              .SetIdGenerator(StringObjectIdGenerator.Instance);
        });

        BsonClassMap.RegisterClassMap<Product>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetSerializer(objectIdString)
              .SetIdGenerator(StringObjectIdGenerator.Instance);
            cm.MapMember(x => x.Price).SetSerializer(money);
        });

        BsonClassMap.RegisterClassMap<OrderItem>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(x => x.ProductId).SetSerializer(objectIdString);
            cm.MapMember(x => x.Price).SetSerializer(money);
        });

        BsonClassMap.RegisterClassMap<Order>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(x => x.Id).SetSerializer(objectIdString)
              .SetIdGenerator(StringObjectIdGenerator.Instance);
            cm.MapMember(x => x.CustomerId).SetSerializer(objectIdString);
            cm.MapMember(x => x.Status)
              .SetSerializer(new EnumSerializer<OrderStatus>(BsonType.String));
            cm.MapMember(x => x.Total).SetSerializer(money);
        });
    }
}