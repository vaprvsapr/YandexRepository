using System.Security.Cryptography;
using System.Text;

namespace EventManager.Application.Security;

public static class PasswordManager
{
    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool CheckPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }
}
