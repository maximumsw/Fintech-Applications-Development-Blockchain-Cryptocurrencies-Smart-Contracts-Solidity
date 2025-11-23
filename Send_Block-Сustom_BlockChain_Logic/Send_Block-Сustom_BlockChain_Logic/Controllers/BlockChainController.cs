using Microsoft.AspNetCore.Mvc;
using Send_Block_Сustom_BlockChain_Logic.Services;
using Send_Block_Сustom_BlockChain_Logic.ViewModels;

namespace Send_Block_Сustom_BlockChain_Logic.Controllers;

public class BlockChainController : Controller
{
    private readonly NodeRegistryService _nodeRegistry;

    public BlockChainController(NodeRegistryService nodeRegistry)
    {
        _nodeRegistry = nodeRegistry;
    }

    public IActionResult Index(string nodeId = "A")
    {
        var node = _nodeRegistry.GetNode(nodeId);

        var viewModel = new BlockChainViewModel
        {
            CurrentNodeId = nodeId,
            AllNodeIds = _nodeRegistry.GetAllNodeIds().ToList(),
            Chain = node.Chain,
            Mempool = node.Mempool,
            Wallets = node.Wallets,
            Difficulty = node.Difficulty,
            MiningReward = node.MiningReward
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult SetDifficulty(string nodeId, int difficulty)
    {
        try
        {
            var node = _nodeRegistry.GetNode(nodeId);
            node.SetDifficulty(difficulty);
            TempData["Success"] = $"Difficulty set to {difficulty} on Node {nodeId}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index", new { nodeId });
    }

    [HttpPost]
    public IActionResult RegisterWallet(string nodeId, string walletName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(walletName))
            {
                TempData["Error"] = "Wallet name is required";
                return RedirectToAction("Index", new { nodeId });
            }

            var node = _nodeRegistry.GetNode(nodeId);

            if (node.Wallets.ContainsKey(walletName))
            {
                TempData["Error"] = $"Wallet '{walletName}' already exists on Node {nodeId}";
                return RedirectToAction("Index", new { nodeId });
            }

            var wallet = node.RegisterWallet(walletName);
            TempData["Success"] = $"Wallet '{walletName}' registered on Node {nodeId}. Address: {wallet.Address}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index", new { nodeId });
    }

    [HttpPost]
    public IActionResult CreateTransaction(string nodeId, string fromWallet, string toAddress, decimal amount)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fromWallet) || string.IsNullOrWhiteSpace(toAddress))
            {
                TempData["Error"] = "From wallet and to address are required";
                return RedirectToAction("Index", new { nodeId });
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than 0";
                return RedirectToAction("Index", new { nodeId });
            }

            var node = _nodeRegistry.GetNode(nodeId);
            node.CreateTransaction(fromWallet, toAddress, amount);
            TempData["Success"] = $"Transaction created: {fromWallet} → {toAddress} ({amount} coins) on Node {nodeId}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index", new { nodeId });
    }

    [HttpPost]
    public IActionResult MinePending(string nodeId, string minerWallet)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(minerWallet))
            {
                TempData["Error"] = "Miner wallet is required";
                return RedirectToAction("Index", new { nodeId });
            }

            var node = _nodeRegistry.GetNode(nodeId);
            var block = node.MinePendingTransactions(minerWallet);

            TempData["Success"] = $"Block #{block.Index} mined successfully on Node {nodeId} by {minerWallet}! " +
                                  $"Hash: {block.Hash[..16]}... Nonce: {block.Nonce}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index", new { nodeId });
    }

    [HttpPost]
    public IActionResult BroadcastLastBlock(string nodeId)
    {
        try
        {
            var sourceNode = _nodeRegistry.GetNode(nodeId);

            if (sourceNode.Chain.Count <= 1)
            {
                TempData["Error"] = "No blocks to broadcast (only genesis block exists)";
                return RedirectToAction("Index", new { nodeId });
            }

            var results = _nodeRegistry.BroadcastBlock(nodeId);
            var lastBlock = sourceNode.GetLatestBlock();

            var acceptedCount = results.Count(r => r.Value);
            var totalNodes = results.Count;

            var acceptedNodes = string.Join(", ", results.Where(r => r.Value).Select(r => r.Key));
            var rejectedNodes = string.Join(", ", results.Where(r => !r.Value).Select(r => r.Key));

            var message = $"Block #{lastBlock.Index} broadcasted from Node {nodeId}. " +
                          $"Accepted: {acceptedCount} of {totalNodes} nodes.";

            if (!string.IsNullOrEmpty(acceptedNodes))
                message += $" Accepted by: {acceptedNodes}.";

            if (!string.IsNullOrEmpty(rejectedNodes))
                message += $" Rejected by: {rejectedNodes}.";

            TempData["Success"] = message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index", new { nodeId });
    }

    [HttpPost]
    public IActionResult DemoSetup(string nodeId)
    {
        try
        {
            var node = _nodeRegistry.GetNode(nodeId);
            node.DemoSetup();
            TempData["Success"] = $"Demo wallets created on Node {nodeId}: Alice, Bob, Charlie";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Index", new { nodeId });
    }

    [HttpGet]
    public IActionResult GetBalance(string nodeId, string address)
    {
        try
        {
            var node = _nodeRegistry.GetNode(nodeId);
            var balance = node.GetBalance(address);
            return Json(new { success = true, balance });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
}
