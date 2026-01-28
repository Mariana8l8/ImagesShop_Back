using ImagesShop.Domain.Enums;

namespace ImagesShop.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public User? User { get; set; }

        public DateTime CreatedAt { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public string Currency { get; set; } = "USD";

        public string? Notes { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}