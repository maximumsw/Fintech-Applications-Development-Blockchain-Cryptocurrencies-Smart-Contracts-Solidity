using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Send_Block_Сustom_BlockChain_Logic.Models;

public class Block
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
    public string PrevHash { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public int Nonce { get; set; }
    public int Difficulty { get; set; }
    public string MinedBy { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string MinerPublicKey { get; set; } = string.Empty;

    public Block()
    {
        Timestamp = DateTime.UtcNow;
    }

    public Block(int index, List<Transaction> transactions, string prevHash, int difficulty)
    {
        Index = index;
        Timestamp = DateTime.UtcNow;
        Transactions = transactions ?? new List<Transaction>();
        PrevHash = prevHash;
        Difficulty = difficulty;
        Nonce = 0;
    }

    public string ComputeHash()
    {
        var blockData = $"{Index}{Timestamp:O}{PrevHash}{Nonce}{Difficulty}{MinedBy}";

        // Додати дані транзакції
        foreach (var tx in Transactions.OrderBy(t => t.Timestamp))
        {
            blockData += $"{tx.From}{tx.To}{tx.Amount}{tx.Timestamp:O}";
        }

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(blockData));
        return Convert.ToBase64String(hash);
    }

    public void Mine(Wallet minerWallet)
    {
        MinedBy = minerWallet.Address;
        MinerPublicKey = minerWallet.PublicKey;

        var target = new string('0', Difficulty);

        while (true)
        {
            Hash = ComputeHash();
            var hashHex = BitConverter.ToString(Convert.FromBase64String(Hash))
                .Replace("-", "").ToLower();

            if (hashHex.StartsWith(target))
            {
                break;
            }

            Nonce++;
        }

        // Підписати блок
        var blockData = Encoding.UTF8.GetBytes(Hash);
        var signature = minerWallet.Sign(blockData);
        Signature = Convert.ToBase64String(signature);
    }

    public bool HasValidProof()
    {
        var target = new string('0', Difficulty);
        var computedHash = ComputeHash();

        if (computedHash != Hash)
            return false;

        var hashHex = BitConverter.ToString(Convert.FromBase64String(Hash))
            .Replace("-", "").ToLower();

        return hashHex.StartsWith(target);
    }

    public bool Verify()
    {
        // Генезис-блок не має підпису
        if (Index == 0) return true;

        if (string.IsNullOrEmpty(Signature) || string.IsNullOrEmpty(MinerPublicKey))
            return false;

        try
        {
            var blockData = Encoding.UTF8.GetBytes(Hash);
            var signature = Convert.FromBase64String(Signature);
            return Wallet.Verify(MinerPublicKey, blockData, signature);
        }
        catch
        {
            return false;
        }
    }

    public bool IsValid()
    {
        // Перевірити хеш
        if (Hash != ComputeHash())
            return false;

        // Перевірити підтвердження роботи
        if (!HasValidProof())
            return false;

        // Перевірити підпис
        if (!Verify())
            return false;

        // Перевірити всі транзакції
        foreach (var tx in Transactions)
        {
            if (!tx.IsValid())
                return false;
        }

        return true;
    }
}
