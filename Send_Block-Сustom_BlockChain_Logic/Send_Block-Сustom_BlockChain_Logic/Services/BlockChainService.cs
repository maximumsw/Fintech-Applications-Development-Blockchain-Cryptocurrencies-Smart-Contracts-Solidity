using Send_Block_Сustom_BlockChain_Logic.Models;

namespace Send_Block_Сustom_BlockChain_Logic.Services;

public class BlockChainService
{
    public string NodeId { get; }
    public List<Block> Chain { get; private set; } = new();
    public List<Transaction> Mempool { get; private set; } = new();
    public Dictionary<string, Wallet> Wallets { get; private set; } = new();
    public int Difficulty { get; set; } = 2;
    public decimal MiningReward { get; set; } = 50m;

    public BlockChainService(string nodeId)
    {
        NodeId = nodeId;
        CreateGenesisBlock();
    }

    private void CreateGenesisBlock()
    {
        var genesisBlock = new Block(0, new List<Transaction>(), "0", Difficulty)
        {
            MinedBy = "GENESIS",
            Timestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        genesisBlock.Hash = genesisBlock.ComputeHash();
        Chain.Add(genesisBlock);
    }

    public Block GetLatestBlock()
    {
        return Chain[^1];
    }

    public void SetDifficulty(int difficulty)
    {
        if (difficulty < 1 || difficulty > 6)
            throw new ArgumentException("Difficulty must be between 1 and 6");

        Difficulty = difficulty;
    }

    public Wallet RegisterWallet(string name)
    {
        var wallet = new Wallet();
        Wallets[name] = wallet;
        return wallet;
    }

    public Wallet? GetWallet(string name)
    {
        return Wallets.GetValueOrDefault(name);
    }

    public void CreateTransaction(string fromWalletName, string toAddress, decimal amount)
    {
        var wallet = GetWallet(fromWalletName);
        if (wallet == null)
            throw new ArgumentException($"Wallet '{fromWalletName}' not found");

        var balance = GetBalance(wallet.Address);
        if (balance < amount)
            throw new InvalidOperationException($"Insufficient balance. Available: {balance}, Required: {amount}");

        var transaction = new Transaction(wallet.Address, toAddress, amount);
        transaction.Sign(wallet);

        if (!transaction.IsValid())
            throw new InvalidOperationException("Transaction signature is invalid");

        Mempool.Add(transaction);
    }

    public decimal GetBalance(string address)
    {
        decimal balance = 0;

        foreach (var block in Chain)
        {
            foreach (var tx in block.Transactions)
            {
                if (tx.To == address)
                    balance += tx.Amount;
                if (tx.From == address)
                    balance -= tx.Amount;
            }
        }

        return balance;
    }

    public Block MinePendingTransactions(string minerWalletName)
    {
        var wallet = GetWallet(minerWalletName);
        if (wallet == null)
            throw new ArgumentException($"Miner wallet '{minerWalletName}' not found");

        // Створити транзакцію винагороди
        var rewardTx = new Transaction("COINBASE", wallet.Address, MiningReward);
        var transactionsToMine = new List<Transaction>(Mempool) { rewardTx };

        var newBlock = new Block(
            Chain.Count,
            transactionsToMine,
            GetLatestBlock().Hash,
            Difficulty
        );

        newBlock.Mine(wallet);

        Chain.Add(newBlock);
        Mempool.Clear();

        return newBlock;
    }

    public bool TryAddExternalBlock(Block block)
    {
        // Перевірити індекс блоку
        if (block.Index != Chain.Count)
            return false;

        // Перевірити попередній хеш
        if (block.PrevHash != GetLatestBlock().Hash)
            return false;

        // Перевірити обчислення хешу
        if (block.Hash != block.ComputeHash())
            return false;

        // Перевірити підтвердження роботи
        if (!block.HasValidProof())
            return false;

        // Перевірити підпис блоку
        if (!block.Verify())
            return false;

        // Перевірити всі транзакції
        foreach (var tx in block.Transactions)
        {
            if (!tx.IsValid())
                return false;
        }

        // Видалити транзакції з мемпулу, які знаходяться в прийнятому блоці
        foreach (var tx in block.Transactions)
        {
            var matchingTx = Mempool.FirstOrDefault(m =>
                m.From == tx.From &&
                m.To == tx.To &&
                m.Amount == tx.Amount &&
                Math.Abs((m.Timestamp - tx.Timestamp).TotalSeconds) < 1);

            if (matchingTx != null)
            {
                Mempool.Remove(matchingTx);
            }
        }

        Chain.Add(block);
        return true;
    }

    public bool IsChainValid()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            var currentBlock = Chain[i];
            var previousBlock = Chain[i - 1];

            if (!currentBlock.IsValid())
                return false;

            if (currentBlock.PrevHash != previousBlock.Hash)
                return false;
        }

        return true;
    }

    public void DemoSetup()
    {
        // Очистити наявні дані
        Wallets.Clear();
        Mempool.Clear();

        // Створити демонстраційні гаманці
        var alice = RegisterWallet("Alice");
        var bob = RegisterWallet("Bob");
        var charlie = RegisterWallet("Charlie");

        // Створити кілька демонстраційних транзакцій, тільки якщо у нас більше, ніж генезис-блок
        if (Chain.Count > 1)
        {
            // Нам потрібен початковий баланс, який надходитиме від майнінгу
            // Тому ми не будемо створювати транзакції в демонстраційній установці
            // Користувачі повинні спочатку майнити, щоб отримати винагороди Coinbase
        }
    }
}
