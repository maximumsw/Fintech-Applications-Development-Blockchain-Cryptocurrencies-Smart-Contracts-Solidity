using System.Security.Cryptography;
using System.Text;

namespace Send_Block_Сustom_BlockChain_Logic.Models;

public class Wallet
{
    public string Address { get; set; }
    public string PrivateKey { get; set; }
    public string PublicKey { get; set; }

    public Wallet()
    {
        using var rsa = RSA.Create(2048);
        PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
        PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        Address = ComputeAddress(PublicKey);
    }

    private static string ComputeAddress(string publicKey)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(publicKey));
        return Convert.ToBase64String(hash)[..16];
    }

    public byte[] Sign(byte[] data)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(PrivateKey), out _);
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public static bool Verify(string publicKey, byte[] data, byte[] signature)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }
}
