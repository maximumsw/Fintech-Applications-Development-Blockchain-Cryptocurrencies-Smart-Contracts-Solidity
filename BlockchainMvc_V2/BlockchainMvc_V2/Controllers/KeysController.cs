using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace BlockchainMvc_V2.Controllers;

public class KeysController : Controller
{
    public IActionResult Index()
    {
        using var rsa = RSA.Create(2048);

        var privatePem = ExportPrivateKeyPem(rsa);
        var publicPem = ExportPublicKeyPem(rsa);

        ViewBag.PrivateKey = privatePem;
        ViewBag.PublicKey = publicPem;

        return View();
    }

    private string ExportPrivateKeyPem(RSA rsa)
    {
        var priv = rsa.ExportPkcs8PrivateKey();
        var base64 = Convert.ToBase64String(priv, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN PRIVATE KEY-----\n{base64}\n-----END PRIVATE KEY-----";
    }

    private string ExportPublicKeyPem(RSA rsa)
    {
        var pub = rsa.ExportSubjectPublicKeyInfo();
        var base64 = Convert.ToBase64String(pub, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN PUBLIC KEY-----\n{base64}\n-----END PUBLIC KEY-----";
    }
}