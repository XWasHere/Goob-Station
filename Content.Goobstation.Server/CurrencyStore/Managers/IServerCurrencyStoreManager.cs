using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Managers;

public interface IServerCurrencyStoreManager : ISharedCurrencyStoreManager
{
    #region Lifecycle

    public void Initialize();

    public void Shutdown();

    #endregion

    #region Items

    /// <summary>
    ///     Attempt to purchase an item.
    /// </summary>
    /// <param name="uid">User id of the purchasing user</param>
    /// <param name="item">Item to purchase</param>
    /// <returns>Returns true if the item was purchased successfully</returns>
    public bool TryPurchaseItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Purchase an item and add it to a user's inventory.
    /// </summary>
    /// <remarks>
    ///     Check <see cref="CanPurchaseItem"/> first to check that the player can
    ///     afford an item and has not already purchased a permanent
    ///     item.
    /// </remarks>
    /// <param name="uid">The user ID of the purchasing user.</param>
    /// <param name="item">The item prototype.</param>
    public void PurchaseItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Attempt to activate an item
    /// </summary>
    /// <param name="item">The item to activate</param>
    /// <returns>Returns true if the item was activated successfully</returns>
    public bool TryActivateItem(CurrencyStoreInventoryItem item);

    /// <summary>
    ///     Activates an item, decreases it's use count, and potentially removes it
    ///     from the target player's inventory.
    /// </summary>
    /// <remarks>
    ///     Check <see cref="CanActivateItem"/> first to check that the item's conditions are met and
    ///     that the item is valid.
    /// </remarks>
    /// <param name="item">The item to activate</param>
    public void ActivateItem(CurrencyStoreInventoryItem item);

    /// <summary>
    ///     Removes an item from a user's inventory.
    /// </summary>
    /// <param name="item">Item to remove</param>
    public void RemoveItem(CurrencyStoreInventoryItem item);

    /// <summary>
    ///     Transfers an item from one user to another.
    /// </summary>
    /// <param name="item">The item to transfer</param>
    /// <param name="toUid">The user to add the item to</param>
    /// <returns>Returns true if the item was transferred successfully</returns>
    public bool TryTransferItem(CurrencyStoreInventoryItem item, NetUserId toUid);

    #endregion

    #region Permanent Items

    /// <summary>
    ///     Marks that a user has purchased a permanent item.
    /// </summary>
    /// <param name="uid">The user to modify</param>
    /// <param name="proto">The prototype id of the purchased item</param>
    public void SetPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Marks that a user has not purchased a permanent item.
    /// </summary>
    /// <param name="uid">The user to modify</param>
    /// <param name="proto">The prototype id of the purchased item</param>
    public void ClearPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto);

    #endregion

    #region Vouchers

    /// <summary>
    ///     Gives a user a voucher
    /// </summary>
    /// <param name="uid">The user to give the voucher to</param>
    /// <param name="proto">The prototype of the voucher to grant</param>
    public void AddVoucher(NetUserId uid, ProtoId<CurrencyStoreVoucherPrototype> proto);

    /// <summary>
    ///     Remove a voucher from a player's inventory
    /// </summary>
    /// <param name="voucher">The voucher to remove</param>
    public void RemoveVoucher(CurrencyStoreVoucher voucher);

    /// <summary>
    ///     Try to redeem a voucher.
    /// </summary>
    /// <param name="voucher">The voucher to redeem</param>
    /// <param name="item">The item to redeem for</param>
    /// <returns>If the voucher was successfully redeemed</returns>
    public bool TryRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Redeem a voucher
    /// </summary>
    /// <remarks>
    ///     Check <see cref="CanRedeemVoucher"/> prior to executing this unless you know what you're doing.
    /// </remarks>
    /// <param name="voucher">The voucher to redeem</param>
    /// <param name="proto">The prototype of the item to be redeemed</param>
    public void RedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Try to transfer a voucher to another player
    /// </summary>
    /// <param name="voucher">The voucher to transfer</param>
    /// <param name="toUid"></param>
    /// <returns></returns>
    public bool TryTransferVoucher(CurrencyStoreVoucher voucher, NetUserId toUid);

    #endregion
}

