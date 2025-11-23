namespace Halving2.Models;

/// <summary>
/// Модель транзакції в блокчейні
/// Представляє переказ коштів від одного адреси до іншого
/// </summary>
public class Transaction
{
    // Адреса відправника
    public string FromAddress { get; set; } = string.Empty;

    // Адреса одержувача
    public string ToAddress { get; set; } = string.Empty;

    // Сума переказу
    public decimal Amount { get; set; }

    // Комісія за транзакцію (отримує майнер)
    public decimal Fee { get; set; }

    // Опціональна примітка до транзакції
    public string? Note { get; set; }

    // Висота блоку, в який включена транзакція (null якщо в мемпулі)
    public int? BlockHeight { get; set; }

    public Transaction()
    {
    }

    public Transaction(string fromAddress, string toAddress, decimal amount, decimal fee = 0, string? note = null)
    {
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Amount = amount;
        Fee = fee;
        Note = note;
    }
}
