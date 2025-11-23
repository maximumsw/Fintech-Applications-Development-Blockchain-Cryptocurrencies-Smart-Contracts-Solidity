using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Halving.Models;

namespace Halving.Services;

/// <summary>
/// Основний сервіс блокчейну - управляє ланцюгом блоків, транзакціями та балансами
/// </summary>
public class BlockChainService
{
    // Ланцюг блоків
    public List<Block> Chain { get; private set; }

    // Мемпул - черга транзакцій, які очікують на майнінг
    public List<Transaction> Mempool { get; private set; }

    // Баланси всіх адрес
    public Dictionary<string, decimal> Balances { get; private set; }

    // Поточна складність майнінгу
    public int Difficulty { get; private set; }

    public decimal MiningReward { get; set; }
    public int TargetBlockTimeSeconds { get; set; }
    public int DifficultyAdjustmentWindow { get; set; }

    private const int MinDifficulty = 1;
    private const int MaxDifficulty = 10;

    public BlockChainService()
    {
        Chain = new List<Block>();
        Mempool = new List<Transaction>();
        Balances = new Dictionary<string, decimal>();

        Difficulty = 2; // Початкова складність
        MiningReward = 50m;
        TargetBlockTimeSeconds = 10;
        DifficultyAdjustmentWindow = 5;

        // Створюємо перший блок (genesis block)
        CreateGenesisBlock();
    }

    private void CreateGenesisBlock()
    {
        var genesisBlock = new Block(0, DateTime.UtcNow, "0", Difficulty);
        genesisBlock.Hash = CalculateHash(genesisBlock);
        Chain.Add(genesisBlock);
    }

    public string CalculateHash(Block block)
    {
        var blockData = $"{block.Index}{block.Timestamp:O}{block.PreviousHash}{block.Nonce}{block.Difficulty}";

        // Add transaction data to hash
        foreach (var tx in block.Transactions)
        {
            blockData += $"{tx.FromAddress}{tx.ToAddress}{tx.Amount}{tx.Fee}{tx.Note}";
        }

        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(blockData));
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }

    public void AddTransaction(Transaction transaction)
    {
        // Validate transaction
        if (string.IsNullOrEmpty(transaction.FromAddress) || string.IsNullOrEmpty(transaction.ToAddress))
        {
            throw new ArgumentException("Transaction must include from and to addresses");
        }

        if (transaction.Amount <= 0)
        {
            throw new ArgumentException("Transaction amount must be greater than zero");
        }

        // Check if sender has sufficient balance (skip for COINBASE)
        if (transaction.FromAddress != "COINBASE")
        {
            var balance = GetBalance(transaction.FromAddress);
            var totalRequired = transaction.Amount + transaction.Fee;

            if (balance < totalRequired)
            {
                throw new InvalidOperationException($"Insufficient balance. Available: {balance}, Required: {totalRequired}");
            }
        }

        Mempool.Add(transaction);
    }

    /// <summary>
    /// Майнінг нового блоку з усіма транзакціями з мемпулу
    /// </summary>
    public Block MinePending(string minerAddress)
    {
        var stopwatch = Stopwatch.StartNew();

        // Створюємо новий блок
        var newBlock = new Block(
            Chain.Count,
            DateTime.UtcNow,
            Chain[^1].Hash,
            Difficulty
        );

        // Додаємо coinbase транзакцію (винагорода майнеру)
        var coinbaseTransaction = new Transaction(
            "COINBASE",
            minerAddress,
            MiningReward,
            0,
            "Mining reward"
        );
        newBlock.Transactions.Add(coinbaseTransaction);

        // Додаємо всі транзакції з мемпулу
        newBlock.Transactions.AddRange(Mempool);

        // Виконуємо proof of work (майнінг)
        ProofOfWork(newBlock);

        stopwatch.Stop();
        newBlock.MiningDurationMs = stopwatch.ElapsedMilliseconds;

        // Додаємо блок до ланцюга
        Chain.Add(newBlock);

        // Оновлюємо баланси
        UpdateBalances(newBlock);

        // Очищаємо мемпул
        Mempool.Clear();

        // Коригуємо складність після майнінгу
        AdjustDifficulty();

        return newBlock;
    }

    /// <summary>
    /// Proof of Work - шукаємо nonce, щоб хеш починався з N нулів
    /// </summary>
    public void ProofOfWork(Block block)
    {
        var target = new string('0', block.Difficulty);

        while (true)
        {
            block.Hash = CalculateHash(block);

            // Якщо хеш починається з потрібної кількості нулів - готово!
            if (block.Hash.StartsWith(target))
            {
                break;
            }

            block.Nonce++;
        }
    }

    private void UpdateBalances(Block block)
    {
        foreach (var transaction in block.Transactions)
        {
            // Deduct from sender (skip for COINBASE)
            if (transaction.FromAddress != "COINBASE")
            {
                if (!Balances.ContainsKey(transaction.FromAddress))
                {
                    Balances[transaction.FromAddress] = 0;
                }
                Balances[transaction.FromAddress] -= (transaction.Amount + transaction.Fee);
            }

            // Add to receiver
            if (!Balances.ContainsKey(transaction.ToAddress))
            {
                Balances[transaction.ToAddress] = 0;
            }
            Balances[transaction.ToAddress] += transaction.Amount;
        }
    }

    public decimal GetBalance(string address)
    {
        return Balances.ContainsKey(address) ? Balances[address] : 0;
    }

    /// <summary>
    /// Автоматичне коригування складності на основі швидкості майнінгу останніх блоків
    /// </summary>
    public void AdjustDifficulty()
    {
        // Потрібно хоча б кілька блоків для аналізу
        if (Chain.Count <= DifficultyAdjustmentWindow)
        {
            return;
        }

        // Беремо останні K блоків для аналізу
        var blocksToAnalyze = Chain
            .Skip(Math.Max(1, Chain.Count - DifficultyAdjustmentWindow - 1))
            .Take(DifficultyAdjustmentWindow)
            .Where(b => b.MiningDurationMs > 0)
            .ToList();

        if (blocksToAnalyze.Count == 0)
        {
            return;
        }

        // Обчислюємо середній час майнінгу
        var avgMiningTimeMs = blocksToAnalyze.Average(b => b.MiningDurationMs);
        var avgMiningTimeSeconds = avgMiningTimeMs / 1000.0;

        var targetTimeSeconds = TargetBlockTimeSeconds;

        // Якщо майнінг занадто швидкий - збільшуємо складність
        if (avgMiningTimeSeconds < targetTimeSeconds * 0.7)
        {
            Difficulty = Math.Min(Difficulty + 1, MaxDifficulty);
        }
        // Якщо майнінг занадто повільний - зменшуємо складність
        else if (avgMiningTimeSeconds > targetTimeSeconds * 1.5)
        {
            Difficulty = Math.Max(Difficulty - 1, MinDifficulty);
        }
    }

    public double GetAverageMiningTime()
    {
        var blocksWithMiningTime = Chain
            .Skip(Math.Max(1, Chain.Count - DifficultyAdjustmentWindow))
            .Where(b => b.MiningDurationMs > 0)
            .ToList();

        if (blocksWithMiningTime.Count == 0)
        {
            return 0;
        }

        return blocksWithMiningTime.Average(b => b.MiningDurationMs) / 1000.0;
    }

    public bool ValidateChain()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            var currentBlock = Chain[i];
            var previousBlock = Chain[i - 1];

            // Validate hash
            if (currentBlock.Hash != CalculateHash(currentBlock))
            {
                return false;
            }

            // Validate previous hash link
            if (currentBlock.PreviousHash != previousBlock.Hash)
            {
                return false;
            }

            // Validate proof of work
            var target = new string('0', currentBlock.Difficulty);
            if (!currentBlock.Hash.StartsWith(target))
            {
                return false;
            }
        }

        return true;
    }
}
