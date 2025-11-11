using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BlockchainMvc.Models
{
    public class Blockchain
    {
        private readonly List<Block> _chain = new();
        public IReadOnlyList<Block> Chain => _chain;

        public Blockchain()
        {
            if (!_chain.Any())
            {
                _chain.Add(CreateGenesisBlock());
            }
        }

        private Block CreateGenesisBlock()
        {
            return new Block(0, "Genesis Block", "0");
        }

        public Block GetLastBlock() => _chain[^1];

        public Block AddBlock(string data)
        {
            var last = GetLastBlock();
            var newBlock = new Block(last.Index + 1, data, last.Hash);
            _chain.Add(newBlock);
            return newBlock;
        }

        public bool IsValid()
        {
            for (int i = 1; i < _chain.Count; i++)
            {
                var current = _chain[i];
                var prev = _chain[i - 1];

                if (current.Hash != current.CalculateHash())
                    return false;

                if (current.PrevHash != prev.Hash)
                    return false;
            }

            return true;
        }

        public Block? FindByIndex(int index)
            => _chain.FirstOrDefault(b => b.Index == index);

        public Block? FindByHash(string hash)
            => _chain.FirstOrDefault(b =>
                b.Hash.Equals(hash, StringComparison.OrdinalIgnoreCase));

        public void SaveToFile(string path)
        {
            var json = JsonSerializer.Serialize(_chain,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path))
                return;

            var json = File.ReadAllText(path);
            var blocks = JsonSerializer.Deserialize<List<Block>>(json);
            if (blocks is { Count: > 0 })
            {
                _chain.Clear();
                _chain.AddRange(blocks);
            }
        }
    }
}
