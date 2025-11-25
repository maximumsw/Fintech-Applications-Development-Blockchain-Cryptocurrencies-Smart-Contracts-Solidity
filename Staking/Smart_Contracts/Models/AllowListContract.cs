using Smart_Contracts.Services;

namespace Smart_Contracts.Models;

public class AllowListContract : ISmartContract
{
    public string Address { get; }
    public HashSet<string> AllowedAddresses { get; }

    public AllowListContract(string address, IEnumerable<string> allowed)
    {
        Address = address;
        AllowedAddresses = new HashSet<string>(allowed);
    }

    public void ValidateTransaction(BlockChainService chain, Transaction tx, int currentBlockIndex)
    {
        if (tx.ToAddress == Address && !AllowedAddresses.Contains(tx.FromAddress))
        {
            throw new Exception("AllowList: sender not allowed");
        }
    }
}
