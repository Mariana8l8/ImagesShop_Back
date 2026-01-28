using System;

namespace ImagesShop.Application.DTOs
{
    public class PurchaseHistoryDTO
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public Guid ImageId { get; set; }

        public decimal ImagePrice { get; set; }

        public string ImageTitle { get; set; } = string.Empty;

        public DateTime PurchasedAt { get; set; }
    }
}
