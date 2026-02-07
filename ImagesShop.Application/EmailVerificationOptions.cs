namespace ImagesShop.Application
{
    public class EmailVerificationOptions
    {
        public int CodeLength { get; set; } = 6;

        public int CodeTtlMinutes { get; set; } = 10;

        public int ResendCooldownSeconds { get; set; } = 30;
    }
}
