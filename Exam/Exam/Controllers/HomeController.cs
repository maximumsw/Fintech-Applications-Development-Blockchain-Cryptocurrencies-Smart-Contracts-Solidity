using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BlockchainApp.Models;
using BlockchainApp.Services;

namespace BlockchainApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BlockChainService _chain;

        public HomeController(ILogger<HomeController> logger, BlockChainService chain)
        {
            _logger = logger;
            _chain = chain;
        }

        public IActionResult Index()
        {
            // Адреса PenaltyStaking-контракту для UI
            ViewBag.PenaltyStakingAddress = _chain.PenaltyStakingWallet.Address;
            ViewBag.ChainLength = _chain.Chain.Count;

            // ✅ ПЕРЕДАЄМО МОДЕЛЬ У VIEW
            var model = _chain.Chain;
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Mine()
        {
            _chain.MineBlock("miner1");
            TempData["Msg"] = "Новий блок намайнено. Довжина ланцюга: " + _chain.Chain.Count;
            return RedirectToAction("Index");
        }
    }
}