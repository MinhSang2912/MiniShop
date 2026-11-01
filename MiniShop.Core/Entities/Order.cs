using MiniShop.Core.Exceptions;

namespace MiniShop.Core.Entities
{
    public class Order
    {
        public string Id { get; set; } = default!;
        public string CustomerId { get; set; } = default!;
        public List<OrderItem> Items { get; set; } = new();
        public OrderStatus Status { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public void Cancel()
        {
            if (Status != OrderStatus.Pending)
                throw new DomainException($"Cannot cancel an order in status {Status}.");
            Status = OrderStatus.Cancelled;
        }
    }
}
