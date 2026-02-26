using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Application.DTOs;
using ImagesShop.Domain.Entities;
using System.Security.Claims;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet(Name = "GetAllOrders")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var orders = await _orderService.GetAllAsync(cancellationToken);
            var ordersDto = orders.Select(order => MapToDto(order));
            
            return Ok(ordersDto);
        }

        [HttpGet("{id:guid}", Name = "GetOrderById")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetByIdAsync(id, cancellationToken);
            
            if (order is null)
            {
                return NotFound();
            }

            var userId = GetUserIdOrThrow();
            
            if (User.IsInRole("Admin") || order.UserId == userId)
            {
                return Ok(MapToDto(order));
            }

            return Forbid();
        }

        [HttpPost("buy/{imageId:guid}", Name = "BuyImage")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Buy(Guid imageId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();
            var purchaseResult = await _orderService.PurchaseImageAsync(userId, imageId, cancellationToken);
            
            return Ok(purchaseResult);
        }

        [HttpPost(Name = "CreateOrder")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] OrderDTO orderDto, CancellationToken cancellationToken)
        {
            var orderEntity = MapToEntity(orderDto);
            var createdOrder = await _orderService.CreateAsync(orderEntity, cancellationToken);
            
            return CreatedAtAction(nameof(Get), new { id = createdOrder.Id }, MapToDto(createdOrder));
        }

        [HttpPut("{id:guid}", Name = "UpdateOrder")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderDTO orderDto, CancellationToken cancellationToken)
        {
            if (id != orderDto.Id)
            {
                return BadRequest("Order ID mismatch.");
            }

            var orderEntity = MapToEntity(orderDto);
            await _orderService.UpdateAsync(orderEntity, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteOrder")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _orderService.DeleteAsync(id, cancellationToken);
            
            return NoContent();
        }

        private static OrderDTO MapToDto(Order order) => new OrderDTO
        {
            Id = order.Id,
            UserId = order.UserId,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Notes = order.Notes,
            Items = order.Items?.Select(orderItem => new OrderItemDTO
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                ImageId = orderItem.ImageId
            }).ToList() ?? new List<OrderItemDTO>()
        };

        private static Order MapToEntity(OrderDTO orderDto) => new Order
        {
            Id = orderDto.Id == Guid.Empty ? Guid.NewGuid() : orderDto.Id,
            UserId = orderDto.UserId,
            CreatedAt = orderDto.CreatedAt == default ? DateTime.UtcNow : orderDto.CreatedAt,
            Status = orderDto.Status,
            TotalAmount = orderDto.TotalAmount,
            Currency = orderDto.Currency,
            Notes = orderDto.Notes,
            Items = orderDto.Items?.Select(orderItemDto => new OrderItem
            {
                Id = orderItemDto.Id == Guid.Empty ? Guid.NewGuid() : orderItemDto.Id,
                OrderId = orderDto.Id == Guid.Empty ? Guid.NewGuid() : orderDto.Id,
                ImageId = orderItemDto.ImageId
            }).ToList() ?? new List<OrderItem>()
        };

        private Guid GetUserIdOrThrow()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            
            if (!Guid.TryParse(nameIdentifier, out var userId)) 
            {
                throw new UnauthorizedAccessException();
            }
            
            return userId;
        }
    }
}