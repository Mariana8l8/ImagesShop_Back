using System.Security.Cryptography;

namespace ImagesShop.Application.Helpers
{
    public static class PasswordHasher
    {
        public static (string hash, string salt) HashPassword(string password, int iterations = 100_000, int saltSize = 16, int hashSize = 32)
        {
            var salt = RandomNumberGenerator.GetBytes(saltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(hashSize);
            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool Verify(string password, string storedHashBase64, string storedSaltBase64, int iterations = 100_000, int hashSize = 32)
        {
            var salt = Convert.FromBase64String(storedSaltBase64);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(hashSize);
            var storedHash = Convert.FromBase64String(storedHashBase64);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }
    }
}