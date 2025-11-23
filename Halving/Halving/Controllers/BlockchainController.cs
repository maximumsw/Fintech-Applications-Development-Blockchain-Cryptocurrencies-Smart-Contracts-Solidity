using Microsoft.AspNetCore.Mvc;
using Halving.Models;
using Halving.Services;

namespace Halving.Controllers;

/// <summary>
/// Контролер для управління блокчейном - майнінг, транзакції, перегляд балансів
/// </summary>
public class BlockchainController : Controller
{
    private readonly BlockChainService _blockChainService;
    private readonly ILogger<BlockchainController> _logger;

    public BlockchainController(BlockChainService blockChainService, ILogger<BlockchainController> logger)
    {
        _blockChainService = blockChainService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.CurrentDifficulty = _blockChainService.Difficulty;
        ViewBag.AverageMiningTime = _blockChainService.GetAverageMiningTime();
        ViewBag.TargetBlockTime = _blockChainService.TargetBlockTimeSeconds;
        ViewBag.MiningReward = _blockChainService.MiningReward;
        ViewBag.MempoolCount = _blockChainService.Mempool.Count;

        return View(_blockChainService.Chain);
    }

    [HttpPost]
    public IActionResult AddTransaction(string fromAddress, string toAddress, decimal amount, decimal fee = 0, string? note = null)
    {
        try
        {
            var transaction = new Transaction(fromAddress, toAddress, amount, fee, note);
            _blockChainService.AddTransaction(transaction);
            TempData["Success"] = $"Transaction added to mempool. From: {fromAddress}, To: {toAddress}, Amount: {amount}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to add transaction: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Mine(string minerAddress)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(minerAddress))
            {
                TempData["Error"] = "Miner address is required";
                return RedirectToAction(nameof(Index));
            }

            var block = _blockChainService.MinePending(minerAddress);
            TempData["Success"] = $"Block #{block.Index} mined successfully! Mining took {block.MiningDurationMs}ms. Difficulty: {block.Difficulty}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Mining failed: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Balance(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            ViewBag.Error = "Address is required";
            return View();
        }

        ViewBag.Address = address;
        ViewBag.Balance = _blockChainService.GetBalance(address);

        return View();
    }

    public IActionResult Validate()
    {
        var isValid = _blockChainService.ValidateChain();
        ViewBag.IsValid = isValid;
        ViewBag.Message = isValid ? "Blockchain is valid!" : "Blockchain validation failed!";

        return View();
    }

    public IActionResult Mempool()
    {
        return View(_blockChainService.Mempool);
    }

    public IActionResult Balances()
    {
        return View(_blockChainService.Balances);
    }
}
