using Transaction.Models;

// ═══════════════════════════════════════════════════════════════
// ДЕМОНСТРАЦІЯ РОБОТИ БЛОКЧЕЙН-СИСТЕМИ
// Практична робота з блокчейн-технологіями
// ═══════════════════════════════════════════════════════════════

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
Console.WriteLine("║     ДЕМОНСТРАЦІЯ БЛОКЧЕЙН ТРАНЗАКЦІЙ ТА МАЙНІНГУ          ║");
Console.WriteLine("║              Практична робота                              ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

// ═══════════════════════════════════════════════════════════════
// КРОК 1: Створення гаманців
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n📝 КРОК 1: Створення гаманців для користувачів\n");

// Створюємо гаманець для Олени
Console.WriteLine("Створюємо гаманець для Олени...");
var walletOlena = new Wallet();
Console.WriteLine($"✓ Гаманець Олени створено!");
Console.WriteLine($"  Адреса: {walletOlena.Address}");
Console.WriteLine($"  Публічний ключ: {walletOlena.PublicKey[..50]}...");

// Створюємо гаманець для Андрія
Console.WriteLine("\nСтворюємо гаманець для Андрія...");
var walletAndriy = new Wallet();
Console.WriteLine($"✓ Гаманець Андрія створено!");
Console.WriteLine($"  Адреса: {walletAndriy.Address}");
Console.WriteLine($"  Публічний ключ: {walletAndriy.PublicKey[..50]}...");

// Створюємо гаманець для майнера
Console.WriteLine("\nСтворюємо гаманець для майнера...");
var walletMiner = new Wallet();
Console.WriteLine($"✓ Гаманець майнера створено!");
Console.WriteLine($"  Адреса: {walletMiner.Address}");

// ═══════════════════════════════════════════════════════════════
// КРОК 2: Створення та підпис транзакцій
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n\n📝 КРОК 2: Створення та підпис транзакцій\n");

// Олена хоче відправити 5 BTC Андрію
Console.WriteLine("Олена створює транзакцію для відправки 5 BTC Андрію...");
var transaction1 = new BlockchainTransaction(
    fromAddress: walletOlena.Address,
    toAddress: walletAndriy.Address,
    amount: 5.0m,
    fee: 0.001m
);

Console.WriteLine("\nДані транзакції до підпису:");
Console.WriteLine(transaction1);

// Олена підписує транзакцію своїм приватним ключем
Console.WriteLine("Олена підписує транзакцію...");
transaction1.SignTransaction(walletOlena);

Console.WriteLine("\nДані транзакції після підпису:");
Console.WriteLine(transaction1);

// Створюємо ще одну транзакцію: Андрій відправляє 2 BTC Олені
Console.WriteLine("\n\nАндрій створює транзакцію для відправки 2 BTC Олені...");
var transaction2 = new BlockchainTransaction(
    fromAddress: walletAndriy.Address,
    toAddress: walletOlena.Address,
    amount: 2.0m,
    fee: 0.002m  // Вища комісія для швидшого опрацювання
);

Console.WriteLine("Андрій підписує транзакцію...");
transaction2.SignTransaction(walletAndriy);
Console.WriteLine(transaction2);

// ═══════════════════════════════════════════════════════════════
// КРОК 3: Додавання транзакцій в Mempool
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n\n📝 КРОК 3: Додавання транзакцій в Mempool\n");

// Створюємо mempool
var mempool = new Mempool();
Console.WriteLine("✓ Mempool створено\n");

// Додаємо транзакції в mempool
Console.WriteLine("Додаємо транзакцію Олени в mempool...");
mempool.AddTransaction(transaction1);

Console.WriteLine("\nДодаємо транзакцію Андрія в mempool...");
mempool.AddTransaction(transaction2);

// Створюємо ще кілька транзакцій для демонстрації
Console.WriteLine("\nСтворюємо додаткові транзакції...");
for (int i = 0; i < 3; i++)
{
    var tx = new BlockchainTransaction(
        fromAddress: walletOlena.Address,
        toAddress: walletAndriy.Address,
        amount: 0.5m + i,
        fee: 0.0001m * (i + 1)
    );
    tx.SignTransaction(walletOlena);
    mempool.AddTransaction(tx);
}

// Показуємо статистику mempool
mempool.PrintStats();

// ═══════════════════════════════════════════════════════════════
// КРОК 4: Перевірка валідності транзакцій
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n📝 КРОК 4: Перевірка валідності транзакцій\n");

Console.WriteLine("Перевіряємо транзакцію Олени:");
bool isValid1 = transaction1.IsValid();

Console.WriteLine("\nПеревіряємо транзакцію Андрія:");
bool isValid2 = transaction2.IsValid();

// Демонструємо перевірку підпису
Console.WriteLine("\n\nПеревірка цифрового підпису транзакції Олени:");
bool signatureValid = transaction1.VerifySignature(walletOlena.PublicKey);
Console.WriteLine($"Результат перевірки підпису: {(signatureValid ? "✓ Валідний" : "✗ Невалідний")}");

// ═══════════════════════════════════════════════════════════════
// КРОК 5: Майнінг блоку
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n\n📝 КРОК 5: Майнінг блоку\n");

// Майнер обирає транзакції з найвищою комісією
Console.WriteLine("Майнер обирає транзакції з mempool...");
var transactionsForBlock = mempool.GetTopTransactionsByFee(3);
Console.WriteLine($"✓ Обрано {transactionsForBlock.Count} транзакцій з найвищою комісією\n");

// Показуємо обрані транзакції
Console.WriteLine("Транзакції, які будуть включені в блок:");
foreach (var tx in transactionsForBlock)
{
    Console.WriteLine($"  • Від {tx.FromAddress[..16]}... → До {tx.ToAddress[..16]}...");
    Console.WriteLine($"    Сума: {tx.Amount} BTC, Комісія: {tx.Fee} BTC");
}

// Створюємо новий блок
Console.WriteLine("\n\nСтворюємо новий блок...");
var block = new Block(
    index: 1,
    transactions: transactionsForBlock,
    previousHash: "0000000000000000000000000000000000000000000000000000000000000000",
    minerAddress: walletMiner.Address
);

// Додаємо винагороду майнеру
decimal blockReward = 6.25m;
decimal totalFees = transactionsForBlock.Sum(t => t.Fee);
block.AddMinerReward(blockReward + totalFees);

Console.WriteLine($"\nВинагорода майнера:");
Console.WriteLine($"  Базова винагорода: {blockReward} BTC");
Console.WriteLine($"  Сума комісій: {totalFees} BTC");
Console.WriteLine($"  Загальна винагорода: {blockReward + totalFees} BTC");

// Майнимо блок
block.MineBlock();

// Показуємо інформацію про змайнений блок
Console.WriteLine(block);

// ═══════════════════════════════════════════════════════════════
// КРОК 6: Валідація блоку
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n📝 КРОК 6: Валідація блоку\n");

Console.WriteLine("Перевіряємо валідність блоку...");
bool isBlockValid = block.IsValid();

if (isBlockValid)
{
    Console.WriteLine("\n✓✓✓ Блок успішно пройшов валідацію! ✓✓✓");
    Console.WriteLine("Блок може бути доданий в блокчейн.");

    // Видаляємо опрацьовані транзакції з mempool
    Console.WriteLine("\nВидаляємо опрацьовані транзакції з mempool...");
    mempool.RemoveTransactions(transactionsForBlock);

    // Показуємо оновлену статистику mempool
    mempool.PrintStats();
}

// ═══════════════════════════════════════════════════════════════
// КРОК 7: Демонстрація невалідної транзакції
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n📝 КРОК 7: Демонстрація невалідної транзакції\n");

Console.WriteLine("Спроба створити невалідну транзакцію (від'ємна сума)...");
try
{
    var invalidTx = new BlockchainTransaction(
        fromAddress: walletOlena.Address,
        toAddress: walletAndriy.Address,
        amount: -10.0m,  // Невалідна сума
        fee: 0.001m
    );
    invalidTx.SignTransaction(walletOlena);

    Console.WriteLine("Перевіряємо невалідну транзакцію:");
    invalidTx.IsValid();
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Помилка: {ex.Message}");
}

Console.WriteLine("\n\nСпроба підписати транзакцію не своїм гаманцем...");
try
{
    var wrongTx = new BlockchainTransaction(
        fromAddress: walletOlena.Address,
        toAddress: walletAndriy.Address,
        amount: 1.0m,
        fee: 0.001m
    );

    // Андрій намагається підписати транзакцію Олени
    Console.WriteLine("Андрій намагається підписати транзакцію з гаманця Олени...");
    wrongTx.SignTransaction(walletAndriy);
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Помилка (очікувана): {ex.Message}");
    Console.WriteLine("✓ Система правильно відхилила спробу підпису чужої транзакції!");
}

// ═══════════════════════════════════════════════════════════════
// ЗАВЕРШЕННЯ
// ═══════════════════════════════════════════════════════════════
Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                  ДЕМОНСТРАЦІЮ ЗАВЕРШЕНО                    ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

Console.WriteLine("\n📚 Що було продемонстровано:\n");
Console.WriteLine("✓ Створення гаманців з криптографічними ключами");
Console.WriteLine("✓ Генерація приватних та публічних ключів");
Console.WriteLine("✓ Створення транзакцій");
Console.WriteLine("✓ Цифровий підпис транзакцій");
Console.WriteLine("✓ Перевірка валідності підпису");
Console.WriteLine("✓ Додавання транзакцій в Mempool");
Console.WriteLine("✓ Вибір транзакцій майнером за комісією");
Console.WriteLine("✓ Майнінг блоку (Proof of Work)");
Console.WriteLine("✓ Валідація блоку");
Console.WriteLine("✓ Захист від невалідних транзакцій");

Console.WriteLine("\n📖 Детальну теорію дивіться у файлі Theory.md");
Console.WriteLine("\n✨ Програма завершена успішно!\n");
