using MiniShop.Core.Entities;
using MiniShop.Core.Exceptions;

namespace MiniShop.Tests;

public class OrderTests
{
    [Fact]
    public void Cancel_WhenPending_SetsStatusToCancelled()
    {
        var order = new Order { Status = OrderStatus.Pending };

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Cancelled)]
    public void Cancel_WhenNotPending_Throws(OrderStatus status)
    {
        var order = new Order { Status = status };

        Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Equal(status, order.Status);   // trạng thái không bị đổi
    }
}