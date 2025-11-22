using Microsoft.AspNetCore.Mvc;
using POW_MemPOOL.Models;
using POW_MemPOOL.Services;
using System;
using System.Diagnostics;

namespace POW_MemPOOL.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlockChainService _blockchainService;

        public HomeController(BlockChainService blockchainService)
        {
            _blockchainService = blockchainService;
        }

        public IActionResult Index()
        {
            var status = _blockchainService.GetMiningStatus();
            var isMining = status.Status == MiningStatus.InProgress;
            var isChainValid = _blockchainService.IsChainValid();

            var viewModel = new IndexViewModel(
                _blockchainService.GetBlockchain(),
                isMining,
                _blockchainService.Difficulty,
                isChainValid
            );

            // If mining just completed or was canceled, show the result and then reset.
            if (status.Status == MiningStatus.Completed || status.Status == MiningStatus.Canceled)
            {
                TempData["MiningResult"] = $"Mining {status.Status} in {status.ElapsedMs} ms.";
                // Reset status to avoid showing the message on next refresh
                 _blockchainService.GetMiningStatus().Status = MiningStatus.NotStarted;
            }

            //_blockchainService.GetBlockchain().Chain[1].Data = "BadBLock";
           
            return View(viewModel);
        }

        [HttpPost("Home/AddBlock")]
        public IActionResult AddBlock(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                _blockchainService.AddBlock(data);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SetDifficulty(int difficulty)
        {
            if (difficulty > 0 && difficulty < 10) // Basic validation
            {
                _blockchainService.Difficulty = difficulty;
            }
            return RedirectToAction("Index");
        }
        
        [HttpPost("Home/AddBlockAsync")]
        public IActionResult AddBlockAsync(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                _blockchainService.StartMiningAsync(data);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CancelMining()
        {
            _blockchainService.CancelMining();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetMiningStatus()
        {
            var status = _blockchainService.GetMiningStatus();
            return Json(status);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
