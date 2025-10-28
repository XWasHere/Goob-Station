using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CurrencyStore.Managers;

/// <remarks>
///     All client methods only work with the client's user ID. Requesting values for any other UID will throw an error.
/// </remarks>
public interface IClientCurrencyStoreManager : ISharedCurrencyStoreManager
{
    #region Items

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

    #region Vouchers

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
}

