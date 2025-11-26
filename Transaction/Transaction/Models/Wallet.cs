using System.Security.Cryptography;
using System.Text;

namespace Transaction.Models;

/// <summary>
/// Клас гаманця для роботи з криптовалютними транзакціями
/// </summary>
public class Wallet
{
    // Приватний ключ - це секретна інформація, яку ніколи не можна розкривати
    // Він використовується для підпису транзакцій
    public string PrivateKey { get; private set; } = string.Empty;

    // Публічний ключ - це адреса гаманця, яку можна показувати всім
    // Він використовується для перевірки підпису та отримання коштів
    public string PublicKey { get; private set; } = string.Empty;

    // Адреса гаманця - це скорочена версія публічного ключа
    // Її використовують для отримання переказів
    public string Address { get; private set; } = string.Empty;

    /// <summary>
    /// Конструктор створює новий гаманець з унікальною парою ключів
    /// </summary>
    public Wallet()
    {
        // Генеруємо нову пару ключів при створенні гаманця
        GenerateKeyPair();
    }

    /// <summary>
    /// Генерація пари ключів (приватний та публічний)
    /// Використовуємо алгоритм RSA для асиметричного шифрування
    /// </summary>
    private void GenerateKeyPair()
    {
        // Створюємо провайдер RSA з розміром ключа 2048 біт
        // Чим більший розмір, тим безпечніше, але повільніше
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            // Встановлюємо, що ключі можуть бути експортовані
            // Це потрібно для збереження приватного ключа
            rsa.PersistKeyInCsp = false;

            // Експортуємо приватний ключ у форматі XML
            // Цей ключ треба зберігати в секреті!
            PrivateKey = rsa.ToXmlString(true);

            // Експортуємо публічний ключ у форматі XML
            // Цей ключ можна показувати всім
            PublicKey = rsa.ToXmlString(false);

            // Створюємо адресу з публічного ключа
            // Використовуємо хеш для скорочення довжини
            Address = GenerateAddress(PublicKey);
        }
    }

    /// <summary>
    /// Генерація адреси гаманця з публічного ключа
    /// Використовуємо SHA256 хешування для створення унікальної короткої адреси
    /// </summary>
    private string GenerateAddress(string publicKey)
    {
        // Створюємо об'єкт для SHA256 хешування
        using (var sha256 = SHA256.Create())
        {
            // Перетворюємо публічний ключ в байти
            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);

            // Хешуємо публічний ключ
            byte[] hash = sha256.ComputeHash(publicKeyBytes);

            // Беремо перші 20 байт хешу (як у Bitcoin)
            byte[] addressBytes = new byte[20];
            Array.Copy(hash, addressBytes, 20);

            // Конвертуємо байти в hex-рядок (Base16)
            // Це і є наша адреса
            return Convert.ToHexString(addressBytes);
        }
    }

    /// <summary>
    /// Підпис даних приватним ключем
    /// Цей підпис доводить, що транзакцію створив власник гаманця
    /// </summary>
    public byte[] SignData(string data)
    {
        // Перетворюємо дані в байти для підпису
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        // Створюємо новий RSA провайдер
        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            rsa.PersistKeyInCsp = false;

            // Завантажуємо наш приватний ключ
            rsa.FromXmlString(PrivateKey);

            // Підписуємо дані приватним ключем
            // Використовуємо SHA256 для хешування даних перед підписом
            byte[] signature = rsa.SignData(dataBytes, SHA256.Create());

            // Повертаємо цифровий підпис
            return signature;
        }
    }

    /// <summary>
    /// Перевірка підпису за допомогою публічного ключа
    /// Це статичний метод, його може викликати будь-хто для перевірки
    /// </summary>
    public static bool VerifySignature(string data, byte[] signature, string publicKey)
    {
        // Перетворюємо дані в байти
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        try
        {
            // Створюємо RSA провайдер для перевірки
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;

                // Завантажуємо публічний ключ відправника
                rsa.FromXmlString(publicKey);

                // Перевіряємо, чи відповідає підпис даним і публічному ключу
                // Якщо так - транзакція справжня, якщо ні - підроблена
                return rsa.VerifyData(dataBytes, SHA256.Create(), signature);
            }
        }
        catch
        {
            // Якщо сталася помилка - підпис невалідний
            return false;
        }
    }

    /// <summary>
    /// Отримання балансу гаманця
    /// В реальній блокчейн-системі це б рахувалось з усіх блоків
    /// </summary>
    public decimal GetBalance()
    {
        // TODO: В реальній реалізації тут має бути підрахунок з блокчейну
        // Наразі це просто заглушка для демонстрації
        return 0;
    }

    public override string ToString()
    {
        return $"Адреса: {Address}";
    }
}
