
using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _user;

        public UsersController(IUserService user)
        {
            _user = user;
        }

        [HttpGet(Name = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _user.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var user = await _user.GetByIdAsync(id, cancellationToken);
            return user is null ? NotFound() : Ok(MapToDto(user));
        }

        [HttpPost(Name = "AddUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] UserDTO dto, CancellationToken cancellationToken)
        {
            var entity = MapToEntity(dto);
            var created = await _user.CreateAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, MapToDto(created));
        }

        [HttpPut("{id:guid}", Name = "ChangeUserById")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserDTO dto, CancellationToken cancellationToken)
        {
            if (id != dto.Id) return BadRequest();
            var entity = MapToEntity(dto);
            await _user.UpdateAsync(entity, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteUserById")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _user.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        private static UserDTO MapToDto(User user) => new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Balance = user.Balance,
            WishlistIds = user.Wishlist?.Select(image => image.Id).ToList() ?? new List<Guid>()
        };

        private static User MapToEntity(UserDTO dto) => new User
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Balance = dto.Balance,
            PasswordHash = string.Empty
        };
    }
}
