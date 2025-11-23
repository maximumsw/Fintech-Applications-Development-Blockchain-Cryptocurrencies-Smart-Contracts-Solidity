using Halving2.Models;

namespace Halving2.ViewModels;

public class WalletTransactionViewModel
{
    public Transaction Transaction { get; set; } = null!;
    public string Direction { get; set; } = string.Empty;
    public bool IsInMempool { get; set; }
    public int Confirmations { get; set; }
}
