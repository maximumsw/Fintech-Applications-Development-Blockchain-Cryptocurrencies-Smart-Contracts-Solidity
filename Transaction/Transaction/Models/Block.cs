using System.Security.Cryptography;
using System.Text;

namespace Transaction.Models;

/// <summary>
/// Клас блоку в блокчейні
/// Блок містить список транзакцій та зв'язується з попереднім блоком
/// </summary>
public class Block
{
    // Індекс блоку в ланцюзі (висота блоку)
    public int Index { get; set; }

    // Час створення блоку
    public DateTime Timestamp { get; set; }

    // Список транзакцій в блоці
    public List<BlockchainTransaction> Transactions { get; set; }

    // Хеш попереднього блоку (зв'язок з попереднім блоком)
    public string PreviousHash { get; set; }

    // Хеш поточного блоку
    public string Hash { get; set; }

    // Nonce - число, яке майнер підбирає для знаходження правильного хешу
    public long Nonce { get; set; }

    // Складність майнінгу (кількість нулів на початку хешу)
    public int Difficulty { get; set; }

    // Адреса майнера, який створив блок
    public string MinerAddress { get; set; }

    /// <summary>
    /// Конструктор створює новий блок
    /// </summary>
    public Block(int index, List<BlockchainTransaction> transactions, string previousHash, string minerAddress)
    {
        Index = index;
        Timestamp = DateTime.UtcNow;
        Transactions = transactions;
        PreviousHash = previousHash;
        MinerAddress = minerAddress;
        Nonce = 0;
        Difficulty = 2; // Кількість нулів на початку хешу

        // Розраховуємо хеш блоку
        Hash = CalculateHash();
    }

    /// <summary>
    /// Розрахунок хешу блоку
    /// Хеш залежить від всіх даних блоку
    /// </summary>
    public string CalculateHash()
    {
        // Збираємо всі транзакції в один рядок
        string transactionsData = string.Join("", Transactions.Select(t => t.TransactionId));

        // Створюємо рядок з усіх даних блоку
        string rawData = $"{Index}{Timestamp:O}{transactionsData}{PreviousHash}{Nonce}{MinerAddress}";

        // Хешуємо дані
        using (var sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(rawData);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }

    /// <summary>
    /// Майнінг блоку - пошук правильного Nonce
    /// Майнер підбирає Nonce поки хеш не почнеться з потрібної кількості нулів
    /// </summary>
    public void MineBlock()
    {
        // Створюємо рядок з потрібною кількістю нулів
        string target = new string('0', Difficulty);

        Console.WriteLine($"\n⛏️  Починаємо майнінг блоку {Index}...");
        Console.WriteLine($"   Складність: {Difficulty} (потрібно {Difficulty} нулів на початку)");

        var startTime = DateTime.Now;

        // Підбираємо Nonce поки не знайдемо правильний хеш
        while (!Hash.StartsWith(target))
        {
            Nonce++;
            Hash = CalculateHash();

            // Показуємо прогрес кожні 100000 спроб
            if (Nonce % 100000 == 0)
            {
                Console.WriteLine($"   Спроба #{Nonce}: {Hash[..32]}...");
            }
        }

        var endTime = DateTime.Now;
        var duration = (endTime - startTime).TotalSeconds;

        Console.WriteLine($"✓ Блок змайнено!");
        Console.WriteLine($"   Nonce: {Nonce}");
        Console.WriteLine($"   Хеш: {Hash}");
        Console.WriteLine($"   Час майнінгу: {duration:F2} секунд");
    }

    /// <summary>
    /// Перевірка валідності блоку
    /// </summary>
    public bool IsValid()
    {
        // Перевіряємо всі транзакції в блоці
        foreach (var transaction in Transactions)
        {
            if (!transaction.IsValid())
            {
                Console.WriteLine($"✗ Блок містить невалідну транзакцію: {transaction.TransactionId}");
                return false;
            }
        }

        // Перевіряємо що хеш правильний
        string calculatedHash = CalculateHash();
        if (Hash != calculatedHash)
        {
            Console.WriteLine($"✗ Хеш блоку не співпадає!");
            return false;
        }

        // Перевіряємо складність (хеш має починатись з потрібної кількості нулів)
        string target = new string('0', Difficulty);
        if (!Hash.StartsWith(target))
        {
            Console.WriteLine($"✗ Блок не задовольняє вимогу складності!");
            return false;
        }

        Console.WriteLine($"✓ Блок {Index} валідний!");
        return true;
    }

    /// <summary>
    /// Додавання винагороди майнеру (coinbase transaction)
    /// </summary>
    public void AddMinerReward(decimal reward)
    {
        // Створюємо спеціальну транзакцію винагороди
        // У неї немає відправника (FromAddress = null), бо монети створюються з нічого
        var coinbase = new BlockchainTransaction("", MinerAddress, reward, 0)
        {
            Signature = null // Транзакції винагороди не потребують підпису
        };

        // Додаємо транзакцію на початок списку
        Transactions.Insert(0, coinbase);

        Console.WriteLine($"✓ Додано винагороду майнеру: {reward} BTC → {MinerAddress}");
    }

    /// <summary>
    /// Вивід інформації про блок
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n╔═══════════════════════════════════════╗");
        sb.AppendLine($"║           БЛОК #{Index,-4}                ║");
        sb.AppendLine("╠═══════════════════════════════════════╣");
        sb.AppendLine($"║ Хеш: {Hash[..32]}... ║");
        sb.AppendLine($"║ Попередній: {PreviousHash[..24]}... ║");
        sb.AppendLine($"║ Час: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC    ║");
        sb.AppendLine($"║ Транзакцій: {Transactions.Count,-4}                  ║");
        sb.AppendLine($"║ Nonce: {Nonce,-10}                    ║");
        sb.AppendLine($"║ Майнер: {MinerAddress[..20]}...     ║");
        sb.AppendLine("╚═══════════════════════════════════════╝");
        return sb.ToString();
    }
}
