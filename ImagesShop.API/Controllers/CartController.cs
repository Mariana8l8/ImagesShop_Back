using System.Security.Claims;
using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart)
        {
            _cart = cart;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCart(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var items = await _cart.GetMyCartAsync(userId.Value, cancellationToken);
            return Ok(items.Select(MapToDto));
        }

        [HttpPost("items")]
        public async Task<IActionResult> Add([FromBody] AddToCartRequestDTO request, CancellationToken cancellationToken)
        {
            if (request is null || request.ImageId == Guid.Empty) return BadRequest();

            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            await _cart.AddAsync(userId.Value, request.ImageId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("items/{imageId:guid}")]
        public async Task<IActionResult> Remove(Guid imageId, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            await _cart.RemoveAsync(userId.Value, imageId, cancellationToken);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Clear(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            await _cart.ClearAsync(userId.Value, cancellationToken);
            return NoContent();
        }

        private static CartItemDTO MapToDto(CartItem item) => new()
        {
            ImageId = item.ImageId,
            Title = item.Image.Title,
            Price = item.Image.Price,
            WatermarkedUrl = item.Image.WatermarkedUrl
        };

        private Guid? GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
