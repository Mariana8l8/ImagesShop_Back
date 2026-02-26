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
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCart(CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();

            var cartItems = await _cartService.GetMyCartAsync(userId, cancellationToken);
            var cartItemsDto = cartItems.Select(cartItem => MapToDto(cartItem));
            
            return Ok(cartItemsDto);
        }

        [HttpPost("items")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Add([FromBody] AddToCartRequestDTO request, CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();

            await _cartService.AddAsync(userId, request.ImageId, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("items/{imageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Remove(Guid imageId, CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();

            await _cartService.RemoveAsync(userId, imageId, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Clear(CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();

            await _cartService.ClearAsync(userId, cancellationToken);
            
            return NoContent();
        }

        private static CartItemDTO MapToDto(CartItem cartItem) => new()
        {
            ImageId = cartItem.ImageId,
            Title = cartItem.Image.Title,
            Price = cartItem.Image.Price,
            WatermarkedUrl = cartItem.Image.WatermarkedUrl
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
