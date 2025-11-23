using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Send_Block_Сustom_BlockChain_Logic.Models;

public class Transaction
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string Signature { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;

    public Transaction()
    {
        Timestamp = DateTime.UtcNow;
    }

    public Transaction(string from, string to, decimal amount)
    {
        From = from;
        To = to;
        Amount = amount;
        Timestamp = DateTime.UtcNow;
    }

    public byte[] GetBytes()
    {
        var data = $"{From}{To}{Amount}{Timestamp:O}";
        return Encoding.UTF8.GetBytes(data);
    }

    public void Sign(Wallet wallet)
    {
        PublicKey = wallet.PublicKey;
        var signature = wallet.Sign(GetBytes());
        Signature = Convert.ToBase64String(signature);
    }

    public bool IsValid()
    {
        // Транзакції Coinbase (винагороди за майнінг) не потребують підписів
        if (From == "COINBASE") return true;

        if (string.IsNullOrEmpty(Signature) || string.IsNullOrEmpty(PublicKey))
            return false;

        try
        {
            var signature = Convert.FromBase64String(Signature);
            return Wallet.Verify(PublicKey, GetBytes(), signature);
        }
        catch
        {
            return false;
        }
    }
}
