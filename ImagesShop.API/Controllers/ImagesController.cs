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
        private readonly IImageService _images;
        private readonly IOrderService _orders;

        public ImagesController(IImageService images, IOrderService orders)
        {
            _images = images;
            _orders = orders;
        }

        [HttpGet(Name = "GetAllImages")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _images.GetAllAsync(cancellationToken);
            var userId = GetUserId();
            var dto = new List<ImageDTO>();
            foreach (var image in list)
            {
                var mapped = MapToDto(image);
                if (userId is null || !await HasPurchasedAsync(userId.Value, image.Id, cancellationToken))
                {
                    mapped.OriginalUrl = string.Empty;
                }
                dto.Add(mapped);
            }
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetImageById")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var img = await _images.GetByIdAsync(id, cancellationToken);
            if (img is null) return NotFound();

            var dto = MapToDto(img);
            var userId = GetUserId();
            if (userId is null || !await HasPurchasedAsync(userId.Value, id, cancellationToken))
            {
                dto.OriginalUrl = string.Empty;
            }

            return Ok(dto);
        }

        [HttpPost(Name = "AddImage")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ImageDTO imageDto, CancellationToken cancellationToken)
        {
            var entity = MapToEntity(imageDto);
            var created = await _images.CreateAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id:guid}", Name = "ChangeImageById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] ImageDTO imageDto, CancellationToken cancellationToken)
        {
            if (id != imageDto.Id) return BadRequest();
            var entity = MapToEntity(imageDto);
            await _images.UpdateAsync(entity, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteImageById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _images.DeleteAsync(id, cancellationToken);
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

        private static Image MapToEntity(ImageDTO dto) => new Image
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            WatermarkedUrl = dto.WatermarkedUrl,
            OriginalUrl = dto.OriginalUrl,
            CategoryId = dto.CategoryId
        };

        private Guid? GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            return Guid.TryParse(sub, out var id) ? id : null;
        }

        private async Task<bool> HasPurchasedAsync(Guid userId, Guid imageId, CancellationToken cancellationToken)
        {
            var orders = await _orders.GetAllAsync(cancellationToken);
            return orders.Any(o => o.UserId == userId && o.Items.Any(i => i.ImageId == imageId));
        }
    }
}