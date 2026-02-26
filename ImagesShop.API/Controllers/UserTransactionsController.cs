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
        private readonly IUserTransactionService _userTransactionService;

        public UserTransactionsController(IUserTransactionService userTransactionService)
        {
            _userTransactionService = userTransactionService;
        }

        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyTransactions(CancellationToken cancellationToken)
        {
            var userId = GetUserIdOrThrow();

            var userTransactions = await _userTransactionService.GetMyAsync(userId, cancellationToken);
            var userTransactionsDto = userTransactions.Select(transaction => MapToDto(transaction));
            
            return Ok(userTransactionsDto);
        }

        private static UserTransactionDTO MapToDto(UserTransaction userTransaction) => new()
        {
            Id = userTransaction.Id,
            Type = userTransaction.Type,
            Amount = userTransaction.Amount,
            BalanceBefore = userTransaction.BalanceBefore,
            BalanceAfter = userTransaction.BalanceAfter,
            CreatedAtUtc = userTransaction.CreatedAt,
            OrderId = userTransaction.OrderId,
            Status = userTransaction.Status
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
