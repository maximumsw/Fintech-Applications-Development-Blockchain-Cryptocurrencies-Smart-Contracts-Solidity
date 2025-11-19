using System;
using System.Security.Cryptography;
using System.Text;
using BlockchainApp.Models;

namespace BlockchainApp.Services
{
    public static class WalletService
    {
        public static Wallet CreateWallet()
        {
            var privateKey = Guid.NewGuid().ToString("N");
            var address = "W" + privateKey.Substring(0, 8);

            return new Wallet
            {
                Address = address,
                PrivateKey = privateKey
            };
        }

        public static void SignTransaction(Transaction tx, string privateKey)
        {
            var raw = $"{privateKey}|{tx.FromAddress}|{tx.ToAddress}|{tx.Amount}|{tx.Fee}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            tx.Signature = Convert.ToHexString(hash);
        }
    }
}