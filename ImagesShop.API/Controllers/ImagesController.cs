using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _images;

        public ImagesController(IImageService images)
        {
            _images = images;
        }

        [HttpGet(Name = "GetAllImages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _images.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetImageById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var img = await _images.GetByIdAsync(id, cancellationToken);
            return img is null ? NotFound() : Ok(MapToDto(img));
        }

        [HttpPost(Name = "AddImage")]
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
    }
}