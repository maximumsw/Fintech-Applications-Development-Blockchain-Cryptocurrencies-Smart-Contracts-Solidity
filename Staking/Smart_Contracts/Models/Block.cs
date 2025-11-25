using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Smart_Contracts.Models;

/// <summary>
/// Блок у блокчейні - містить транзакції та зв'язок з попереднім блоком
/// </summary>
public class Block
{
    /// <summary>
    /// Індекс блоку в ланцюгу (висота блоку)
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Мітка часу створення блоку
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Список транзакцій у блоці
    /// </summary>
    public List<Transaction> Transactions { get; set; }

    /// <summary>
    /// Хеш попереднього блоку (забезпечує незмінність ланцюга)
    /// </summary>
    public string PreviousHash { get; set; }

    /// <summary>
    /// Хеш поточного блоку
    /// </summary>
    public string Hash { get; set; }

    /// <summary>
    /// Nonce - число для proof-of-work алгоритму
    /// </summary>
    public int Nonce { get; set; }

    public Block(int index, List<Transaction> transactions, string previousHash = "")
    {
        Index = index;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Transactions = transactions;
        PreviousHash = previousHash;
        Hash = string.Empty;
        Nonce = 0;
    }

    public string CalculateHash()
    {
        var transactionData = JsonSerializer.Serialize(Transactions);
        var rawData = $"{Index}{Timestamp}{transactionData}{PreviousHash}{Nonce}";

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Майнить блок - знаходить хеш, що відповідає заданій складності
    /// </summary>
    /// <param name="difficulty">Кількість нулів на початку хешу</param>
    public void MineBlock(int difficulty)
    {
        // Створюємо цільовий шаблон: "000..." залежно від складності
        var target = new string('0', difficulty);

        // Перебираємо nonce, поки не знайдемо підходящий хеш
        while (!Hash.StartsWith(target))
        {
            Nonce++;
            Hash = CalculateHash();
        }
    }
}
