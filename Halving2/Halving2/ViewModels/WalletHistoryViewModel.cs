namespace Halving2.ViewModels;

public class WalletHistoryViewModel
{
    public string Address { get; set; } = string.Empty;
    public decimal ConfirmedBalance { get; set; }
    public decimal TotalBalance { get; set; }
    public List<WalletTransactionViewModel> Transactions { get; set; } = new();
    public int LastBlockHeight { get; set; }
}
