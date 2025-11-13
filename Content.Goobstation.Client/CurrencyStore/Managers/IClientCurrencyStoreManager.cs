using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CurrencyStore.Managers;

/// <remarks>
///     All client methods only work with the client's user ID. Requesting values for any other UID will throw an error.
/// </remarks>
public interface IClientCurrencyStoreManager
{
    #region Lifecycle

    void Initialize();
    void Shutdown();

    #endregion

    #region Events

    /// <summary>
    ///     Event raised when an item price update is received from the server.
    /// </summary>
    public event Action<ProtoId<CurrencyStoreItemPrototype>, CurrencyStoreItemData>? OnItemUpdate;

    #endregion

    #region Items

    /// <summary>
    ///     Check if the client player can afford an item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool CanAfford(ProtoId<CurrencyStoreItemPrototype> item);

    public bool CanPurchaseItem(ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Send a request to the server to purchase an item
    /// </summary>
    /// <param name="proto">The prototype id of the item to purchase</param>
    public void RequestPurchaseItem(ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Send a request to the server to transfer an item to another player
    /// </summary>
    /// <param name="item">The item to transfer</param>
    /// <param name="target">The user to transfer the item to</param>
    public void RequestTransferItem(CurrencyStoreInventoryItem item, string target);

    #endregion

    #region Permanent Items

    /// <summary>
    ///     Get all permanent items owned by the client player
    /// </summary>
    public HashSet<ProtoId<CurrencyStoreItemPrototype>> GetPurchasedPermanentItems();

    /// <summary>
    ///     Check if the client player owns a permanent item
    /// </summary>
    /// <param name="proto">Item prototype</param>
    public bool CheckPurchasedPermanentItem(ProtoId<CurrencyStoreItemPrototype> proto);

    #endregion

    #region Vouchers

    /// <summary>
    ///     Get vouchers owned by the client player
    /// </summary>
    public List<CurrencyStoreVoucher> GetVouchers();

    /// <summary>
    ///     Check if the client player can redeem a voucher.
    /// </summary>
    /// <param name="voucher">The voucher to redeem</param>
    /// <param name="proto">The item prototype to redeem for</param>
    public bool CanRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Send a request to the server to redeem a voucher
    /// </summary>
    /// <param name="voucher">The voucher to redeem</param>
    /// <param name="proto">The item to redeem the voucher for</param>
    public void RequestRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Send a request to the server to transfer a voucher to another player
    /// </summary>
    /// <param name="voucher">The voucher to transfer</param>
    /// <param name="target">The player to transfer the voucher to</param>
    public void RequestTransferVoucher(CurrencyStoreVoucher voucher, string target);

    #endregion

    #region Dynamic Item Data

    /// <summary>
    ///     Gets the current price of an item
    /// </summary>
    /// <param name="proto">The item prototype</param>
    /// <returns>The current price, or -1 if the prototype is invalid</returns>
    public int GetItemDynamicPrice(ProtoId<CurrencyStoreItemPrototype> proto);

    #endregion
}
