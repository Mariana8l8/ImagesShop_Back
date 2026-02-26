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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet(Name = "GetAllUsers")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var users = await _userService.GetAllAsync(cancellationToken);
            var usersDto = users.Select(user => MapToDto(user));
            
            return Ok(usersDto);
        }

        [HttpGet("{id:guid}", Name = "GetUserById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            
            if (user is null)
            {
                return NotFound();
            }

            return Ok(MapToDto(user));
        }

        [HttpPost(Name = "AddUser")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] UserDTO userDto, CancellationToken cancellationToken)
        {
            var userEntity = MapToEntity(userDto);
            var createdUser = await _userService.CreateAsync(userEntity, cancellationToken);
            
            return CreatedAtAction(nameof(Get), new { id = createdUser.Id }, MapToDto(createdUser));
        }

        [HttpPut("{id:guid}", Name = "ChangeUserById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserDTO userDto, CancellationToken cancellationToken)
        {
            if (id != userDto.Id)
            {
                return BadRequest("User ID mismatch.");
            }

            var userEntity = MapToEntity(userDto);
            await _userService.UpdateAsync(userEntity, cancellationToken);
            
            return NoContent();
        }

        [HttpDelete("{id:guid}", Name = "DeleteUserById")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _userService.DeleteAsync(id, cancellationToken);
            
            return NoContent();
        }

        [HttpGet("me", Name = "GetCurrentUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();
            var user = await _userService.GetByIdAsync(userId, cancellationToken);
            
            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(MapToDto(user));
        }

        [HttpPost("topup", Name = "TopUpBalance")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequestDTO request, CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();
            var newBalance = await _userService.TopUpAsync(userId, request.Amount, cancellationToken);
            
            return Ok(new { balance = newBalance });
        }

        [HttpPatch("me", Name = "UpdateMyName")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyName([FromBody] UpdateNameRequestDTO request, CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();
            await _userService.UpdateNameAsync(userId, request.Name, cancellationToken);
            
            return NoContent();
        }

        private static UserDTO MapToDto(User user) => new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Balance = user.Balance,
            WishlistIds = user.Wishlist?.Select(image => image.Id).ToList() ?? new List<Guid>(),
            Role = user.Role
        };

        private static User MapToEntity(UserDTO userDto) => new User
        {
            Id = userDto.Id == Guid.Empty ? Guid.NewGuid() : userDto.Id,
            Name = userDto.Name,
            Email = userDto.Email,
            Balance = userDto.Balance,
            PasswordHash = string.Empty,
            Role = userDto.Role
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
