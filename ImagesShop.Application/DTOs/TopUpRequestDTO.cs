using System.ComponentModel.DataAnnotations;

namespace ImagesShop.Application.DTOs
{
    public class TopUpRequestDTO
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }
    }
}
