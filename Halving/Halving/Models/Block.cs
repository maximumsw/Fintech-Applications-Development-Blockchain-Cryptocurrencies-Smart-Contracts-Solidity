namespace Halving.Models;

/// <summary>
/// Блок в блокчейні
/// </summary>
public class Block
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }

    // Хеш попереднього блоку для зв'язку ланцюга
    public string PreviousHash { get; set; }

    // Хеш поточного блоку (результат майнінгу)
    public string Hash { get; set; }

    // Число для proof-of-work
    public int Nonce { get; set; }

    // Складність майнінгу (кількість нулів на початку хешу)
    public int Difficulty { get; set; }

    // Список транзакцій в блоці
    public List<Transaction> Transactions { get; set; }

    // Час майнінгу в мілісекундах
    public long MiningDurationMs { get; set; }

    public Block()
    {
        Transactions = new List<Transaction>();
        PreviousHash = string.Empty;
        Hash = string.Empty;
    }

    public Block(int index, DateTime timestamp, string previousHash, int difficulty)
    {
        Index = index;
        Timestamp = timestamp;
        PreviousHash = previousHash;
        Difficulty = difficulty;
        Transactions = new List<Transaction>();
        Hash = string.Empty;
        Nonce = 0;
        MiningDurationMs = 0;
    }
}
