using System.Security.Cryptography;
using System.Text;

namespace Halving2.Models;

/// <summary>
/// Модель блоку в блокчейні
/// Містить список транзакцій та зв'язок з попереднім блоком через хеш
/// </summary>
public class Block
{
    // Висота (індекс) блоку в ланцюзі
    public int Height { get; set; }

    // Час створення блоку (UTC)
    public DateTime Timestamp { get; set; }

    // Хеш попереднього блоку (для зв'язку блоків в ланцюг)
    public string PreviousHash { get; set; } = string.Empty;

    // Хеш поточного блоку
    public string Hash { get; set; } = string.Empty;

    // Nonce - число, що підбирається для Proof-of-Work
    public int Nonce { get; set; }

    // Складність майнінгу (кількість нулів на початку хешу)
    public int Difficulty { get; set; }

    // Список транзакцій в блоці
    public List<Transaction> Transactions { get; set; } = new();

    // Час, витрачений на майнінг блоку (мілісекунди)
    public long MiningDurationMs { get; set; }

    public Block(int height, string previousHash, List<Transaction> transactions, int difficulty = 2)
    {
        Height = height;
        Timestamp = DateTime.UtcNow;
        PreviousHash = previousHash;
        Transactions = transactions;
        Difficulty = difficulty;
        Nonce = 0;
        Hash = string.Empty;
    }

    /// <summary>
    /// Обчислює SHA256 хеш блоку на основі всіх його даних
    /// </summary>
    public string CalculateHash()
    {
        // Об'єднуємо всі транзакції в один рядок
        var transactionData = string.Join(";", Transactions.Select(t =>
            $"{t.FromAddress}:{t.ToAddress}:{t.Amount}:{t.Fee}"));

        // Формуємо рядок з усіх даних блоку
        var rawData = $"{Height}{Timestamp:O}{PreviousHash}{transactionData}{Nonce}{Difficulty}";

        // Обчислюємо SHA256 хеш
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLower();
    }

    /// <summary>
    /// Виконує майнінг блоку (Proof-of-Work)
    /// Підбирає Nonce так, щоб хеш починався з заданої кількості нулів
    /// </summary>
    public void MineBlock()
    {
        var startTime = DateTime.UtcNow;
        var target = new string('0', Difficulty);

        // Підбираємо Nonce поки не знайдемо хеш з потрібною кількістю нулів
        while (!Hash.StartsWith(target))
        {
            Nonce++;
            Hash = CalculateHash();
        }

        // Зберігаємо час майнінгу
        MiningDurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
    }
}
