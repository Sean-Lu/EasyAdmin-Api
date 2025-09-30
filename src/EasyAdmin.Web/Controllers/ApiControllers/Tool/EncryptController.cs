using System.Security.Cryptography;
using EasyAdmin.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Sean.Utility.Security;
using Sean.Utility.Security.Provider;

namespace EasyAdmin.Web.Controllers;

/// <summary>
/// 加密\解密
/// </summary>
public class EncryptController(
    ILogger<EncryptController> logger
    ) : BaseApiController
{
    /// <summary>
    /// MD5加密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Md5Encrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var upper = body?["upper"]?.Value<bool>() ?? true;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("加密内容不能为空！");
        }

        var md5 = new HashCryptoProvider().MD5(data);
        return await Task.FromResult(Success<object>(new
        {
            md5 = upper ? md5?.ToUpper() : md5?.ToLower()
        }));
    }

    /// <summary>
    /// Base64编码
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Base64Encrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("编码内容不能为空！");
        }

        var encrypt = new Base64CryptoProvider().Encrypt(data);
        return await Task.FromResult(Success<object>(new
        {
            encrypt
        }));
    }

    /// <summary>
    /// Base64解码
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Base64Decrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("解码内容不能为空！");
        }

        var decrypt = new Base64CryptoProvider().Decrypt(data);
        return await Task.FromResult(Success<object>(new
        {
            decrypt
        }));
    }

    /// <summary>
    /// AES加密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> AesEncrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("加密内容不能为空！");
        }

        var aes = new AESCryptoProvider { Key = key };
        var encrypt = aes.Encrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            encrypt
        }));
    }

    /// <summary>
    /// AES解密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> AesDecrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("解密内容不能为空！");
        }

        var aes = new AESCryptoProvider { Key = key };
        var decrypt = aes.Decrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            decrypt
        }));
    }

    /// <summary>
    /// DES加密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> DesEncrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("加密内容不能为空！");
        }

        var des = new DESCryptoProvider { Key = key };
        var encrypt = des.Encrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            encrypt
        }));
    }

    /// <summary>
    /// DES解密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> DesDecrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("解密内容不能为空！");
        }

        var des = new DESCryptoProvider { Key = key };
        var decrypt = des.Decrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            decrypt
        }));
    }

    /// <summary>
    /// 3DES加密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> TripleDesEncrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("加密内容不能为空！");
        }

        var tripleDes = new TripleDESCryptoProvider { Key = key };
        var encrypt = tripleDes.Encrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            encrypt
        }));
    }

    /// <summary>
    /// 3DES解密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> TripleDesDecrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("解密内容不能为空！");
        }

        var tripleDes = new TripleDESCryptoProvider { Key = key };
        var decrypt = tripleDes.Decrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            decrypt
        }));
    }

    /// <summary>
    /// RC2加密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Rc2Encrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("加密内容不能为空！");
        }

        var rc2 = new RC2CryptoProvider { Key = key };
        var encrypt = rc2.Encrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            encrypt
        }));
    }

    /// <summary>
    /// RC2解密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Rc2Decrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("解密内容不能为空！");
        }

        var rc2 = new RC2CryptoProvider { Key = key };
        var decrypt = rc2.Decrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            decrypt
        }));
    }

    /// <summary>
    /// RC4加密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Rc4Encrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("加密内容不能为空！");
        }

        var rc4 = new RC4CryptoProvider();
        var encrypt = rc4.Encrypt(data, key);
        return await Task.FromResult(Success<object>(data: new
        {
            encrypt
        }));
    }

    /// <summary>
    /// RC4解密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> Rc4Decrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("解密内容不能为空！");
        }

        var rc4 = new RC4CryptoProvider();
        var decrypt = rc4.Decrypt(data, key);
        return await Task.FromResult(Success<object>(data: new
        {
            decrypt
        }));
    }

    /// <summary>
    /// RSA加密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> RsaEncrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("加密内容不能为空！");
        }

        var rsa = new RSACryptoProvider
        {
            DefaultKeyType = RsaKeyType.OpenSsl,
            DefaultKeyEncodeMode = EncodeMode.Base64,
            DefaultEncryptionEncodeMode = EncodeMode.Hex,
            DefaultHashAlgorithmName = HashAlgorithmName.SHA1,
            PublickKey = key
        };
        var encrypt = rsa.Encrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            encrypt
        }));
    }

    /// <summary>
    /// RSA解密
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult<object>> RsaDecrypt([FromBody] JObject? body)
    {
        var data = body?["data"]?.Value<string>() ?? default;
        var key = body?["key"]?.Value<string>() ?? default;
        if (string.IsNullOrWhiteSpace(data))
        {
            return Fail<object>("解密内容不能为空！");
        }

        var rsa = new RSACryptoProvider
        {
            DefaultKeyType = RsaKeyType.OpenSsl,
            DefaultKeyEncodeMode = EncodeMode.Base64,
            DefaultEncryptionEncodeMode = EncodeMode.Hex,
            DefaultHashAlgorithmName = HashAlgorithmName.SHA1,
            PrivatekKey = key
        };
        var decrypt = rsa.Decrypt(data);
        return await Task.FromResult(Success<object>(data: new
        {
            decrypt
        }));
    }
}