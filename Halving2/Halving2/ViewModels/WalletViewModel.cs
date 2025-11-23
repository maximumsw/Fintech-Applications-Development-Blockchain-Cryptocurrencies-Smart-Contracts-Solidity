namespace Halving2.ViewModels;

public class WalletViewModel
{
    public string Address { get; set; } = string.Empty;
    public decimal ConfirmedBalance { get; set; }
    public decimal TotalBalance { get; set; }
}
