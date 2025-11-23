using Halving2.Models;

namespace Halving2.Services;

/// <summary>
/// Основний сервіс блокчейну
/// Управляє ланцюгом блоків, мемпулом транзакцій та балансами гаманців
/// </summary>
public class BlockchainService
{
    // Ланцюг блоків
    public List<Block> Chain { get; private set; }

    // Мемпул - список транзакцій, що очікують на включення в блок
    public List<Transaction> Mempool { get; private set; }

    // Словник балансів гаманців
    private Dictionary<string, decimal> _balances;

    // Конфігурація halving (зменшення винагороди)
    // BaseReward: Початкова винагорода за блок в монетах
    // HalvingInterval: Кількість блоків між кожним halvingом
    // Формула: винагорода = BaseReward / 2^(blockHeight / HalvingInterval)
    public decimal BaseReward { get; set; } = 50m;
    public int HalvingInterval { get; set; } = 10;

    // Складність майнінгу (кількість нулів на початку хешу)
    public int Difficulty { get; set; } = 2;

    public BlockchainService()
    {
        Chain = new List<Block>();
        Mempool = new List<Transaction>();
        _balances = new Dictionary<string, decimal>();

        // Створюємо genesis блок (перший блок в ланцюзі)
        CreateGenesisBlock();
    }

    /// <summary>
    /// Створює genesis блок - перший блок в блокчейні
    /// Виділяє початкові монети для тестових гаманців
    /// </summary>
    private void CreateGenesisBlock()
    {
        // Початковий розподіл монет для тестування
        var genesisTransactions = new List<Transaction>
        {
            new Transaction("GENESIS", "Alice", 1000m, 0, "Genesis allocation"),
            new Transaction("GENESIS", "Bob", 1000m, 0, "Genesis allocation"),
            new Transaction("GENESIS", "Charlie", 1000m, 0, "Genesis allocation")
        };

        var genesisBlock = new Block(0, "0", genesisTransactions, Difficulty);
        genesisBlock.MineBlock();

        // Встановлюємо висоту блоку для всіх транзакцій
        foreach (var tx in genesisTransactions)
        {
            tx.BlockHeight = 0;
        }

        Chain.Add(genesisBlock);
        RecalculateBalances();
    }

    /// <summary>
    /// Обчислює винагороду за блок на основі графіку halving
    /// Формула: винагорода = BaseReward / 2^(blockHeight / HalvingInterval)
    /// </summary>
    public decimal GetBlockReward(int blockHeight)
    {
        if (blockHeight == 0)
            return 0; // Genesis блок не має винагороди за майнінг

        // Обчислюємо кількість halvingів
        int halvings = blockHeight / HalvingInterval;
        decimal reward = BaseReward;

        // Ділимо винагороду навпіл для кожного halving
        for (int i = 0; i < halvings; i++)
        {
            reward /= 2m;
        }

        return reward;
    }

    /// <summary>
    /// Додає транзакцію до мемпулу після валідації
    /// </summary>
    public void AddTransaction(Transaction tx)
    {
        // Базова валідація
        if (string.IsNullOrEmpty(tx.FromAddress) || string.IsNullOrEmpty(tx.ToAddress))
            throw new ArgumentException("Транзакція повинна мати валідні адреси відправника та одержувача");

        if (tx.Amount <= 0)
            throw new ArgumentException("Сума транзакції повинна бути додатною");

        if (tx.Fee < 0)
            throw new ArgumentException("Комісія не може бути від'ємною");

        // Для звичайних транзакцій (не coinbase) перевіряємо баланс
        if (tx.FromAddress != "COINBASE" && tx.FromAddress != "GENESIS")
        {
            var balance = GetBalance(tx.FromAddress);
            var totalRequired = tx.Amount + tx.Fee;

            if (balance < totalRequired)
                throw new InvalidOperationException($"Недостатньо коштів. Потрібно: {totalRequired}, Доступно: {balance}");
        }

        Mempool.Add(tx);
    }

    /// <summary>
    /// Майнить новий блок з усіма транзакціями з мемпулу
    /// Створює coinbase транзакцію з винагородою для майнера
    /// </summary>
    public void MinePending(string minerAddress)
    {
        if (string.IsNullOrEmpty(minerAddress))
            throw new ArgumentException("Адреса майнера не може бути порожньою");

        var newBlockHeight = Chain.Count;
        var previousHash = Chain[^1].Hash;

        // Обчислюємо винагороду та комісії
        var blockReward = GetBlockReward(newBlockHeight);
        var totalFees = Mempool.Sum(tx => tx.Fee);
        var coinbaseAmount = blockReward + totalFees;

        // Створюємо coinbase транзакцію (винагорода майнеру)
        var coinbaseTx = new Transaction("COINBASE", minerAddress, coinbaseAmount, 0,
            $"Block reward (base: {blockReward}, fees: {totalFees})");

        // Підготовка транзакцій для блоку
        var blockTransactions = new List<Transaction> { coinbaseTx };
        blockTransactions.AddRange(Mempool);

        // Створюємо та майнимо новий блок
        var newBlock = new Block(newBlockHeight, previousHash, blockTransactions, Difficulty);
        newBlock.MineBlock();

        // Оновлюємо висоту блоку для всіх транзакцій
        foreach (var tx in blockTransactions)
        {
            tx.BlockHeight = newBlockHeight;
        }

        // Додаємо блок до ланцюга
        Chain.Add(newBlock);

        // Очищаємо мемпул
        Mempool.Clear();

        // Перераховуємо баланси
        RecalculateBalances();
    }

    /// <summary>
    /// Отримує підтверджений баланс адреси
    /// </summary>
    public decimal GetBalance(string address)
    {
        return _balances.GetValueOrDefault(address, 0m);
    }

    /// <summary>
    /// Отримує баланс включаючи транзакції з мемпулу (непідтверджені)
    /// </summary>
    public decimal GetBalanceWithPending(string address)
    {
        var confirmedBalance = GetBalance(address);

        // Отримані монети з мемпулу
        var pendingReceived = Mempool
            .Where(tx => tx.ToAddress == address)
            .Sum(tx => tx.Amount);

        // Відправлені монети з мемпулу
        var pendingSent = Mempool
            .Where(tx => tx.FromAddress == address)
            .Sum(tx => tx.Amount + tx.Fee);

        return confirmedBalance + pendingReceived - pendingSent;
    }

    /// <summary>
    /// Отримує список всіх адрес з балансами
    /// </summary>
    public List<string> GetAllAddresses()
    {
        return _balances.Keys.OrderBy(k => k).ToList();
    }

    /// <summary>
    /// Обчислює кількість підтверджень для блоку
    /// Формула: підтвердження = ВисотаОстанньогоБлоку - ВисотаБлоку + 1
    /// </summary>
    public int GetConfirmations(int blockHeight)
    {
        var lastBlockHeight = Chain.Count - 1;
        return lastBlockHeight - blockHeight + 1;
    }

    /// <summary>
    /// Отримує історію транзакцій для конкретного гаманця
    /// Включає як підтверджені транзакції, так і з мемпулу
    /// </summary>
    public List<Transaction> GetWalletHistory(string address)
    {
        var history = new List<Transaction>();

        // Отримуємо всі підтверджені транзакції з блоків
        foreach (var block in Chain)
        {
            foreach (var tx in block.Transactions)
            {
                if (tx.FromAddress == address || tx.ToAddress == address)
                {
                    history.Add(tx);
                }
            }
        }

        // Додаємо непідтверджені транзакції з мемпулу
        foreach (var tx in Mempool)
        {
            if (tx.FromAddress == address || tx.ToAddress == address)
            {
                history.Add(tx);
            }
        }

        return history;
    }

    /// <summary>
    /// Перераховує баланси всіх гаманців на основі всього ланцюга блоків
    /// </summary>
    private void RecalculateBalances()
    {
        _balances.Clear();

        foreach (var block in Chain)
        {
            foreach (var tx in block.Transactions)
            {
                // Зараховуємо монети одержувачу
                if (!_balances.ContainsKey(tx.ToAddress))
                    _balances[tx.ToAddress] = 0;
                _balances[tx.ToAddress] += tx.Amount;

                // Списуємо монети з відправника (крім coinbase та genesis)
                if (tx.FromAddress != "COINBASE" && tx.FromAddress != "GENESIS")
                {
                    if (!_balances.ContainsKey(tx.FromAddress))
                        _balances[tx.FromAddress] = 0;
                    _balances[tx.FromAddress] -= (tx.Amount + tx.Fee);
                }
            }
        }
    }
}
