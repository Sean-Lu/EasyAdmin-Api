using System.Security.Cryptography;
using System.Text;
using EasyAdmin.Infrastructure.Wrapper;
using Microsoft.Extensions.Configuration;

namespace EasyAdmin.Application.Services;

/// <summary>
/// 分享密码保护器
/// </summary>
public class SharePasswordProtector
{
    private readonly byte[] key;

    public SharePasswordProtector(IConfiguration configuration)
    {
        var secret = configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ExplicitException("未配置分享密码保护密钥");
        }
        key = SHA256.HashData(Encoding.UTF8.GetBytes($"{secret}:easyadmin-share-password-v1"));
    }

    public string Encrypt(string password)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintext = Encoding.UTF8.GetBytes(password);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, result, nonce.Length + tag.Length, ciphertext.Length);
        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        try
        {
            var data = Convert.FromBase64String(ciphertext);
            if (data.Length < 28)
            {
                throw new CryptographicException();
            }
            var nonce = data[..12];
            var tag = data[12..28];
            var encrypted = data[28..];
            var plaintext = new byte[encrypted.Length];
            using var aes = new AesGcm(key, tag.Length);
            aes.Decrypt(nonce, encrypted, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (Exception exception) when (exception is FormatException or CryptographicException)
        {
            throw new ExplicitException("分享密码数据无效");
        }
    }

    public bool IsMatch(string password, string ciphertext)
    {
        var expected = Encoding.UTF8.GetBytes(Decrypt(ciphertext));
        var actual = Encoding.UTF8.GetBytes(password);
        return expected.Length == actual.Length && CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}