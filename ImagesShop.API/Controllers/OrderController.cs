using Microsoft.AspNetCore.Mvc;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Application.DTOs;
using ImagesShop.Domain.Entities;

namespace ImagesShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet(Name = "GetAllOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _orderService.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetOrderById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetByIdAsync(id, cancellationToken);
            return order is null ? NotFound() : Ok(MapToDto(order));
        }

        [HttpPost(Name = "CreateOrder")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] OrderDTO dto, CancellationToken cancellationToken)
        {
            var entity = MapToEntity(dto);
            var created = await _orderService.CreateAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id:guid}", Name = "UpdateOrder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.Id) return BadRequest();
            var entity = MapToEntity(dto);
            await _orderService.UpdateAsync(entity, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteOrder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            Items = order.Items?.Select(orderItemDTO => new OrderItemDTO
            {
                Id = orderItemDTO.Id,
                OrderId = orderItemDTO.OrderId,
                ImageId = orderItemDTO.ImageId
            }).ToList() ?? new List<OrderItemDTO>()
        };

        private static Order MapToEntity(OrderDTO dto) => new Order
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            UserId = dto.UserId,
            CreatedAt = dto.CreatedAt,
            Status = dto.Status,
            TotalAmount = dto.TotalAmount,
            Currency = dto.Currency,
            Notes = dto.Notes,
            Items = dto.Items?.Select(orderItem => new OrderItem
            {
                Id = orderItem.Id == Guid.Empty ? Guid.NewGuid() : orderItem.Id,
                OrderId = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                ImageId = orderItem.ImageId
            }).ToList() ?? new List<OrderItem>()
        };
    }
}