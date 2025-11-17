using BlockchainMvc_V2.Models;
using BlockchainMvc_V2.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlockchainMvc_V2.Controllers;

public class BlockchainController : Controller
{
    private static readonly Blockchain _blockchain = new();

    public IActionResult Index()
    {
        return View(_blockchain);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(string data, string privateKey)
    {
        if (string.IsNullOrWhiteSpace(privateKey))
        {
            ModelState.AddModelError("", "Private Key is required.");
            return View();
        }

        _blockchain.AddBlock(data, privateKey);
        return RedirectToAction("Index");
    }

    public IActionResult Edit(int id)
    {
        var block = _blockchain.Chain.FirstOrDefault(b => b.Index == id);
        if (block == null) return NotFound();

        return View(block);
    }

    [HttpPost]
    public IActionResult Edit(int index, string data)
    {
        _blockchain.UpdateBlock(index, data);
        return RedirectToAction("Index");
    }
}