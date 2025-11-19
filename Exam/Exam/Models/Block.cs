using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainApp.Models
{
    public class Block
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string PreviousHash { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public int Nonce { get; set; }
        public List<Transaction> Transactions { get; set; } = new();

        public string CalculateHash()
        {
            var sb = new StringBuilder();
            sb.Append(Index);
            sb.Append(Timestamp.Ticks);
            sb.Append(PreviousHash);
            sb.Append(Nonce);

            foreach (var tx in Transactions)
            {
                sb.Append(tx.FromAddress);
                sb.Append(tx.ToAddress);
                sb.Append(tx.Amount);
                sb.Append(tx.Fee);
                sb.Append(tx.Signature);
            }

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(bytes);
        }

        public Block MineBlock(int difficulty)
        {
            var targetPrefix = new string('0', difficulty);

            do
            {
                Nonce++;
                Hash = CalculateHash();
            } while (!Hash.StartsWith(targetPrefix, StringComparison.Ordinal));

            return this;
        }
    }
}