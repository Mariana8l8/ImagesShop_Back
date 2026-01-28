using System;
using System.Collections.Generic;

namespace ImagesShop.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public ICollection<Guid> WishlistIds { get; set; } = new List<Guid>();
    }
}
