using System;

namespace ImagesShop.Application.DTOs
{
    public class OrderItemDTO
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Guid ImageId { get; set; }
    }
}
