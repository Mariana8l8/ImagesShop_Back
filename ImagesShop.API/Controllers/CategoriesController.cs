using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet(Name = "GetAllCategories")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var categories = await _categoryService.GetAllAsync(cancellationToken);
            var categoriesDto = categories.Select(category => MapToDto(category));
            
            return Ok(categoriesDto);
        }

        [HttpGet("{id:guid}", Name = "GetCategoryById")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var category = await _categoryService.GetByIdAsync(id, cancellationToken);
            
            if (category is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(category));
        }

        [HttpPost(Name = "AddCategory")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CategoryDTO categoryDto, CancellationToken cancellationToken)
        {
            var categoryEntity = MapToEntity(categoryDto);
            var createdCategory = await _categoryService.CreateAsync(categoryEntity, cancellationToken);
            
            return CreatedAtAction(nameof(Get), new { id = createdCategory.Id }, MapToDto(createdCategory));
        }

        [HttpPut("{id:guid}", Name = "ChangeCategoryById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryDTO categoryDto, CancellationToken cancellationToken)
        {
            if (id != categoryDto.Id)
            {
                return BadRequest("Category ID mismatch.");
            }

            var categoryEntity = MapToEntity(categoryDto);
            await _categoryService.UpdateAsync(categoryEntity, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteCategoryById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _categoryService.DeleteAsync(id, cancellationToken);
            
            return NoContent();
        }

        private static CategoryDTO MapToDto(Category category) => new CategoryDTO
        {
            Id = category.Id,
            Name = category.Name
        };

        private static Category MapToEntity(CategoryDTO categoryDto) => new Category
        {
            Id = categoryDto.Id == Guid.Empty ? Guid.NewGuid() : categoryDto.Id,
            Name = categoryDto.Name
        };
    }
}
