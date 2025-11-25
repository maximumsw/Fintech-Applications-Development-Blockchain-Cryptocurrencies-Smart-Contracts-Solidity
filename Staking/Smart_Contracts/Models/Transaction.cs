using System.Security.Cryptography;
using System.Text;

namespace Smart_Contracts.Models;

/// <summary>
/// Транзакція - переказ коштів між адресами
/// </summary>
public class Transaction
{
    /// <summary>
    /// Адреса відправника (пуста для винагороди за майнінг)
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Адреса отримувача
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Сума переказу
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Мітка часу створення транзакції
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Цифровий підпис транзакції
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    public Transaction(string fromAddress, string toAddress, decimal amount)
    {
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Amount = amount;
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string CalculateHash()
    {
        var rawData = $"{FromAddress}{ToAddress}{Amount}{Timestamp}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Підписує транзакцію приватним ключем
    /// </summary>
    public void SignTransaction(string privateKey)
    {
        if (string.IsNullOrEmpty(FromAddress))
            throw new Exception("Cannot sign transaction without a from address");

        var hash = CalculateHash();
        Signature = GenerateSignature(hash, privateKey);
    }

    /// <summary>
    /// Перевіряє валідність транзакції (чи підписана правильно)
    /// </summary>
    public bool IsValid()
    {
        // Транзакції винагороди за майнінг не мають відправника і завжди валідні
        if (string.IsNullOrEmpty(FromAddress))
            return true;

        // Транзакція без підпису невалідна
        if (string.IsNullOrEmpty(Signature))
            return false;

        var hash = CalculateHash();
        return VerifySignature(hash, Signature, FromAddress);
    }

    /// <summary>
    /// Генерує цифровий підпис (спрощена реалізація для демонстрації)
    /// </summary>
    private string GenerateSignature(string data, string privateKey)
    {
        // Спрощений підпис: Hash(дані + приватний ключ)
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data + privateKey));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Перевіряє цифровий підпис (спрощена реалізація для демонстрації)
    /// </summary>
    private bool VerifySignature(string data, string signature, string publicKey)
    {
        // Спрощена перевірка: для демонстраційних цілей
        // У продакшені використовуйте справжню криптографію (RSA, ECDSA)
        return !string.IsNullOrEmpty(signature);
    }
}
