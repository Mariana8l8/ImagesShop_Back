using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Application.DTOs;
using ImagesShop.Domain.Entities;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemsController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpGet(Name = "GetAllOrderItems")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var orderItems = await _orderItemService.GetAllAsync(cancellationToken);
            var orderItemsDto = orderItems.Select(orderItem => MapToDto(orderItem));
            
            return Ok(orderItemsDto);
        }

        [HttpGet("{id:guid}", Name = "GetOrderItemById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var orderItem = await _orderItemService.GetByIdAsync(id, cancellationToken);
            
            if (orderItem is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(orderItem));
        }

        [HttpGet("order/{orderId:guid}", Name = "GetItemsByOrderId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
        {
            var orderItems = await _orderItemService.GetByOrderIdAsync(orderId, cancellationToken);
            var orderItemsDto = orderItems.Select(orderItem => MapToDto(orderItem));
            
            return Ok(orderItemsDto);
        }

        [HttpPost(Name = "CreateOrderItem")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] OrderItemDTO orderItemDto, CancellationToken cancellationToken)
        {
            var orderItemEntity = MapToEntity(orderItemDto);
            var createdOrderItem = await _orderItemService.CreateAsync(orderItemEntity, cancellationToken);
            
            return CreatedAtAction(nameof(Get), new { id = createdOrderItem.Id }, MapToDto(createdOrderItem));
        }

        [HttpPut("{id:guid}", Name = "UpdateOrderItem")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderItemDTO orderItemDto, CancellationToken cancellationToken)
        {
            if (id != orderItemDto.Id)
            {
                return BadRequest("Order item ID mismatch.");
            }

            var orderItemEntity = MapToEntity(orderItemDto);
            await _orderItemService.UpdateAsync(orderItemEntity, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteOrderItem")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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

        private static OrderItem MapToEntity(OrderItemDTO orderItemDto) => new OrderItem
        {
            Id = orderItemDto.Id == Guid.Empty ? Guid.NewGuid() : orderItemDto.Id,
            OrderId = orderItemDto.OrderId,
            ImageId = orderItemDto.ImageId
        };
    }
}