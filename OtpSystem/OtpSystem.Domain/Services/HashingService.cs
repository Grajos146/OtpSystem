using System;
using System.Security.Cryptography;
using System.Text;

namespace OtpSystem.Domain.Services;

public class HashingService
{
    public static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }
}
