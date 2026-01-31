using System;
using System.Collections.Generic;
using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public ICollection<Guid> WishlistIds { get; set; } = new List<Guid>();
    }
}
