using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet(Name = "GetAllTags")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var tags = await _tagService.GetAllAsync(cancellationToken);
            var tagsDto = tags.Select(tag => MapToDto(tag));
            
            return Ok(tagsDto);
        }

        [HttpGet("{id:guid}", Name = "GetTagById")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var tag = await _tagService.GetByIdAsync(id, cancellationToken);
            
            if (tag is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(tag));
        }

        [HttpPost(Name = "AddTag")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] TagDTO tagDto, CancellationToken cancellationToken)
        {
            var tagEntity = MapToEntity(tagDto);
            var createdTag = await _tagService.CreateAsync(tagEntity, cancellationToken);
            
            return CreatedAtAction(nameof(Get), new { id = createdTag.Id }, MapToDto(createdTag));
        }

        [HttpPut("{id:guid}", Name = "ChangeTagById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] TagDTO tagDto, CancellationToken cancellationToken)
        {
            if (id != tagDto.Id)
            {
                return BadRequest("Tag ID mismatch.");
            }

            var tagEntity = MapToEntity(tagDto);
            await _tagService.UpdateAsync(tagEntity, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteTagById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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

        private static Tag MapToEntity(TagDTO tagDto) => new Tag
        {
            Id = tagDto.Id == Guid.Empty ? Guid.NewGuid() : tagDto.Id,
            Name = tagDto.Name
        };
    }
}
