using System.Threading.Tasks;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Managers;

public interface IServerCurrencyStoreManager : ISharedCurrencyStoreManager
{
    #region Events

    /// <summary>
    ///     Event fired when an item is added to a player's inventory.
    /// </summary>
    public event Action<CurrencyStoreInventoryItem, ItemModificationReason, NetUserId?>? ItemAdded;

    /// <summary>
    ///     Event fired when an item is removed from a player's inventory
    /// </summary>
    public event Action<CurrencyStoreInventoryItem, ItemModificationReason, NetUserId?>? ItemRemoved;

    #endregion

    #region Items

    /// <summary>
    ///     Add an item to a player's inventory
    /// </summary>
    /// <param name="uid">The player's user id</param>
    /// <param name="item">The item prototype</param>
    /// <param name="immediate">Should the item be activated immediately</param>
    /// <param name="uses">Number of uses, -1 if infinite</param>
    /// <param name="reason">What caused the item to be added</param>
    /// <param name="actor">Which user caused the item to be added</param>
    /// <returns>The new item, or null if nothing was added</returns>
    public CurrencyStoreInventoryItem? AddItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item, bool immediate, int uses, ItemModificationReason reason = ItemModificationReason.Other, NetUserId? actor = null);

    /// <summary>
    ///     Removes an item from a user's inventory.
    /// </summary>
    /// <param name="item">Item to remove</param>
    /// <param name="reason">What caused the item to be removed</param>
    /// <param name="actor">Which user caused the item to be added</param>
    public void RemoveItem(CurrencyStoreInventoryItem item, ItemModificationReason reason = ItemModificationReason.Other, NetUserId? actor = null);

    /// <summary>
    ///     Transfers an item from one user to another.
    /// </summary>
    /// <param name="item">The item to transfer</param>
    /// <param name="toUid">The user to add the item to</param>
    /// <param name="result">A string to be displayed to the user if the item can not be transferred</param>
    /// <returns>Returns true if the item was transferred successfully</returns>
    public bool TryTransferItem(CurrencyStoreInventoryItem item, NetUserId toUid, out string result);

    /// <summary>
    ///     Set the number of uses on an item
    /// </summary>
    /// <param name="item">The item to modify</param>
    /// <param name="uses">The new number of uses</param>
    public void SetItemUses(CurrencyStoreInventoryItem item, int uses);

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
    /// <param name="result">A string to be displayed to the user if the voucher can not be redeemed</param>
    /// <returns>If the voucher was successfully redeemed</returns>
    public void TryRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> item, out string result);

    /// <summary>
    ///     Try to transfer a voucher to another player
    /// </summary>
    /// <param name="voucher">The voucher to transfer</param>
    /// <param name="toUid"></param>
    /// <param name="result">A string to be displayed to the user if the voucher fails to activate</param>
    /// <returns></returns>
    public void TryTransferVoucher(CurrencyStoreVoucher voucher, NetUserId toUid, out string result);

    #endregion
}

/// <summary>
///     What caused an item to be added or removed
/// </summary>
public enum ItemModificationReason
{
    Admin, Purchase, Transfer, Activation, Other
}

