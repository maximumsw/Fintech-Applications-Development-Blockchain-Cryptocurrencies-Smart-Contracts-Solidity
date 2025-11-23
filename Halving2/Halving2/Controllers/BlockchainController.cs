using Microsoft.AspNetCore.Mvc;
using Halving2.Models;
using Halving2.Services;
using Halving2.ViewModels;

namespace Halving2.Controllers;

/// <summary>
/// Контролер для роботи з блокчейном
/// Обробляє всі запити пов'язані з переглядом блоків, гаманців, створенням транзакцій та майнінгом
/// </summary>
public class BlockchainController : Controller
{
    private readonly BlockchainService _blockchainService;
    private readonly ILogger<BlockchainController> _logger;

    public BlockchainController(BlockchainService blockchainService, ILogger<BlockchainController> logger)
    {
        _blockchainService = blockchainService;
        _logger = logger;
    }

    /// <summary>
    /// GET: Blockchain/Index - Відображає список всіх блоків в ланцюзі
    /// </summary>
    public IActionResult Index()
    {
        var blocks = _blockchainService.Chain;
        return View(blocks);
    }

    /// <summary>
    /// GET: Blockchain/BlockDetails/5 - Відображає деталі блоку з транзакціями та підтвердженнями
    /// Завдання 2: Показує кількість підтверджень для кожної транзакції в блоці
    /// </summary>
    public IActionResult BlockDetails(int height)
    {
        var block = _blockchainService.Chain.FirstOrDefault(b => b.Height == height);
        if (block == null)
        {
            return NotFound();
        }

        var lastBlockHeight = _blockchainService.Chain.Count - 1;
        var confirmations = _blockchainService.GetConfirmations(height);

        var viewModel = new BlockDetailsViewModel
        {
            Block = block,
            Confirmations = confirmations,
            LastBlockHeight = lastBlockHeight
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET: Blockchain/Wallets - Відображає всі гаманці з балансами
    /// </summary>
    public IActionResult Wallets()
    {
        var addresses = _blockchainService.GetAllAddresses();
        var wallets = addresses.Select(addr => new WalletViewModel
        {
            Address = addr,
            ConfirmedBalance = _blockchainService.GetBalance(addr),
            TotalBalance = _blockchainService.GetBalanceWithPending(addr)
        }).ToList();

        return View(wallets);
    }

    /// <summary>
    /// GET: Blockchain/WalletHistory/Alice - Відображає історію транзакцій гаманця
    /// Завдання 3: Показує всі транзакції (вхідні/вихідні) з підтвердженнями та статусом
    /// </summary>
    public IActionResult WalletHistory(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            return BadRequest();
        }

        var history = _blockchainService.GetWalletHistory(address);
        var lastBlockHeight = _blockchainService.Chain.Count - 1;

        var viewModel = new WalletHistoryViewModel
        {
            Address = address,
            ConfirmedBalance = _blockchainService.GetBalance(address),
            TotalBalance = _blockchainService.GetBalanceWithPending(address),
            Transactions = history.Select(tx => new WalletTransactionViewModel
            {
                Transaction = tx,
                Direction = tx.ToAddress == address ? "Incoming" : "Outgoing",
                IsInMempool = tx.BlockHeight == null,
                Confirmations = tx.BlockHeight.HasValue
                    ? _blockchainService.GetConfirmations(tx.BlockHeight.Value)
                    : 0
            }).OrderByDescending(t => t.Transaction.BlockHeight ?? int.MaxValue)
              .ToList(),
            LastBlockHeight = lastBlockHeight
        };

        return View(viewModel);
    }

    /// <summary>
    /// GET: Blockchain/CreateTransaction - Форма для створення нової транзакції
    /// </summary>
    public IActionResult CreateTransaction()
    {
        var addresses = _blockchainService.GetAllAddresses();
        ViewBag.Addresses = addresses;
        return View();
    }

    /// <summary>
    /// POST: Blockchain/CreateTransaction - Обробка створення транзакції
    /// Додає транзакцію до мемпулу після валідації
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateTransaction(string fromAddress, string toAddress, decimal amount, decimal fee, string? note)
    {
        try
        {
            var transaction = new Transaction(fromAddress, toAddress, amount, fee, note);
            _blockchainService.AddTransaction(transaction);
            TempData["Success"] = $"Транзакцію додано до мемпулу. Від: {fromAddress}, До: {toAddress}, Сума: {amount}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Не вдалося додати транзакцію: {ex.Message}";
            var addresses = _blockchainService.GetAllAddresses();
            ViewBag.Addresses = addresses;
            return View();
        }
    }

    /// <summary>
    /// GET: Blockchain/Mine - Сторінка для майнінгу нового блоку
    /// Завдання 1: Показує винагороду за наступний блок (з урахуванням halving)
    /// </summary>
    public IActionResult Mine()
    {
        var addresses = _blockchainService.GetAllAddresses();
        ViewBag.Addresses = addresses;
        ViewBag.MempoolCount = _blockchainService.Mempool.Count;
        ViewBag.NextBlockHeight = _blockchainService.Chain.Count;
        ViewBag.NextBlockReward = _blockchainService.GetBlockReward(_blockchainService.Chain.Count);
        return View();
    }

    /// <summary>
    /// POST: Blockchain/Mine - Майнить новий блок
    /// Виконує Proof-of-Work та додає блок до ланцюга
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Mine(string minerAddress)
    {
        try
        {
            if (string.IsNullOrEmpty(minerAddress))
            {
                TempData["Error"] = "Будь ласка, оберіть адресу майнера";
                return RedirectToAction(nameof(Mine));
            }

            if (_blockchainService.Mempool.Count == 0)
            {
                TempData["Error"] = "В мемпулі немає транзакцій для майнінгу";
                return RedirectToAction(nameof(Mine));
            }

            var blockHeight = _blockchainService.Chain.Count;
            _blockchainService.MinePending(minerAddress);
            TempData["Success"] = $"Блок {blockHeight} успішно намайнений гаманцем {minerAddress}!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Помилка майнінгу: {ex.Message}";
            return RedirectToAction(nameof(Mine));
        }
    }

    /// <summary>
    /// GET: Blockchain/Mempool - Відображає список транзакцій, що очікують на майнінг
    /// </summary>
    public IActionResult Mempool()
    {
        var mempool = _blockchainService.Mempool;
        return View(mempool);
    }
}
