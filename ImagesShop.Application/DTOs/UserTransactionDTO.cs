using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.DTOs
{
    public class UserTransactionDTO
    {
        public Guid Id { get; set; }

        public UserTransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public Guid? OrderId { get; set; }

        public UserTransactionStatus Status { get; set; }
    }
}
