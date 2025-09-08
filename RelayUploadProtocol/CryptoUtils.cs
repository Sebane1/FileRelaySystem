using System.Security.Cryptography;
namespace RelayClientProtocol
{
    public static class CryptoUtils
    {
        public static byte[] DeriveKeyFromPassword(string password, byte[] salt, int keySize = 32, int iterations = 100_000)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(keySize); // 32 bytes = 256-bit AES key
        }
    }
}
