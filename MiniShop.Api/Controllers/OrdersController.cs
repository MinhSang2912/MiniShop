using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MiniShop.Core.Entities;
using MiniShop.Core.Interfaces;
using MiniShop.Core.Services;

namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private const string ObjectIdPattern = "^[0-9a-fA-F]{24}$";
    private const string ObjectIdMessage = "Must be a 24-character hex string.";

    private readonly OrderService _service;
    public OrdersController(OrderService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<Order>>> GetByCustomer(
        [FromQuery, RegularExpression(ObjectIdPattern, ErrorMessage = ObjectIdMessage)] string customerId,
        [FromQuery] OrderStatus? status,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
        => Ok(await _service.GetByCustomerAsync(customerId, status, page, pageSize));

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<Order>> Cancel(
        [RegularExpression(ObjectIdPattern, ErrorMessage = ObjectIdMessage)] string id)
        => Ok(await _service.CancelAsync(id));
}