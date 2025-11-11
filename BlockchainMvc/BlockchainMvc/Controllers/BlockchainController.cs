using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using BlockchainMvc.Models;

namespace BlockchainMvc.Controllers
{
    public class BlockchainController : Controller
    {
        private static Blockchain _blockchain = new Blockchain();
        private readonly string _filePath;

        public BlockchainController(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "blockchain.json");
            _blockchain.LoadFromFile(_filePath);
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.IsValid = _blockchain.IsValid();
            var model = _blockchain.Chain.OrderBy(b => b.Index).ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                ModelState.AddModelError(string.Empty, "Дані не можуть бути порожні.");
                ViewBag.IsValid = _blockchain.IsValid();
                var chain = _blockchain.Chain.OrderBy(b => b.Index).ToList();
                return View("Index", chain);
            }

            _blockchain.AddBlock(data);
            _blockchain.SaveToFile(_filePath);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string hash, int? index)
        {
            Block? block = null;

            if (index.HasValue)
                block = _blockchain.FindByIndex(index.Value);
            else if (!string.IsNullOrWhiteSpace(hash))
                block = _blockchain.FindByHash(hash);

            return View("SearchResult", block);
        }
    }
}