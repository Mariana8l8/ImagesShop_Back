namespace ImagesShop.Application.DTOs
{
    public class CartItemDTO
    {
        public Guid ImageId { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string WatermarkedUrl { get; set; } = string.Empty;
    }
}
