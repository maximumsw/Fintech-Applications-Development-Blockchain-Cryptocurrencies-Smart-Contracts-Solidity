using Microsoft.AspNetCore.Mvc;
using Smart_Contracts.Models;
using Smart_Contracts.Services;

namespace Smart_Contracts.Controllers;

public class BlockChainController : Controller
{
    private static readonly BlockChainService _blockChain = new();
    private static readonly Dictionary<string, string> _wallets = new();

    static BlockChainController()
    {
        // Initialize with some demo wallets
        _wallets["Alice"] = "alice-private-key";
        _wallets["Bob"] = "bob-private-key";
        _wallets["Charlie"] = "charlie-private-key";

        // Give Alice some initial funds
        var genesisTransaction = new Transaction("", "Alice", 100m);
        _blockChain.PendingTransactions.Add(genesisTransaction);
        _blockChain.MinePendingTransactions("Miner");
    }

    public IActionResult Index()
    {
        ViewBag.Wallets = _wallets.Keys.ToList();
        ViewBag.BlockChain = _blockChain;
        return View();
    }

    public IActionResult Wallet()
    {
        var walletData = new List<object>();

        foreach (var wallet in _wallets.Keys)
        {
            var balance = _blockChain.GetBalanceOfAddress(wallet);
            var contract = _blockChain.GetContract(wallet);
            var contractType = contract?.GetType().Name ?? "";

            walletData.Add(new
            {
                Address = wallet,
                Balance = balance,
                IsContract = contract != null,
                ContractType = contractType
            });
        }

        ViewBag.WalletData = walletData;
        ViewBag.Contracts = _blockChain.Contracts;
        return View();
    }

    [HttpGet]
    public IActionResult CreateTransaction()
    {
        ViewBag.Wallets = _wallets.Keys.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult CreateTransaction(string fromAddress, string toAddress, decimal amount)
    {
        try
        {
            if (!_wallets.ContainsKey(fromAddress))
            {
                TempData["Error"] = "Invalid sender address";
                return RedirectToAction(nameof(Wallet));
            }

            var transaction = new Transaction(fromAddress, toAddress, amount);
            transaction.SignTransaction(_wallets[fromAddress]);

            _blockChain.AddTransaction(transaction);

            TempData["Success"] = $"Transaction added to pending transactions. From: {fromAddress}, To: {toAddress}, Amount: {amount}";
            return RedirectToAction(nameof(Wallet));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Transaction failed: {ex.Message}";
            return RedirectToAction(nameof(Wallet));
        }
    }

    [HttpPost]
    public IActionResult MineBlock(string minerAddress)
    {
        try
        {
            if (string.IsNullOrEmpty(minerAddress))
                minerAddress = "Miner";

            _blockChain.MinePendingTransactions(minerAddress);
            TempData["Success"] = $"Block mined successfully! Reward sent to {minerAddress}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Mining failed: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CreateContract()
    {
        ViewBag.Wallets = _wallets.Keys.ToList();
        ViewBag.CurrentBlockIndex = _blockChain.Chain.Count;
        return View();
    }

    [HttpPost]
    public IActionResult CreateTimeLockContract(string address, int unlockBlockIndex)
    {
        try
        {
            if (string.IsNullOrEmpty(address))
            {
                TempData["Error"] = "Address is required";
                return RedirectToAction(nameof(Wallet));
            }

            if (unlockBlockIndex <= _blockChain.Chain.Count)
            {
                TempData["Error"] = $"Unlock block must be greater than current block ({_blockChain.Chain.Count})";
                return RedirectToAction(nameof(Wallet));
            }

            var contract = new TimeLockContract(address, unlockBlockIndex);
            _blockChain.RegisterContract(contract);

            TempData["Success"] = $"TimeLock contract created for {address}. Funds locked until block {unlockBlockIndex}";
            return RedirectToAction(nameof(Wallet));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Contract creation failed: {ex.Message}";
            return RedirectToAction(nameof(Wallet));
        }
    }

    public IActionResult ContractDetails(string address)
    {
        var contract = _blockChain.GetContract(address);

        if (contract == null)
        {
            TempData["Error"] = "Contract not found";
            return RedirectToAction(nameof(Wallet));
        }

        ViewBag.Contract = contract;
        ViewBag.CurrentBlockIndex = _blockChain.Chain.Count;
        ViewBag.Address = address;

        return View();
    }

    public IActionResult Transactions(string address)
    {
        var transactions = _blockChain.GetTransactionsForAddress(address);
        ViewBag.Address = address;
        ViewBag.Transactions = transactions;
        return View();
    }
}
