namespace ImagesShop.Domain.Entities
{
    public class PurchaseHistory
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public Guid ImageId { get; set; }

        public decimal ImagePrice { get; set; } = 0;

        public string ImageTitle { get; set; } = string.Empty;

        public DateTime PurchasedAt { get; set; }
    }
}