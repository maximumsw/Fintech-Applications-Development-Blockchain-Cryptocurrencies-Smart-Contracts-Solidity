using System.Collections.Generic;

namespace POW_MemPOOL.Models
{
    public class IndexViewModel
    {
        public Blockchain Blockchain { get; set; }
        public bool IsMining { get; set; }
        public int CurrentDifficulty { get; set; }
        public bool IsChainValid { get; set; }

        public IndexViewModel(Blockchain blockchain, bool isMining, int currentDifficulty, bool isChainValid)
        {
            Blockchain = blockchain;
            IsMining = isMining;
            CurrentDifficulty = currentDifficulty;
            IsChainValid = isChainValid;
        }
    }
}
