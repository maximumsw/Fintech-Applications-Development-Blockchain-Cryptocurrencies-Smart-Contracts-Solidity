using System.Security.Cryptography;
using System.Text;
using BlockchainMvc_V2.Models;

namespace BlockchainMvc_V2.Services;

public class Blockchain
{
    public List<Block> Chain { get; set; } = new();

    public Blockchain()
    {
        // Genesis block (без ключів)
        var genesis = new Block
        {
            Index = 0,
            Data = "Genesis Block",
            PrevHash = "0"
        };
        genesis.Hash = genesis.CalculateHash();
        Chain.Add(genesis);
    }

    public Block GetLastBlock() => Chain.Last();

    public void AddBlock(string data, string privateKeyPem)
    {
        var prev = GetLastBlock();

        var block = new Block
        {
            Index = prev.Index + 1,
            Data = data,
            PrevHash = prev.Hash
        };

        block.Hash = block.CalculateHash();

        // ==== SIGNATURE PART ====
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var publicKeyPem = ExportPublicKeyPem(rsa);
        block.PublicKeyPem = publicKeyPem;

        var hashBytes = Encoding.UTF8.GetBytes(block.Hash);
        var signature = rsa.SignData(hashBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        block.SignatureBase64 = Convert.ToBase64String(signature);

        Chain.Add(block);
    }

    public void UpdateBlock(int index, string newData)
    {
        var block = Chain.FirstOrDefault(b => b.Index == index);
        if (block == null) return;

        block.Data = newData;
        block.Hash = block.CalculateHash();

        // ПІДПИС СТАЄ НЕКОРЕКТНИМ
        block.SignatureBase64 = "";
        block.PublicKeyPem = "";
    }

    public bool ValidateSignature(Block block)
    {
        if (!block.HasSignature) return false;

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(block.PublicKeyPem);

            var hashBytes = Encoding.UTF8.GetBytes(block.Hash);
            var signatureBytes = Convert.FromBase64String(block.SignatureBase64);

            return rsa.VerifyData(hashBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    private string ExportPublicKeyPem(RSA rsa)
    {
        var pub = rsa.ExportSubjectPublicKeyInfo();
        var base64 = Convert.ToBase64String(pub, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN PUBLIC KEY-----\n{base64}\n-----END PUBLIC KEY-----";
    }
}
