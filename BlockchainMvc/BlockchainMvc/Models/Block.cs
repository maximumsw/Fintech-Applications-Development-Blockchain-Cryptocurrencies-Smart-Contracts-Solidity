using System;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainMvc.Models
{
    public class Block
    {
        public int Index { get; set; }
        public string Data { get; set; } = string.Empty;
        public string PrevHash { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public Block() { }

        public Block(int index, string data, string prevHash)
        {
            Index = index;
            Data = data;
            PrevHash = prevHash;
            Timestamp = DateTime.Now;
            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            using var sha256 = SHA256.Create();
            var rawData = $"{Index}{Timestamp:O}{Data}{PrevHash}";
            var bytes = Encoding.UTF8.GetBytes(rawData);
            var hashBytes = sha256.ComputeHash(bytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}