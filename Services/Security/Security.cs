using System.Security.Cryptography;
using System.Text; 
namespace MangaApplication.Services;
public static class Security 
{
    const int keySize = 64;
    const int iterations = 100000;
    
    public static string HashPassword(string password)
    {
        var hashAlgo = HashAlgorithmName.SHA512;
        byte[] salt = RandomNumberGenerator.GetBytes(keySize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgo, keySize);
        return Convert.ToBase64String(salt) + "$" + Convert.ToHexString(hash);
    }

    private static byte[] GetPasswordSalt(string password)
    {
        byte[] salt = Convert.FromBase64String(password.Split('$')[0]);
        return salt;
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var salt = GetPasswordSalt(hashedPassword);
        var hashAlgo = HashAlgorithmName.SHA512;
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgo, keySize);
        hashedPassword = string.Join('$', hashedPassword.Split('$')[1..]);
        return CryptographicOperations.FixedTimeEquals(hash, Convert.FromHexString(hashedPassword));
    }

    public static void CheckValidPassword(string password)
    {
        bool hasDigit = false;
        bool hasLowerCase = false;
        bool hasUpperCase = false;
        int minLength = 6;
        if (password is null || password.Length == 0) throw new Exception("Password must be provided.");
        if (password.Length < minLength) throw new Exception("Password should have at least 6 characters.");
        for (int i = 0; i < password.Length; i++)
        {
            if (Char.IsNumber(password[i]))
            {
                hasDigit = true;
            }
            else if (password[i] == Char.ToLower(password[i]))
            {
                hasLowerCase = true;
            }
            else if (password[i] == Char.ToUpper(password[i]))
            {
                hasUpperCase = true;
            }
        }
        if (!hasDigit) throw new Exception("Password should have a digit.");
        if (!hasLowerCase) throw new Exception("Password should have a lower case character.");
        if (!hasUpperCase) throw new Exception("Password should have an upper case character.");
    }
}