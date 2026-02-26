using System;
using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs
{
    public class TagDTO
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}
