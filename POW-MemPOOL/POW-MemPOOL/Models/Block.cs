using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace POW_MemPOOL.Models
{
    public class Block
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }

        // Proof-of-Work
        public int Nonce { get; set; }
        public int Difficulty { get; set; }
        public long MiningDurationMs { get; set; }

        public Block(int id, string data, DateTime timestamp, string previousHash)
        {
            Id = id;
            Data = data;
            Timestamp = timestamp;
            PreviousHash = previousHash;
            Nonce = 0;
            Difficulty = 0;
            Hash = ComputeHash();
        }

        public string ComputeHash()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string rawData = $"{Id}{Timestamp:yyyyMMddHHmmssfff}{Data}{PreviousHash}{Nonce}{Difficulty}";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }

        public void Mine(int difficulty)
        {
            Difficulty = difficulty;
            var stopwatch = Stopwatch.StartNew();
            string target = new string('0', difficulty);

            while (!Hash.StartsWith(target))
            {
                Nonce++;
                Hash = ComputeHash();
            }
            stopwatch.Stop();
            MiningDurationMs = stopwatch.ElapsedMilliseconds;
        }

        public bool HasValidProof()
        {
            string target = new string('0', Difficulty);
            return Hash.StartsWith(target) && Hash == ComputeHash();
        }

        public async Task<Block> MineAsync(int difficulty, CancellationToken ct, IProgress<int> progress = null)
        {
            return await Task.Run(() =>
            {
                Difficulty = difficulty;
                var stopwatch = Stopwatch.StartNew();
                string target = new string('0', difficulty);
                int attempts = 0;

                while (!Hash.StartsWith(target))
                {
                    ct.ThrowIfCancellationRequested();
                    
                    Nonce++;
                    Hash = ComputeHash();
                    attempts++;

                    if (attempts % 10000 == 0) // Report progress every 10,000 attempts
                    {
                        progress?.Report(attempts);
                    }
                }
                stopwatch.Stop();
                MiningDurationMs = stopwatch.ElapsedMilliseconds;
                
                // Final progress report
                progress?.Report(attempts);

                return this;
            }, ct);
        }
    }
}
