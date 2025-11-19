using System.Linq;
using Microsoft.AspNetCore.Mvc;
using BlockchainApp.Services;
using BlockchainApp.Models;

namespace BlockchainApp.Controllers
{
    public class StakingController : Controller
    {
        private readonly BlockChainService _chain;

        public StakingController(BlockChainService chain)
        {
            _chain = chain;
        }

        // -------------------------------
        // ФОРМА: ПОКЛАСТИ У STAKING
        // -------------------------------
        [HttpGet]
        public IActionResult Stake()
        {
            ViewBag.ContractAddress = _chain.PenaltyStakingWallet.Address;
            return View();
        }

        [HttpPost]
        public IActionResult Stake(string fromAddress, decimal amount, decimal fee, string privateKey)
        {
            var tx = new Transaction
            {
                FromAddress = fromAddress,
                ToAddress = _chain.PenaltyStakingWallet.Address,
                Amount = amount,
                Fee = fee
            };

            // Підпис користувачем
            WalletService.SignTransaction(tx, privateKey);

            bool added = _chain.CreateTransaction(tx);

            TempData["Msg"] = added ? "Стейк успішно додано в Mempool" : "ПОМИЛКА: транзакція відхилена контрактом";

            return RedirectToAction("Stake");
        }

        // -------------------------------
        // ФОРМА: ВИВЕСТИ STAKE
        // -------------------------------
        [HttpGet]
        public IActionResult Unstake()
        {
            ViewBag.ContractAddress = _chain.PenaltyStakingWallet.Address;
            return View();
        }

        [HttpPost]
        public IActionResult Unstake(string userAddress)
        {
            // Дізнаємося скільки зараз можна вивести
            var state = _chain.PenaltyStakingContract
                .GetState(_chain.CurrentHeight)
                .FirstOrDefault(x => x.User == userAddress);

            if (state.User == null)
            {
                TempData["Msg"] = "У цього користувача немає стейку";
                return RedirectToAction("Unstake");
            }

            decimal amountToWithdraw = state.WithdrawNow;

            var tx = new Transaction
            {
                FromAddress = _chain.PenaltyStakingWallet.Address,
                ToAddress = userAddress,
                Amount = amountToWithdraw,
                Fee = 0
            };

            // Підпис контрактним приватним ключем
            WalletService.SignTransaction(tx, _chain.PenaltyStakingWallet.PrivateKey);

            bool added = _chain.CreateTransaction(tx);

            TempData["Msg"] = added ? "Запит на вивід надіслано" : "ПОМИЛКА: контракт відхилив вивід";

            return RedirectToAction("Unstake");
        }

        // -------------------------------
        // СТОРІНКА СТАНУ КОНТРАКТУ (Завдання 3)
        // -------------------------------
        public IActionResult ContractState()
        {
            var data = _chain.PenaltyStakingContract.GetState(_chain.CurrentHeight).ToList();
            return View(data);
        }
    }
}
