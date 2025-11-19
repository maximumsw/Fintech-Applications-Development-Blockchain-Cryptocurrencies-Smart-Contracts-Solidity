using System.Collections.Generic;
using BlockchainApp.Models;
using BlockchainApp.Services;

namespace BlockchainApp.Models.SmartContracts
{
    public class PenaltyStakingContract : ISmartContract
    {
        public string Address { get; }

        // Внутрішній стан контракту:
        // Для кожного користувача памʼятаємо суму та блок початку стейкінгу
        private readonly Dictionary<string, StakeInfo> _stakes = new();

        // Параметри контракту
        private readonly decimal _rewardPerBlockPerToken;
        private readonly int _minLockBlocks;
        private readonly decimal _earlyPenaltyPercent;

        public PenaltyStakingContract(
            string address,
            decimal rewardPerBlockPerToken,
            int minLockBlocks,
            decimal earlyPenaltyPercent)
        {
            Address = address;
            _rewardPerBlockPerToken = rewardPerBlockPerToken;
            _minLockBlocks = minLockBlocks;
            _earlyPenaltyPercent = earlyPenaltyPercent;
        }

        public bool ValidateTransaction(BlockChainService chain, Transaction tx, int currentBlock)
        {
            // Якщо транзакція не стосується контракту — пропускаємо
            if (tx.ToAddress != Address && tx.FromAddress != Address)
                return true;

            // --------------------------------------------------
            //                 DEPOSIT (STAKE)
            // From = User
            // To   = Contract
            // --------------------------------------------------
            if (tx.ToAddress == Address)
            {
                string user = tx.FromAddress;
                decimal amount = tx.Amount;

                if (!_stakes.ContainsKey(user))
                {
                    _stakes[user] = new StakeInfo
                    {
                        Amount = amount,
                        StartBlock = currentBlock
                    };
                }
                else
                {
                    // Додаємо до існуючого стейку (стартовий блок НЕ змінюємо)
                    _stakes[user].Amount += amount;
                }

                return true;
            }

            // --------------------------------------------------
            //                 WITHDRAW (UNSTAKE)
            // From = Contract
            // To   = User
            // --------------------------------------------------
            if (tx.FromAddress == Address)
            {
                string user = tx.ToAddress;

                // Немає стейку → відхиляємо
                if (!_stakes.ContainsKey(user))
                    return false;

                var stake = _stakes[user];
                decimal principal = stake.Amount;
                int heldBlocks = currentBlock - stake.StartBlock;

                decimal allowedAmount;

                // Користувач виконав умови (достатньо блоків)
                if (heldBlocks >= _minLockBlocks)
                {
                    decimal reward = principal * _rewardPerBlockPerToken * heldBlocks;
                    allowedAmount = principal + reward;
                }
                else
                {
                    // Дострокове виведення → штраф
                    decimal penalty = principal * _earlyPenaltyPercent;
                    allowedAmount = principal - penalty;
                }

                // Якщо amount більше дозволеної суми — відхиляємо
                if (tx.Amount > allowedAmount)
                    return false;

                // Вивід дозволений → видаляємо stake
                _stakes.Remove(user);

                return true;
            }

            return true;
        }

        // ---------------------------------------------------------
        //      Метод для відображення стану (завдання 3)
        // ---------------------------------------------------------
        public IEnumerable<(string User, decimal Amount, int StartBlock, int HeldBlocks, decimal WithdrawNow)>
            GetState(int currentBlock)
        {
            foreach (var kvp in _stakes)
            {
                var user = kvp.Key;
                var st = kvp.Value;
                int held = currentBlock - st.StartBlock;

                decimal reward = st.Amount * _rewardPerBlockPerToken * held;
                decimal penalty = st.Amount * _earlyPenaltyPercent;

                decimal withdraw =
                    held >= _minLockBlocks
                    ? st.Amount + reward
                    : st.Amount - penalty;

                yield return (user, st.Amount, st.StartBlock, held, withdraw);
            }
        }

        // ---------------------------------------------------------
        //      Внутрішня структура стану
        // ---------------------------------------------------------
        private class StakeInfo
        {
            public decimal Amount { get; set; }
            public int StartBlock { get; set; }
        }
    }
}
