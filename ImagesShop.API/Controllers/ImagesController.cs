using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly IOrderService _orderService;

        public ImagesController(IImageService imageService, IOrderService orderService)
        {
            _imageService = imageService;
            _orderService = orderService;
        }

        [HttpGet(Name = "GetAllImages")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var images = await _imageService.GetAllAsync(cancellationToken);
            var currentUserId = GetUserId();
            var imagesDtoList = new List<ImageDTO>();
            
            foreach (var image in images)
            {
                var imageDto = MapToDto(image);
                
                if (currentUserId is null || !await HasPurchasedAsync(currentUserId.Value, image.Id, cancellationToken))
                {
                    imageDto.OriginalUrl = string.Empty;
                }
                
                imagesDtoList.Add(imageDto);
            }
            
            return Ok(imagesDtoList);
        }

        [HttpGet("{id:guid}", Name = "GetImageById")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var image = await _imageService.GetByIdAsync(id, cancellationToken);
            
            if (image is null)
            {
                return NotFound();
            }

            var imageDto = MapToDto(image);
            var currentUserId = GetUserId();
            
            if (currentUserId is null || !await HasPurchasedAsync(currentUserId.Value, id, cancellationToken))
            {
                imageDto.OriginalUrl = string.Empty;
            }

            return Ok(imageDto);
        }

        [HttpPost(Name = "AddImage")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] ImageDTO imageDto, CancellationToken cancellationToken)
        {
            var imageEntity = MapToEntity(imageDto);
            var createdImage = await _imageService.CreateAsync(imageEntity, cancellationToken);
            
            return CreatedAtAction(nameof(Get), new { id = createdImage.Id }, MapToDto(createdImage));
        }

        [HttpPut("{id:guid}", Name = "ChangeImageById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] ImageDTO imageDto, CancellationToken cancellationToken)
        {
            if (id != imageDto.Id)
            {
                return BadRequest("Image ID mismatch.");
            }

            var imageEntity = MapToEntity(imageDto);
            await _imageService.UpdateAsync(imageEntity, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteImageById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _imageService.DeleteAsync(id, cancellationToken);
            
            return NoContent();
        }

        private static ImageDTO MapToDto(Image image) => new ImageDTO
        {
            Id = image.Id,
            Title = image.Title,
            Description = image.Description,
            Price = image.Price,
            WatermarkedUrl = image.WatermarkedUrl,
            OriginalUrl = image.OriginalUrl,
            CategoryId = image.CategoryId
        };

        private static Image MapToEntity(ImageDTO imageDto) => new Image
        {
            Id = imageDto.Id == Guid.Empty ? Guid.NewGuid() : imageDto.Id,
            Title = imageDto.Title,
            Description = imageDto.Description,
            Price = imageDto.Price,
            WatermarkedUrl = imageDto.WatermarkedUrl,
            OriginalUrl = imageDto.OriginalUrl,
            CategoryId = imageDto.CategoryId
        };

        private Guid? GetUserId()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            return Guid.TryParse(nameIdentifier, out var userId) ? userId : null;
        }

        private async Task<bool> HasPurchasedAsync(Guid userId, Guid imageId, CancellationToken cancellationToken)
        {
            var userOrders = await _orderService.GetAllAsync(cancellationToken);
            return userOrders.Any(order => order.UserId == userId && order.Items.Any(item => item.ImageId == imageId));
        }
    }
}