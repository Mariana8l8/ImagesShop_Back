using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet(Name = "GetAllTags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _tagService.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetTagById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var tag = await _tagService.GetByIdAsync(id, cancellationToken);
            return tag is null ? NotFound() : Ok(MapToDto(tag));
        }

        [HttpPost(Name = "AddTag")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] TagDTO dto, CancellationToken cancellationToken)
        {
            var entity = MapToEntity(dto);
            var created = await _tagService.CreateAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id:guid}", Name = "ChangeTagById")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] TagDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.Id) return BadRequest();
            var entity = MapToEntity(dto);
            await _tagService.UpdateAsync(entity, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteTagById")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _tagService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        private static TagDTO MapToDto(Tag tag) => new TagDTO
        {
            Id = tag.Id,
            Name = tag.Name
        };

        private static Tag MapToEntity(TagDTO dto) => new Tag
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name
        };
    }
}
