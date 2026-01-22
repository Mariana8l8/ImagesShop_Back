using System;
using System.Collections.Generic;

namespace ImagesShop.API.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Order? Order { get; set; }

        public Guid ImageId { get; set; }

        public Image? Image { get; set; }
    }
}
