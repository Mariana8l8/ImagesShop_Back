namespace ImagesShop.Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid ImageId { get; set; }
        public Image Image { get; set; } = null!;
    }
}
