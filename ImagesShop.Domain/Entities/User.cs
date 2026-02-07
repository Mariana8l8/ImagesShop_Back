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

        public bool EmailVerified { get; set; }

        public DateTime? EmailVerifiedAt { get; set; }

        public decimal Balance { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Image> Wishlist { get; set; } = new List<Image>();

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public ICollection<UserTransaction> Transactions { get; set; } = new List<UserTransaction>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<EmailVerificationCode> EmailVerificationCodes { get; set; } = new List<EmailVerificationCode>();
    }
}