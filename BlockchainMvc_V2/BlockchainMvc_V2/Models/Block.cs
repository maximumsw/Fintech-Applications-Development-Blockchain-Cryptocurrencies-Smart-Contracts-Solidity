using System.Security.Cryptography;
using System.Text;

namespace BlockchainMvc_V2.Models;

public class Block
{
    public int Index { get; set; }
    public string Data { get; set; } = "";
    public string PrevHash { get; set; } = "";
    public string Hash { get; set; } = "";
    public DateTime Timestamp { get; set; }

    // === NEW FIELDS ===
    public string PublicKeyPem { get; set; } = "";
    public string SignatureBase64 { get; set; } = "";

    public Block()
    {
        Timestamp = DateTime.UtcNow;
    }

    public string CalculateHash()
    {
        using var sha256 = SHA256.Create();
        var rawData = $"{Index}{Data}{PrevHash}{Timestamp:O}";
        var bytes = Encoding.UTF8.GetBytes(rawData);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    public bool HasSignature => 
        !string.IsNullOrWhiteSpace(SignatureBase64) &&
        !string.IsNullOrWhiteSpace(PublicKeyPem);
}