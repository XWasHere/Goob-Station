using System.Threading.Tasks;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Managers;

/// <summary>
///     Out-Of-Simulation currency store code.
/// </summary>
public interface ISharedCurrencyStoreManager
{
    #region Lifecycle

    public void Initialize();
    public void Shutdown();

    #endregion

    #region Items

    /// <summary>
    ///     Get the specified player's inventory.
    /// </summary>
    /// <param name="uid">The user id of the user to query. It is the caller's job to verify the uid is valid.</param>
    public List<CurrencyStoreInventoryItem> GetInventory(NetUserId uid);

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

    #endregion

    #region Permanent Items

    /// <summary>
    ///     Gets a set of purchased permanent items.
    /// </summary>
    /// <param name="uid">The user id of the user to query</param>
    public Task<HashSet<ProtoId<CurrencyStoreItemPrototype>>> GetPurchasedPermanentItems(NetUserId uid);

    /// <summary>
    ///     Checks if a user has purchased a permanent item
    /// </summary>
    /// <param name="uid">The user id of the user to query</param>
    /// <param name="proto">The prototype id of the item to check</param>
    public Task<bool> CheckPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto);

    #endregion

    #region Vouchers

    /// <summary>
    ///     Gets a list of vouchers owned by a user.
    /// </summary>
    /// <param name="uid">The user to query</param>
    public List<CurrencyStoreVoucher> GetVouchers(NetUserId uid);

    /// <summary>
    ///     Check if a voucher can be redeemed.
    /// </summary>
    /// <param name="voucher">The voucher to check</param>
    /// <param name="proto">The prototype of the item to be redeemed</param>
    public bool CanRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto);

    #endregion
}
