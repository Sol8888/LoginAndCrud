using System.Security.Cryptography;

namespace LoginAndCrud.Infrastructure.Security;

public static class PasswordHasher
{
    public static (byte[] hash, byte[] salt) Hash(string password, int saltSize = 16, int iter = 100_000)
    {
        var salt = RandomNumberGenerator.GetBytes(saltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, 32);
        return (hash, salt);
    }

    public static bool Verify(string password, byte[] hash, byte[] salt, int iter = 100_000)
    {
        var test = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(test, hash);
    }
}
