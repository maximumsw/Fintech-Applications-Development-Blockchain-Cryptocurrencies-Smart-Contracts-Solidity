using Smart_Contracts.Models;

namespace Smart_Contracts.Services;

/// <summary>
/// Сервіс для управління блокчейном з підтримкою смарт-контрактів
/// </summary>
public class BlockChainService
{
    public List<Block> Chain { get; }
    public List<Transaction> PendingTransactions { get; }

    /// <summary>
    /// Словник зареєстрованих смарт-контрактів: адреса → контракт
    /// </summary>
    public Dictionary<string, ISmartContract> Contracts { get; } = new();

    public int Difficulty { get; set; }
    public decimal MiningReward { get; set; }

    public BlockChainService()
    {
        Chain = new List<Block>();
        PendingTransactions = new List<Transaction>();
        Difficulty = 2;
        MiningReward = 10m;

        // Create genesis block
        Chain.Add(CreateGenesisBlock());
    }

    private Block CreateGenesisBlock()
    {
        var genesisTransactions = new List<Transaction>();
        var block = new Block(0, genesisTransactions, "0");
        block.Hash = block.CalculateHash();
        return block;
    }

    public Block GetLatestBlock()
    {
        return Chain[^1];
    }

    public void MinePendingTransactions(string miningRewardAddress)
    {
        var rewardTransaction = new Transaction("", miningRewardAddress, MiningReward);
        PendingTransactions.Add(rewardTransaction);

        var block = new Block(Chain.Count, new List<Transaction>(PendingTransactions), GetLatestBlock().Hash);
        block.MineBlock(Difficulty);

        Chain.Add(block);
        PendingTransactions.Clear();
    }

    public void AddTransaction(Transaction transaction)
    {
        if (string.IsNullOrEmpty(transaction.FromAddress) || string.IsNullOrEmpty(transaction.ToAddress))
            throw new Exception("Transaction must include from and to address");

        if (!transaction.IsValid())
            throw new Exception("Cannot add invalid transaction to chain");

        if (transaction.Amount <= 0)
            throw new Exception("Transaction amount must be positive");

        var balance = GetBalanceOfAddress(transaction.FromAddress);
        if (balance < transaction.Amount)
            throw new Exception($"Insufficient balance. Available: {balance}, Required: {transaction.Amount}");

        // ВАЛІДАЦІЯ СМАРТ-КОНТРАКТІВ
        // Отримуємо поточний індекс блоку для передачі контрактам
        var currentBlockIndex = Chain.Count;

        // Перевіряємо чи адреса відправника є смарт-контрактом
        if (Contracts.ContainsKey(transaction.FromAddress))
        {
            try
            {
                // Викликаємо валідацію контракту відправника
                Contracts[transaction.FromAddress].ValidateTransaction(this, transaction, currentBlockIndex);
            }
            catch (Exception ex)
            {
                // Якщо контракт відхилив транзакцію - передаємо помилку далі
                throw new Exception($"Contract validation failed: {ex.Message}");
            }
        }

        // Перевіряємо чи адреса отримувача є смарт-контрактом
        if (Contracts.ContainsKey(transaction.ToAddress))
        {
            try
            {
                // Викликаємо валідацію контракту отримувача
                Contracts[transaction.ToAddress].ValidateTransaction(this, transaction, currentBlockIndex);
            }
            catch (Exception ex)
            {
                // Якщо контракт відхилив транзакцію - передаємо помилку далі
                throw new Exception($"Contract validation failed: {ex.Message}");
            }
        }

        PendingTransactions.Add(transaction);
    }

    public decimal GetBalanceOfAddress(string address)
    {
        decimal balance = 0;

        foreach (var block in Chain)
        {
            foreach (var transaction in block.Transactions)
            {
                if (transaction.FromAddress == address)
                    balance -= transaction.Amount;

                if (transaction.ToAddress == address)
                    balance += transaction.Amount;
            }
        }

        return balance;
    }

    public bool IsChainValid()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            var currentBlock = Chain[i];
            var previousBlock = Chain[i - 1];

            if (currentBlock.Hash != currentBlock.CalculateHash())
                return false;

            if (currentBlock.PreviousHash != previousBlock.Hash)
                return false;

            foreach (var transaction in currentBlock.Transactions)
            {
                if (!transaction.IsValid())
                    return false;
            }
        }

        return true;
    }

    public List<Transaction> GetTransactionsForAddress(string address)
    {
        var transactions = new List<Transaction>();

        foreach (var block in Chain)
        {
            foreach (var transaction in block.Transactions)
            {
                if (transaction.FromAddress == address || transaction.ToAddress == address)
                    transactions.Add(transaction);
            }
        }

        return transactions;
    }

    /// <summary>
    /// Реєструє смарт-контракт за вказаною адресою
    /// </summary>
    public void RegisterContract(ISmartContract contract)
    {
        if (Contracts.ContainsKey(contract.Address))
            throw new Exception($"Contract already registered at address {contract.Address}");

        Contracts[contract.Address] = contract;
    }

    /// <summary>
    /// Отримує смарт-контракт за адресою (якщо існує)
    /// </summary>
    public ISmartContract? GetContract(string address)
    {
        return Contracts.ContainsKey(address) ? Contracts[address] : null;
    }
}
