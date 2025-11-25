using Smart_Contracts.Services;

namespace Smart_Contracts.Models;

/// <summary>
/// Смарт-контракт типу TimeLock - блокує кошти на адресі до досягнення певного блоку
/// </summary>
public class TimeLockContract : ISmartContract
{
    /// <summary>
    /// Адреса, яка заблокована контрактом
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// Індекс блоку, після якого адреса буде розблокована
    /// </summary>
    public int UnlockBlockIndex { get; }

    public TimeLockContract(string address, int unlockBlockIndex)
    {
        Address = address;
        UnlockBlockIndex = unlockBlockIndex;
    }

    /// <summary>
    /// Валідує транзакцію - блокує відправлення коштів з адреси до досягнення блоку розблокування
    /// </summary>
    public void ValidateTransaction(BlockChainService chain, Transaction tx, int currentBlockIndex)
    {
        // Якщо транзакція йде З заблокованої адреси і поточний блок менший за блок розблокування
        if (tx.FromAddress == Address && currentBlockIndex < UnlockBlockIndex)
        {
            throw new Exception($"TimeLock: funds locked until block {UnlockBlockIndex}");
        }
        // Зауважте: транзакції НА цю адресу дозволені завжди
    }
}
