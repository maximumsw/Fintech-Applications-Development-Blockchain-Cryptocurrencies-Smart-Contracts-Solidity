using Smart_Contracts.Services;

namespace Smart_Contracts.Models;

/// <summary>
/// Інтерфейс для смарт-контрактів, які додають правила валідації до адрес
/// </summary>
public interface ISmartContract
{
    /// <summary>
    /// Адреса, до якої застосовується контракт
    /// </summary>
    string Address { get; }

    /// <summary>
    /// Валідує транзакцію відповідно до правил контракту
    /// Викидає виняток, якщо транзакція не відповідає правилам
    /// </summary>
    void ValidateTransaction(BlockChainService chain, Transaction tx, int currentBlockIndex);
}
