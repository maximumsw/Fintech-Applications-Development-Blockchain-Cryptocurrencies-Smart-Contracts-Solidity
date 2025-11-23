using Halving2.Models;

namespace Halving2.ViewModels;

public class BlockDetailsViewModel
{
    public Block Block { get; set; } = null!;
    public int Confirmations { get; set; }
    public int LastBlockHeight { get; set; }
}
