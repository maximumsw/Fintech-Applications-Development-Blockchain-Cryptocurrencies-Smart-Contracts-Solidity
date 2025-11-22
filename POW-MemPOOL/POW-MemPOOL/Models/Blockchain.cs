using System;
using System.Collections.Generic;
using System.Linq;

namespace POW_MemPOOL.Models
{
    public class Blockchain
    {
        public IList<Block> Chain { get; private set; }

        public Blockchain()
        {
            Chain = new List<Block>
            {
                CreateGenesisBlock()
            };
        }

        private Block CreateGenesisBlock()
        {
            var genesis = new Block(0, "Genesis Block", DateTime.UtcNow, "0");
            // No need to mine the genesis block, but we can give it a default valid hash.
            genesis.Difficulty = 1;
            genesis.Hash = genesis.ComputeHash();
            while (!genesis.Hash.StartsWith("0"))
            {
                genesis.Nonce++;
                genesis.Hash = genesis.ComputeHash();
            }
            return genesis;
        }

        public Block GetLatestBlock()
        {
            return Chain.Last();
        }

        public void AddBlock(Block newBlock)
        {
            newBlock.PreviousHash = GetLatestBlock().Hash;
            newBlock.Id = GetLatestBlock().Id + 1;
            // The hash will be re-computed during mining.
            // newBlock.Hash = newBlock.ComputeHash(); 
            Chain.Add(newBlock);
        }

        public bool IsValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                Block currentBlock = Chain[i];
                Block previousBlock = Chain[i - 1];

                if (currentBlock.Hash != currentBlock.ComputeHash())
                {
                    return false;
                }

                if (currentBlock.PreviousHash != previousBlock.Hash)
                {
                    return false;
                }

                if (!currentBlock.HasValidProof())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
