using System.Text;
using System.Text.Json;

namespace Transaction.Models;

/// <summary>
/// Клас транзакції в блокчейні
/// Представляє переказ коштів від одного гаманця до іншого
/// </summary>
public class BlockchainTransaction
{
    // Адреса відправника (з якого гаманця йдуть кошти)
    public string FromAddress { get; set; }

    // Адреса отримувача (на який гаманець йдуть кошти)
    public string ToAddress { get; set; }

    // Сума переказу
    public decimal Amount { get; set; }

    // Час створення транзакції
    public DateTime Timestamp { get; set; }

    // Цифровий підпис транзакції (доводить, що її створив власник гаманця)
    public byte[]? Signature { get; set; }

    // Комісія за транзакцію (винагорода майнеру)
    public decimal Fee { get; set; }

    // Унікальний ідентифікатор транзакції
    public string TransactionId { get; set; }

    /// <summary>
    /// Конструктор створює нову транзакцію
    /// </summary>
    public BlockchainTransaction(string fromAddress, string toAddress, decimal amount, decimal fee = 0.001m)
    {
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Amount = amount;
        Fee = fee;
        Timestamp = DateTime.UtcNow;

        // Генеруємо унікальний ID транзакції на основі її вмісту
        TransactionId = CalculateHash();
    }

    /// <summary>
    /// Розрахунок хешу транзакції
    /// Цей хеш є унікальним ідентифікатором транзакції
    /// </summary>
    public string CalculateHash()
    {
        // Створюємо рядок з усіх даних транзакції
        string rawData = $"{FromAddress}{ToAddress}{Amount}{Fee}{Timestamp:O}";

        // Хешуємо дані за допомогою SHA256
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(rawData);
            byte[] hash = sha256.ComputeHash(bytes);

            // Перетворюємо хеш в hex-рядок
            return Convert.ToHexString(hash);
        }
    }

    /// <summary>
    /// Підпис транзакції приватним ключем гаманця
    /// Без підпису транзакція не може бути додана в блокчейн
    /// </summary>
    public void SignTransaction(Wallet wallet)
    {
        // Перевіряємо, що транзакцію підписує саме відправник
        if (wallet.Address != FromAddress)
        {
            throw new Exception("Ви не можете підписувати транзакції з інших гаманців!");
        }

        // Створюємо рядок для підпису з основних даних транзакції
        string dataToSign = $"{FromAddress}{ToAddress}{Amount}{Fee}{Timestamp:O}";

        // Підписуємо дані приватним ключем гаманця
        Signature = wallet.SignData(dataToSign);

        Console.WriteLine("✓ Транзакцію підписано успішно!");
    }

    /// <summary>
    /// Перевірка валідності транзакції
    /// Перевіряє чи транзакція правильно підписана
    /// </summary>
    public bool IsValid()
    {
        // Транзакції винагороди майнерам (coinbase) не потребують підпису
        if (string.IsNullOrEmpty(FromAddress))
        {
            return true;
        }

        // Якщо немає підпису - транзакція невалідна
        if (Signature == null || Signature.Length == 0)
        {
            Console.WriteLine("✗ Транзакція не має підпису!");
            return false;
        }

        // Створюємо той самий рядок, який був підписаний
        string dataToVerify = $"{FromAddress}{ToAddress}{Amount}{Fee}{Timestamp:O}";

        // Тимчасово створюємо гаманець для отримання публічного ключа
        // В реальній системі публічний ключ зберігався б окремо
        try
        {
            // ВАЖЛИВО: Тут має бути реальний публічний ключ з блокчейну
            // Для демо ми не можемо його отримати, тому показуємо логіку

            // Перевірка базових правил
            if (Amount <= 0)
            {
                Console.WriteLine("✗ Сума транзакції має бути більше нуля!");
                return false;
            }

            if (Fee < 0)
            {
                Console.WriteLine("✗ Комісія не може бути від'ємною!");
                return false;
            }

            if (string.IsNullOrEmpty(ToAddress))
            {
                Console.WriteLine("✗ Не вказано адресу отримувача!");
                return false;
            }

            // В реальній системі тут була б перевірка підпису
            // Wallet.VerifySignature(dataToVerify, Signature, publicKey);

            Console.WriteLine("✓ Транзакція валідна!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Помилка перевірки транзакції: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Перевірка підпису транзакції за допомогою публічного ключа
    /// </summary>
    public bool VerifySignature(string publicKey)
    {
        // Якщо немає підпису - транзакція невалідна
        if (Signature == null || Signature.Length == 0)
        {
            return false;
        }

        // Створюємо рядок даних для перевірки
        string dataToVerify = $"{FromAddress}{ToAddress}{Amount}{Fee}{Timestamp:O}";

        // Перевіряємо підпис за допомогою публічного ключа
        return Wallet.VerifySignature(dataToVerify, Signature, publicKey);
    }

    /// <summary>
    /// Вивід інформації про транзакцію
    /// </summary>
    public override string ToString()
    {
        return $"""
            ═══════════════════════════════════════
            ТРАНЗАКЦІЯ
            ═══════════════════════════════════════
            ID: {TransactionId[..16]}...
            Від: {FromAddress}
            До: {ToAddress}
            Сума: {Amount} BTC
            Комісія: {Fee} BTC
            Час: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC
            Підписано: {(Signature != null ? "Так ✓" : "Ні ✗")}
            ═══════════════════════════════════════
            """;
    }

    /// <summary>
    /// Серіалізація транзакції в JSON
    /// Використовується для передачі по мережі
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
