using MiniShop.Core.Entities;
using MiniShop.Core.Exceptions;
using MiniShop.Core.Interfaces;
using MiniShop.Core.Services;
using Moq;

namespace MiniShop.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly OrderService _service;

    public OrderServiceTests() => _service = new OrderService(_repo.Object);

    [Fact]
    public async Task CancelAsync_PendingOrder_SavesCancelledStatus()
    {
        _repo.Setup(r => r.GetByIdAsync("o1"))
             .ReturnsAsync(new Order { Id = "o1", Status = OrderStatus.Pending });

        var result = await _service.CancelAsync("o1");

        Assert.Equal(OrderStatus.Cancelled, result.Status);
        _repo.Verify(r => r.UpdateStatusAsync("o1", OrderStatus.Cancelled), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_OrderNotFound_ThrowsKeyNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CancelAsync("missing"));
    }

    [Fact]
    public async Task CancelAsync_PaidOrder_ThrowsAndDoesNotSave()
    {
        _repo.Setup(r => r.GetByIdAsync("o2"))
             .ReturnsAsync(new Order { Id = "o2", Status = OrderStatus.Paid });

        await Assert.ThrowsAsync<DomainException>(() => _service.CancelAsync("o2"));

        _repo.Verify(r => r.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Theory]
    [InlineData(0, 500, 1, 100)]    // page < 1 => 1, pageSize > 100 => 100
    [InlineData(-5, 0, 1, 1)]       // pageSize < 1 => 1
    [InlineData(3, 20, 3, 20)]      // giá trị hợp lệ giữ nguyên
    public async Task GetByCustomerAsync_NormalizesPaging(int page, int pageSize, int expectedPage, int expectedSize)
    {
        _repo.Setup(r => r.GetByCustomerAsync(It.IsAny<string>(), It.IsAny<OrderStatus?>(), It.IsAny<int>(), It.IsAny<int>()))
             .ReturnsAsync(new PagedResult<Order>(new List<Order>(), 1, 20, 0));

        await _service.GetByCustomerAsync("c1", null, page, pageSize);

        _repo.Verify(r => r.GetByCustomerAsync("c1", null, expectedPage, expectedSize), Times.Once);
    }
}