using System.Security.Claims;
using ImagesShop.Application.DTOs;
using ImagesShop.Application.Interfaces.IRepositories;
using ImagesShop.Application.Interfaces.IServices;
using ImagesShop.Domain.Entities;
using ImagesShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImagesShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _user;
        private readonly IUserTransactionRepository _transactions;

        public UsersController(IUserService user, IUserTransactionRepository transactions)
        {
            _user = user;
            _transactions = transactions;
        }

        [HttpGet(Name = "GetAllUsers")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var list = await _user.GetAllAsync(cancellationToken);
            var dto = list.Select(MapToDto);
            return Ok(dto);
        }

        [HttpGet("{id:guid}", Name = "GetUserById")]
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _user.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("me", Name = "GetCurrentUser")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            var user = await _user.GetByIdAsync(userId, cancellationToken);
            if (user is null) return Unauthorized();

            return Ok(MapToDto(user));
        }

        [HttpPost("topup", Name = "TopUpBalance")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequestDTO request, CancellationToken cancellationToken)
        {
            if (request is null || request.Amount <= 0) return BadRequest("Amount must be positive");

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            var user = await _user.GetByIdAsync(userId, cancellationToken);
            if (user is null) return Unauthorized();

            var before = user.Balance;
            user.Balance += request.Amount;
            await _user.UpdateAsync(user, cancellationToken);

            await _transactions.AddAsync(new UserTransaction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type = UserTransactionType.TopUp,
                Amount = request.Amount,
                BalanceBefore = before,
                BalanceAfter = user.Balance,
                CreatedAt = DateTime.UtcNow,
                OrderId = null,
                Status = UserTransactionStatus.Success
            }, cancellationToken);

            return Ok(new { balance = user.Balance });
        }

        [HttpPatch("me", Name = "UpdateMyName")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyName([FromBody] UpdateNameRequestDTO request, CancellationToken cancellationToken)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Name is required");

            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            if (!Guid.TryParse(sub, out var userId)) return Unauthorized();

            await _user.UpdateNameAsync(userId, request.Name, cancellationToken);
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

        private static User MapToEntity(UserDTO dto) => new User
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Balance = dto.Balance,
            PasswordHash = string.Empty,
            Role = dto.Role
        };
    }
}
