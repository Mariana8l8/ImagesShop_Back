using System;
using System.Collections.Generic;
using ImagesShop.Domain.Enums;

namespace ImagesShop.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string PasswordSalt { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Image> Wishlist { get; set; } = new List<Image>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}