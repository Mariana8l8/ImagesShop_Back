using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Application.DTOs;
using ImagesShop.Domain.Entities;
using ImagesShop.Domain.Enums;
using System.Security.Claims;

namespace ImagesShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderItemService _orderItemService;
        private readonly IImageService _imageService;
        private readonly IUserService _userService;
        private readonly IPurchaseHistoryService _purchaseHistory;

        public OrdersController(
            IOrderService orderService,
            IOrderItemService orderItemService,
            IImageService imageService,
            IUserService userService,
            IPurchaseHistoryService purchaseHistory)
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
            _imageService = imageService;
            _userService = userService;
            _purchaseHistory = purchaseHistory;
        }

        [HttpGet(Name = "GetAllOrders")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _orderService.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetOrderById")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetByIdAsync(id, cancellationToken);
            if (order is null) return NotFound();

            var userId = GetUserId();
            if (userId is null) return Forbid();
            if (User.IsInRole("Admin") || order.UserId == userId.Value)
            {
                return Ok(MapToDto(order));
            }

            return Forbid();
        }

        [HttpPost("buy/{imageId:guid}", Name = "BuyImage")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Buy(Guid imageId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId is null) return Forbid();

            var user = await _userService.GetByIdAsync(userId.Value, cancellationToken);
            if (user is null) return BadRequest("User not found");

            var image = await _imageService.GetByIdAsync(imageId, cancellationToken);
            if (image is null) return NotFound();

            if (user.Balance < image.Price) return BadRequest("Insufficient balance");

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Status = OrderStatus.Completed,
                TotalAmount = image.Price,
                Currency = "USD",
                Items = new List<OrderItem>()
            };

            await _orderService.CreateAsync(order, cancellationToken);

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ImageId = image.Id
            };
            await _orderItemService.CreateAsync(orderItem, cancellationToken);

            user.Balance -= image.Price;
            await _userService.UpdateAsync(user, cancellationToken);

            var purchase = new PurchaseHistory
            {
                Id = Guid.NewGuid(),
                UserName = user.Name,
                UserEmail = user.Email,
                ImageId = image.Id,
                ImagePrice = image.Price,
                ImageTitle = image.Title,
                PurchasedAt = DateTime.UtcNow
            };
            await _purchaseHistory.CreateAsync(purchase, cancellationToken);

            return Ok(new
            {
                orderId = order.Id,
                originalUrl = image.OriginalUrl
            });
        }

        [HttpPost(Name = "CreateOrder")]
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
            CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt,
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

        private Guid? GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}