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
    public class UserTransactionsController : ControllerBase
    {
        private readonly IUserTransactionService _transactions;

        public UserTransactionsController(IUserTransactionService transactions)
        {
            _transactions = transactions;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var items = await _transactions.GetMyAsync(userId.Value, cancellationToken);
            return Ok(items.Select(MapToDto));
        }

        private static UserTransactionDTO MapToDto(UserTransaction tx) => new()
        {
            Id = tx.Id,
            Type = tx.Type,
            Amount = tx.Amount,
            BalanceBefore = tx.BalanceBefore,
            BalanceAfter = tx.BalanceAfter,
            CreatedAtUtc = tx.CreatedAt,
            OrderId = tx.OrderId,
            Status = tx.Status
        };

        private Guid? GetUserId()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}
