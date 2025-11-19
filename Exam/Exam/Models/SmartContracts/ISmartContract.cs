using BlockchainApp.Models;
using BlockchainApp.Services;

namespace BlockchainApp.Models.SmartContracts
{
    public interface ISmartContract
    {
        string Address { get; }
        bool ValidateTransaction(BlockChainService chain, Transaction tx, int currentBlock);
    }
}