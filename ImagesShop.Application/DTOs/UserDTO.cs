using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ImagesShop.Domain.Enums;

namespace ImagesShop.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public ICollection<Guid> WishlistIds { get; set; } = new List<Guid>();
    }
}
