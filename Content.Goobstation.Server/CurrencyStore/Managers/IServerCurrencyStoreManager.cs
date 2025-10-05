using System.Threading.Tasks;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Managers;

public interface IServerCurrencyStoreManager : ISharedCurrencyStoreManager
{
    #region Items

    /// <summary>
    ///     Attempt to purchase an item.
    /// </summary>
    /// <param name="uid">User id of the purchasing user</param>
    /// <param name="item">Item to purchase</param>
    /// <returns>Returns true if the item was purchased successfully</returns>
    public Task<bool> TryPurchaseItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item);

    /// <summary>
    ///     Attempt to activate an item, decreases it's use cound, and potentially removes
    ///     it from the player's inventory
    /// </summary>
    /// <param name="item">The item to activate</param>
    /// <returns>Returns true if the item was activated successfully</returns>
    public Task<bool> TryActivateItem(CurrencyStoreInventoryItem item);

    /// <summary>
    ///     Removes an item from a user's inventory.
    /// </summary>
    /// <param name="item">Item to remove</param>
    public Task RemoveItem(CurrencyStoreInventoryItem item);

    /// <summary>
    ///     Transfers an item from one user to another.
    /// </summary>
    /// <param name="item">The item to transfer</param>
    /// <param name="toUid">The user to add the item to</param>
    /// <returns>Returns true if the item was transferred successfully</returns>
    public Task<bool> TryTransferItem(CurrencyStoreInventoryItem item, NetUserId toUid);

    #endregion

    #region Permanent Items

    /// <summary>
    ///     Marks that a user has purchased a permanent item.
    /// </summary>
    /// <param name="uid">The user to modify</param>
    /// <param name="proto">The prototype id of the purchased item</param>
    public Task SetPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Marks that a user has not purchased a permanent item.
    /// </summary>
    /// <param name="uid">The user to modify</param>
    /// <param name="proto">The prototype id of the purchased item</param>
    public Task ClearPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto);

    #endregion

    #region Vouchers

    /// <summary>
    ///     Gives a user a voucher
    /// </summary>
    /// <param name="uid">The user to give the voucher to</param>
    /// <param name="proto">The prototype of the voucher to grant</param>
    public Task AddVoucher(NetUserId uid, ProtoId<CurrencyStoreVoucherPrototype> proto);

    /// <summary>
    ///     Remove a voucher from a player's inventory
    /// </summary>
    /// <param name="voucher">The voucher to remove</param>
    public Task RemoveVoucher(CurrencyStoreVoucher voucher);

    /// <summary>
    ///     Try to redeem a voucher.
    /// </summary>
    /// <param name="voucher">The voucher to redeem</param>
    /// <param name="item">The item to redeem for</param>
    /// <returns>If the voucher was successfully redeemed</returns>
    public Task TryRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> item);

   /// <summary>
    ///     Try to transfer a voucher to another player
    /// </summary>
    /// <param name="voucher">The voucher to transfer</param>
    /// <param name="toUid"></param>
    /// <returns></returns>
    public Task TryTransferVoucher(CurrencyStoreVoucher voucher, NetUserId toUid);

    #endregion
}

