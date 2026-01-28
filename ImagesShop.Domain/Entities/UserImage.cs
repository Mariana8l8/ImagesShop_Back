namespace ImagesShop.Domain.Entities
{
    public class UserImage
    {
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public Guid ImageId { get; set; }

        public Image Image { get; set; } = null!;
    }
}