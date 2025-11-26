namespace Transaction.Models;

/// <summary>
/// Mempool (Memory Pool) - пул пам'яті для неопрацьованих транзакцій
/// Це "зала очікування" для транзакцій перед тим, як вони потраплять в блок
/// </summary>
public class Mempool
{
    // Список всіх неопрацьованих транзакцій
    private List<BlockchainTransaction> _pendingTransactions;

    // Максимальний розмір mempool (кількість транзакцій)
    private const int MaxMempoolSize = 1000;

    public Mempool()
    {
        _pendingTransactions = new List<BlockchainTransaction>();
    }

    /// <summary>
    /// Додавання нової транзакції в mempool
    /// </summary>
    public bool AddTransaction(BlockchainTransaction transaction)
    {
        // Перевіряємо чи mempool не переповнений
        if (_pendingTransactions.Count >= MaxMempoolSize)
        {
            Console.WriteLine("✗ Mempool переповнений! Транзакція відхилена.");
            return false;
        }

        // Перевіряємо валідність транзакції перед додаванням
        if (!transaction.IsValid())
        {
            Console.WriteLine("✗ Транзакція невалідна! Не може бути додана в mempool.");
            return false;
        }

        // Перевіряємо чи транзакція вже є в mempool
        if (_pendingTransactions.Any(t => t.TransactionId == transaction.TransactionId))
        {
            Console.WriteLine("✗ Транзакція вже є в mempool!");
            return false;
        }

        // Додаємо транзакцію в mempool
        _pendingTransactions.Add(transaction);
        Console.WriteLine($"✓ Транзакція {transaction.TransactionId[..16]}... додана в mempool");
        Console.WriteLine($"  Поточний розмір mempool: {_pendingTransactions.Count} транзакцій");

        return true;
    }

    /// <summary>
    /// Отримання транзакцій з найвищою комісією для майнера
    /// Майнери обирають транзакції з найбільшою комісією, бо це їх заробіток
    /// </summary>
    public List<BlockchainTransaction> GetTopTransactionsByFee(int count)
    {
        // Сортуємо транзакції за комісією (спадаючий порядок)
        // і беремо потрібну кількість
        return _pendingTransactions
            .OrderByDescending(t => t.Fee)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Видалення транзакцій, які вже були додані в блок
    /// </summary>
    public void RemoveTransactions(List<BlockchainTransaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            _pendingTransactions.RemoveAll(t => t.TransactionId == transaction.TransactionId);
        }

        Console.WriteLine($"✓ Видалено {transactions.Count} транзакцій з mempool");
        Console.WriteLine($"  Залишилось транзакцій: {_pendingTransactions.Count}");
    }

    /// <summary>
    /// Отримання кількості транзакцій в mempool
    /// </summary>
    public int GetTransactionCount()
    {
        return _pendingTransactions.Count;
    }

    /// <summary>
    /// Очищення всього mempool
    /// </summary>
    public void Clear()
    {
        _pendingTransactions.Clear();
        Console.WriteLine("✓ Mempool очищено");
    }

    /// <summary>
    /// Отримання всіх транзакцій для перегляду
    /// </summary>
    public List<BlockchainTransaction> GetAllTransactions()
    {
        return new List<BlockchainTransaction>(_pendingTransactions);
    }

    /// <summary>
    /// Вивід статистики mempool
    /// </summary>
    public void PrintStats()
    {
        Console.WriteLine("\n═══════════════════════════════════════");
        Console.WriteLine("       СТАТИСТИКА MEMPOOL");
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine($"Всього транзакцій: {_pendingTransactions.Count}");

        if (_pendingTransactions.Any())
        {
            var avgFee = _pendingTransactions.Average(t => t.Fee);
            var maxFee = _pendingTransactions.Max(t => t.Fee);
            var minFee = _pendingTransactions.Min(t => t.Fee);

            Console.WriteLine($"Середня комісія: {avgFee:F8} BTC");
            Console.WriteLine($"Максимальна комісія: {maxFee:F8} BTC");
            Console.WriteLine($"Мінімальна комісія: {minFee:F8} BTC");
        }

        Console.WriteLine($"Місткість: {_pendingTransactions.Count}/{MaxMempoolSize}");
        Console.WriteLine("═══════════════════════════════════════\n");
    }
}
