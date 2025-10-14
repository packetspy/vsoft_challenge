using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Application.Services;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16; 
    private const int KeySize = 32; 
    private const int Iterations = 10000;
    private readonly IConfiguration _configuration;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    public PasswordService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashPassword(string? userPassword = null)
    {
        var defaultPassword = _configuration["DefaultPassword"];
        var password = !string.IsNullOrEmpty(userPassword) ? userPassword : defaultPassword;

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
        var hashBytes = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, KeySize);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHashed)
    {
        var hashBytes = Convert.FromBase64String(passwordHashed);
        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);
        var hash = new byte[KeySize];
        Array.Copy(hashBytes, SaltSize, hash, 0, KeySize);
        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
        return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
    }

}
