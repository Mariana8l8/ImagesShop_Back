using System;
using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs
{
    public class OrderItemDTO
    {
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ImageId { get; set; }
    }
}
