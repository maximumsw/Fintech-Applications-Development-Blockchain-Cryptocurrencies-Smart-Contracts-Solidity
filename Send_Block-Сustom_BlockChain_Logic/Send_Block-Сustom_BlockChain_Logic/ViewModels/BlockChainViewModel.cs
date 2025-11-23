using Send_Block_Сustom_BlockChain_Logic.Models;

namespace Send_Block_Сustom_BlockChain_Logic.ViewModels;

public class BlockChainViewModel
{
    public string CurrentNodeId { get; set; } = string.Empty;
    public List<string> AllNodeIds { get; set; } = new();
    public List<Block> Chain { get; set; } = new();
    public List<Transaction> Mempool { get; set; } = new();
    public Dictionary<string, Wallet> Wallets { get; set; } = new();
    public int Difficulty { get; set; }
    public decimal MiningReward { get; set; }
}
