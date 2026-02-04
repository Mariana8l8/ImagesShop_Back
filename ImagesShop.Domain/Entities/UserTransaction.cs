using ImagesShop.Domain.Enums;

namespace ImagesShop.Domain.Entities
{
    public class UserTransaction
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public UserTransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid? OrderId { get; set; }

        public UserTransactionStatus Status { get; set; }
    }
}
