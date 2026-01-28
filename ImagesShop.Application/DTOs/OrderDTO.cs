using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.DTOs
{
    public class OrderDTO
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public OrderStatus Status { get; set; }

        public decimal TotalAmount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public ICollection<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }
}
