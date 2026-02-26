using System;
using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs
{
    public class PurchaseHistoryDTO
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public Guid ImageId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ImagePrice { get; set; }

        [Required]
        [StringLength(200)]
        public string ImageTitle { get; set; } = string.Empty;

        public DateTime PurchasedAt { get; set; }
    }
}
