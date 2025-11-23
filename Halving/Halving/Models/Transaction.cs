namespace Halving.Models;

/// <summary>
/// Транзакція - переказ коштів між адресами
/// </summary>
public class Transaction
{
    public string FromAddress { get; set; }
    public string ToAddress { get; set; }
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public string? Note { get; set; }

    public Transaction()
    {
        FromAddress = string.Empty;
        ToAddress = string.Empty;
        Amount = 0;
        Fee = 0;
    }

    public Transaction(string fromAddress, string toAddress, decimal amount, decimal fee = 0, string? note = null)
    {
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Amount = amount;
        Fee = fee;
        Note = note;
    }

    // Перевірка чи це coinbase транзакція (винагорода за майнінг)
    public bool IsCoinbase()
    {
        return FromAddress == "COINBASE";
    }
}
