using System;

namespace ImagesShop.Domain.Entities
{
    public class EmailVerificationCode
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public User? User { get; set; }

        public string CodeHash { get; set; } = string.Empty;

        public string CodeSalt { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UsedAt { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsActive => UsedAt is null && !IsExpired;
    }
}
