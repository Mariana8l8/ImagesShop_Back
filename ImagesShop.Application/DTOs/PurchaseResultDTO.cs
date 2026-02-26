namespace ImagesShop.Application.DTOs
{
    public class PurchaseResultDTO
    {
        public Guid OrderId { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
    }
}
