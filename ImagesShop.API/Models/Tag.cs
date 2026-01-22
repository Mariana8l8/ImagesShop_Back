using ImagesShop.API.Models;

namespace ImagesShop.API.Models
{
    public class Tag
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
