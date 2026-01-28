using Microsoft.AspNetCore.Mvc;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Application.DTOs;
using ImagesShop.Domain.Entities;

namespace ImagesShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet(Name = "GetAllOrderItems")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _orderItemService.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetOrderItemById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var item = await _orderItemService.GetByIdAsync(id, cancellationToken);
            return item is null ? NotFound() : Ok(MapToDto(item));
        }

        [HttpGet("order/{orderId:guid}", Name = "GetItemsByOrderId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
        {
            var items = await _orderItemService.GetByOrderIdAsync(orderId, cancellationToken);
            return Ok(items.Select(MapToDto));
        }

        [HttpPost(Name = "CreateOrderItem")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] OrderItemDTO dto, CancellationToken cancellationToken)
        {
            var entity = MapToEntity(dto);
            var created = await _orderItemService.CreateAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id:guid}", Name = "UpdateOrderItem")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderItemDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.Id) return BadRequest();
            var entity = MapToEntity(dto);
            await _orderItemService.UpdateAsync(entity, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteOrderItem")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _orderItemService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        private static OrderItemDTO MapToDto(OrderItem orderItem) => new OrderItemDTO
        {
            Id = orderItem.Id,
            OrderId = orderItem.OrderId,
            ImageId = orderItem.ImageId
        };

        private static OrderItem MapToEntity(OrderItemDTO dto) => new OrderItem
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            OrderId = dto.OrderId,
            ImageId = dto.ImageId
        };
    }
}