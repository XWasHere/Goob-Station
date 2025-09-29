using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Managers;

/// <summary>
///     Out-Of-Simulation currency store code.
/// </summary>
public interface ISharedCurrencyStoreManager
{
    #region Items

    /// <summary>
    ///     Get the specified player's inventory.
    /// </summary>
    /// <param name="uid">The user id of the user to query</param>
    public List<CurrencyStoreInventoryItem> GetInventory(NetUserId uid);

    /// <summary>
    ///     Attempt to purchase an item.
    /// </summary>
    /// <param name="uid">User id of the purchasing user</param>
    /// <param name="item">Item to purchase</param>
    /// <returns>Returns true if the item was purchased successfully</returns>
    public bool TryPurchaseItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Checks if the specified user can afford the specified item.
    /// </summary>
    /// <param name="uid">The user id of the user to check</param>
    /// <param name="item">The item to check</param>
    public bool CanAfford(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Checks if a user is able to purchase an item.
    /// </summary>
    /// <param name="uid">The user ID of the purchasing user.</param>
    /// <param name="item">The item prototype.</param>
    public bool CanPurchaseItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Transfers an item from one user to another.
    /// </summary>
    /// <param name="item">The item to transfer</param>
    /// <param name="toUid">The user to add the item to</param>
    /// <returns>Returns true if the item was transferred successfully</returns>
    public bool TryTransferItem(CurrencyStoreInventoryItem item, NetUserId toUid);

    /// <summary>
    ///     Attempt to activate an item
    /// </summary>
    /// <param name="item">The item to activate</param>
    /// <returns>Returns true if the item was activated successfully</returns>
    public bool TryActivateItem(CurrencyStoreInventoryItem item);

    /// <summary>
    ///     Check if an item can be activated.
    /// </summary>
    /// <param name="item">The item to activate</param>
    public bool CanActivateItem(CurrencyStoreInventoryItem item);

    #endregion

    #region Permanent Items

    /// <summary>
    ///     Gets a set of purchased permanent items.
    /// </summary>
    /// <param name="uid">The user id of the user to query</param>
    public HashSet<ProtoId<CurrencyStoreItemPrototype>> GetPurchasedPermanentItems(NetUserId uid);

    /// <summary>
    ///     Checks if a user has purchased a permanent item
    /// </summary>
    /// <param name="uid">The user id of the user to query</param>
    /// <param name="proto">The prototype id of the item to check</param>
    public bool CheckPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto);

    #endregion

    #region Vouchers

    /// <summary>
    ///     Gets a list of vouchers owned by a user.
    /// </summary>
    /// <param name="uid">The user to query</param>
    public List<CurrencyStoreVoucher> GetVouchers(NetUserId uid);

    /// <summary>
    ///     Try to redeem a voucher.
    /// </summary>
    /// <param name="voucher">The voucher to redeem</param>
    /// <param name="item">The item to redeem for</param>
    /// <returns>If the voucher was successfully redeemed</returns>
    public bool TryRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Check if a voucher can be redeemed.
    /// </summary>
    /// <param name="voucher">The voucher to check</param>
    /// <param name="proto">The prototype of the item to be redeemed</param>
    public bool CanRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Try to transfer a voucher to another player
    /// </summary>
    /// <param name="voucher">The voucher to transfer</param>
    /// <param name="toUid"></param>
    /// <returns></returns>
    public bool TryTransferVoucher(CurrencyStoreVoucher voucher, NetUserId toUid);

    #endregion
}
