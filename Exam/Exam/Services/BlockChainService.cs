using System;
using System.Collections.Generic;
using System.Linq;
using BlockchainApp.Models;
using BlockchainApp.Models.SmartContracts;

namespace BlockchainApp.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; private set; } = new();
        public List<Transaction> Mempool { get; private set; } = new();

        public Dictionary<string, ISmartContract> Contracts { get; } = new();

        public Wallet PenaltyStakingWallet { get; }
        public PenaltyStakingContract PenaltyStakingContract { get; }

        public int CurrentHeight => Chain.Count == 0 ? 0 : Chain.Last().Index;

        public BlockChainService()
        {
            // 1. Створюємо перший блок
            Chain.Add(CreateGenesisBlock());

            // 2. Створюємо новий гаманець для PenaltyStaking
            PenaltyStakingWallet = WalletService.CreateWallet();

            // 3. Створюємо контракт
            PenaltyStakingContract = new PenaltyStakingContract(
                PenaltyStakingWallet.Address,
                rewardPerBlockPerToken: 0.001m,
                minLockBlocks: 20,
                earlyPenaltyPercent: 0.20m
            );

            // 4. Додаємо у словник контрактів
            Contracts.Add(PenaltyStakingWallet.Address, PenaltyStakingContract);
        }

        // ------------------------------------------------------
        //  БАЗОВІ МЕТОДИ
        // ------------------------------------------------------

        private Block CreateGenesisBlock()
        {
            return new Block
            {
                Index = 0,
                Timestamp = DateTime.UtcNow,
                PreviousHash = "0",
                Transactions = new List<Transaction>(),
            }.MineBlock(2);
        }

        public Block GetLatestBlock() => Chain.Last();

        // ------------------------------------------------------
        //       СТВОРЕННЯ ТРАНЗАКЦІЇ (основне місце інтеграції)
        // ------------------------------------------------------

        public bool CreateTransaction(Transaction tx)
        {
            // 1 — Загальна перевірка
            if (!ValidateGeneralTransaction(tx))
                return false;

            // 2 — Валідація контракту для From
            if (Contracts.TryGetValue(tx.FromAddress, out var fromContract))
            {
                if (!fromContract.ValidateTransaction(this, tx, CurrentHeight))
                    return false;
            }

            // 3 — Валідація контракту для To
            if (Contracts.TryGetValue(tx.ToAddress, out var toContract))
            {
                if (!toContract.ValidateTransaction(this, tx, CurrentHeight))
                    return false;
            }

            // 4 — Якщо все валідно, додаємо в mempool
            Mempool.Add(tx);
            return true;
        }

        // ------------------------------------------------------
        //       ПРОСТА ЗАГАЛЬНА ВАЛІДАЦІЯ
        // ------------------------------------------------------

        public bool ValidateGeneralTransaction(Transaction tx)
        {
            if (tx.Amount < 0) return false;
            if (string.IsNullOrWhiteSpace(tx.FromAddress)) return false;
            if (string.IsNullOrWhiteSpace(tx.ToAddress)) return false;
            return true;
        }

        // ------------------------------------------------------
        //                   МАЙНІНГ БЛОКА
        // ------------------------------------------------------

        public Block MineBlock(string minerAddress)
        {
            // Створюємо новий блок ЗАВЖДИ, навіть якщо Mempool порожній
            var block = new Block
            {
                Index = Chain.Count,
                Timestamp = DateTime.UtcNow,
                PreviousHash = GetLatestBlock().Hash,
                Transactions = new List<Transaction>(Mempool) // може бути пустий список
            };

            // Proof-of-Work: шукаємо хеш з "00" на початку
            block.MineBlock(2);

            // Додаємо блок у ланцюг
            Chain.Add(block);

            // Очищуємо Mempool після майнінгу
            Mempool.Clear();

            return block;
        }
    

        // ------------------------------------------------------
        //          ВАЛІДАЦІЯ ЗОВНІШНЬОГО ЛАНЦЮГА
        // ------------------------------------------------------

        public bool IsChainValid(List<Block> chain)
        {
            for (int i = 1; i < chain.Count; i++)
            {
                var prev = chain[i - 1];
                var curr = chain[i];

                if (curr.PreviousHash != prev.Hash)
                    return false;

                if (curr.Hash != curr.CalculateHash())
                    return false;

                if (!curr.Hash.StartsWith("00")) // PoW target
                    return false;
            }

            return true;
        }

        // ------------------------------------------------------
        //          ЗАМІНА ЛАНЦЮГА (Firebase)
        // ------------------------------------------------------

        public void ReplaceChain(List<Block> newChain)
        {
            Chain = newChain;
        }
    }
}
