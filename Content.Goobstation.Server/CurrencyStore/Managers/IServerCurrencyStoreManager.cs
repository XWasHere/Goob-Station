using System.Threading.Tasks;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Managers;

/// <summary>
///     Out-Of-Simulation currency store code.
/// </summary>
/// <remarks>
///     All methods use cached values unless stated otherwise.
/// </remarks>
public interface IServerCurrencyStoreManager
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

    /// <summary>
    ///     Event fired when a permanent item is added to a player's inventory.
    /// </summary>
    public event Action<NetUserId, ProtoId<CurrencyStoreItemPrototype>, ItemModificationReason, NetUserId?>? PermanentItemAdded;

    /// <summary>
    ///     Event fired when a permanent item is removed from a player's inventory.
    /// </summary>
    public event Action<NetUserId, ProtoId<CurrencyStoreItemPrototype>, ItemModificationReason, NetUserId?>? PermanentItemRemoved;

    /// <summary>
    ///     Event fired when a voucher is added to a player's inventory
    /// </summary>
    public event Action<CurrencyStoreVoucher, ItemModificationReason, NetUserId?>? VoucherAdded;

    /// <summary>
    ///     Event fired when a voucher is removed from a player's inventory
    /// </summary>
    public event Action<CurrencyStoreVoucher, ItemModificationReason, NetUserId?>? VoucherRemoved;

    #endregion

    #region Lifecycle

    public void Initialize();
    public void Shutdown();

    #endregion

    #region Items

    /// <summary>
    ///     Get the specified player's inventory.
    /// </summary>
    /// <param name="uid">The user id of the user to query. It is the caller's job to verify the uid is valid.</param>
    public List<CurrencyStoreInventoryItem> GetInventory(NetUserId uid, bool cache = true);

    /// <summary>
    ///     Get an item by its database ID. Does not use cached values.
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>The item, if it exists</returns>
    public CurrencyStoreInventoryItem? GetItem(int id);

    /// <summary>
    ///     Add an item to a player's inventory
    /// </summary>
    /// <remarks>If you change this, don't forget to update CurrencyStoreCommand::GiveItem</remarks>
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
    ///     Gets a set of purchased permanent items.
    /// </summary>
    /// <param name="uid">The user id of the user to query</param>
    public HashSet<ProtoId<CurrencyStoreItemPrototype>> GetPurchasedPermanentItems(NetUserId uid, bool cache = true);

    /// <summary>
    ///     Checks if a user has purchased a permanent item.
    /// </summary>
    /// <remarks>
    ///     Checking permanent item ownerships uses cached values by default because unlike normal items,
    ///     permanent items can be related to things like loadout items and traits. Without caching, this
    ///     could lead to running over 100 database queries each time the round starts. Additionally,
    ///     permanent items cannot be removed from a player's inventory without admin intervention, so it
    ///     is safe to assume that it's value will not change once it's set.
    /// </remarks>
    /// <param name="uid">The user id of the user to query</param>
    /// <param name="proto">The prototype id of the item to check</param>
    public bool CheckPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto, bool cache = true);

    /// <summary>
    ///     Marks that a user has purchased a permanent item.
    /// </summary>
    /// <param name="uid">The user to modify</param>
    /// <param name="proto">The prototype id of the purchased item</param>
    public void SetPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto, ItemModificationReason reason, NetUserId? actor);

    /// <summary>
    ///     Marks that a user has not purchased a permanent item.
    /// </summary>
    /// <param name="uid">The user to modify</param>
    /// <param name="proto">The prototype id of the purchased item</param>
    public void ClearPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto, ItemModificationReason reason, NetUserId? actor);

    #endregion

    #region Vouchers

    /// <summary>
    ///     Gets a list of vouchers owned by a user.
    /// </summary>
    /// <param name="uid">The user to query</param>
    public List<CurrencyStoreVoucher> GetVouchers(NetUserId uid, bool cache = true);

    /// <summary>
    ///     Gets a voucher by its database ID. Does not use cached values
    /// </summary>
    /// <param name="id">Voucher ID</param>
    /// <returns>The voucher, if it exists</returns>
    public CurrencyStoreVoucher? GetVoucher(int id);

    /// <summary>
    ///     Gives a user a voucher
    /// </summary>
    /// <param name="uid">The user to give the voucher to</param>
    /// <param name="proto">The prototype of the voucher to grant</param>
    /// <param name="reason">What caused this voucher to be removed</param>
    /// <param name="actor">Who caused this voucher to be removed</param>
    public CurrencyStoreVoucher? AddVoucher(NetUserId uid, ProtoId<CurrencyStoreVoucherPrototype> proto, int uses, ItemModificationReason reason = ItemModificationReason.Other, NetUserId? actor = null);

    /// <summary>
    ///     Remove a voucher from a player's inventory
    /// </summary>
    /// <param name="voucher">The voucher to remove</param>
    /// <param name="reason">What caused this voucher to be removed</param>
    /// <param name="actor">Who caused this voucher to be removed</param>
    public void RemoveVoucher(CurrencyStoreVoucher voucher, ItemModificationReason reason = ItemModificationReason.Other, NetUserId? actor = null);

    /// <summary>
    ///     Check if a voucher can be redeemed.
    /// </summary>
    /// <param name="voucher">The voucher to check</param>
    /// <param name="proto">The prototype of the item to be redeemed</param>
    public bool CanRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto);

    /// <summary>
    ///     Try to redeem a voucher.
    /// </summary>
    /// <param name="voucher">The voucher to redeem</param>
    /// <param name="item">The item to redeem for</param>
    /// <param name="result">A string to be displayed to the user if the voucher can not be redeemed</param>
    /// <returns>If the voucher was successfully redeemed</returns>
    public bool TryRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> item, out string result);

    /// <summary>
    ///     Try to transfer a voucher to another player
    /// </summary>
    /// <param name="voucher">The voucher to transfer</param>
    /// <param name="toUid"></param>
    /// <param name="result">A string to be displayed to the user if the voucher fails to activate</param>
    /// <returns>If the voucher was transferred successfully</returns>
    public bool TryTransferVoucher(CurrencyStoreVoucher voucher, NetUserId toUid, out string result);

    #endregion
}

/// <summary>
///     What caused an item to be added or removed
/// </summary>
public enum ItemModificationReason
{
    Admin, Purchase, Transfer, Activation, Other
}

