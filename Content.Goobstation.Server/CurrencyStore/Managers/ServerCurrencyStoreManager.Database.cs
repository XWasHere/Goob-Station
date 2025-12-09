using System.Linq;
using System.Threading.Tasks;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Managers;

public sealed partial class ServerCurrencyStoreManager
{
    /// <summary>
    ///     Load the data for a player into a cache if it is not already loaded
    /// </summary>
    /// <param name="uid">The player's user ID</param>
    private StorePlayerData LoadPlayerData(NetUserId uid)
    {
        // Return cached data
        if (_cachedPlayerData.TryGetValue(uid, out var cachedData))
            return cachedData;

        // Load player data from DB
        var data = new StorePlayerData
        {
            Inventory = GetPlayerInventoryInternal(uid, false),
            Vouchers = GetPlayerVouchersInternal(uid, false),
            PermanentItems = GetPlayerPermanentItemsInternal(uid, false),
        };
        _cachedPlayerData.Add(uid, data);
        return data;
    }

    /// <summary>
    ///     Unload cached player data if it is loaded
    /// </summary>
    /// <param name="uid">Plaer's user ID</param>
    private void UnloadPlayerData(NetUserId uid)
    {
        // Remove from cache dictionary
        _cachedPlayerData.Remove(uid);
    }

    #region Inventory

    /// <summary>
    ///     Returns a dictionary containing all items a player owns
    /// </summary>
    /// <param name="uid">The player's user ID</param>
    /// <param name="cache">Get data from the cache if available</param>
    private Dictionary<int, CurrencyStoreInventoryItem> GetPlayerInventoryInternal(NetUserId uid, bool cache = true)
    {
        // Return cached value
        if (_cachedPlayerData.TryGetValue(uid, out var data) && cache)
            return data.Inventory;

        // Get data
        var fresh = Task.Run(() => _db.GetPlayerInventory(uid)).GetAwaiter().GetResult()
            .ToDictionary(i => i.Id);

        // Maybe update cache
        if (data != null)
            data.Inventory = fresh;

        return fresh;
    }

    /// <summary>
    ///     Get an item.
    /// </summary>
    /// <param name="id">The item's database ID</param>
    private CurrencyStoreInventoryItem? GetPlayerInventoryItem(int id)
    {
        // Get data
        var fresh = Task.Run(() => _db.GetPlayerInventoryItem(id)).GetAwaiter().GetResult();

        // Maybe update cache
        if (fresh != null && _cachedPlayerData.TryGetValue(fresh.Owner, out var data))
            data.Inventory[id] = fresh;

        return fresh;
    }

    /// <summary>
    ///     Add an item to a player's inventory.
    /// </summary>
    /// <param name="uid">The player's user id</param>
    /// <param name="item">The item to add</param>
    /// <param name="immediate">Is the item immediate</param>
    /// <param name="maxUses">Is the item permanent</param>
    /// <returns>The newly added item</returns>
    private CurrencyStoreInventoryItem AddPlayerInventoryItem(NetUserId uid,
        CurrencyStoreItemPrototype item,
        bool immediate,
        int maxUses)
    {
        // Add a new item to the database
        var task = Task.Run(() => _db.AddPlayerInventoryItem(uid, item, immediate, maxUses));
        TrackPending(task);
        var record = task.GetAwaiter().GetResult();

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(uid, out var data))
            data.Inventory.Add(record.Id, record);

        // Send updated inventory to player
        SendUpdatedPlayerData(uid, true, false, false);

        // Return item
        return record;
    }

    /// <summary>
    ///     Set the number of uses left on an item.
    /// </summary>
    /// <param name="item">The item to modify</param>
    /// <param name="uses">Remaining uses</param>
    private void SetPlayerInventoryItemUses(CurrencyStoreInventoryItem item, int uses)
    {
        // Update database
        var task = Task.Run(() => _db.SetPlayerInventoryItemUses(item.Id, uses));
        TrackPending(task);
        task.GetAwaiter().GetResult();

        // Update uses on item
        item.UsesLeft = uses;

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var data)
            && data.Inventory.TryGetValue(item.Id, out var cachedItem))
            cachedItem.UsesLeft = uses;

        // Send updated inventory to player
        SendUpdatedPlayerData(item.Owner, true, false, false);
    }

    /// <summary>
    ///     Set an item's owner
    /// </summary>
    /// <param name="item">The item to modify</param>
    /// <param name="owner">New owner</param>
    private void SetPlayerItemOwner(CurrencyStoreInventoryItem item, NetUserId owner)
    {
        // Update database
        var task = Task.Run(() => _db.SetPlayerInventoryItemOwner(item.Id, owner));
        TrackPending(task);
        task.GetAwaiter().GetResult();

        // Update cache if players are connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var oldData))
            oldData.Inventory.Remove(item.Id);

        if (_cachedPlayerData.TryGetValue(owner, out var newData))
            newData.Inventory.Add(item.Id, item);

        // Send previous player updated inventory
        SendUpdatedPlayerData(item.Owner, true, false, false);

        // Update item
        item.Owner = owner;

        // Send new owner updated inventory
        SendUpdatedPlayerData(item.Owner, true, false, false);
    }

    /// <summary>
    ///     Remove an item from a player's inventory
    /// </summary>
    /// <param name="item">The item to remove</param>
    private void RemovePlayerInventoryItem(CurrencyStoreInventoryItem item)
    {
        // Update database
        var task = Task.Run(() => _db.RemovePlayerInventoryItem(item.Id));
        TrackPending(task);
        task.GetAwaiter().GetResult();

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var data))
            data.Inventory.Remove(item.Id);

        // Send updated player inventory
        SendUpdatedPlayerData(item.Owner, true, false, false);
    }

    #endregion

    #region Vouchers

    private Dictionary<int, CurrencyStoreVoucher> GetPlayerVouchersInternal(NetUserId uid, bool cache = true)
    {
        if (_cachedPlayerData.TryGetValue(uid, out var data) && cache)
            return data.Vouchers;

        // Get data
        var fresh = Task.Run(() => _db.GetPlayerOwnedVouchers(uid)).GetAwaiter().GetResult()
            .ToDictionary(v => v.Id);

        // Maybe update cache
        if (data != null)
            data.Vouchers = fresh;

        return fresh;
    }

    private CurrencyStoreVoucher? GetPlayerVoucher(int id)
    {
        // Get data
        var fresh = Task.Run(() => _db.GetStoreVoucher(id)).GetAwaiter().GetResult();

        // Maybe update cache
        if (fresh != null && _cachedPlayerData.TryGetValue(fresh.Owner, out var data))
            data.Vouchers[id] = fresh;

        return fresh;
    }

    private CurrencyStoreVoucher AddPlayerVoucher(NetUserId uid, CurrencyStoreVoucherPrototype prototype, int uses)
    {
        // Add a new voucher to the database
        var task = Task.Run(() => _db.AddStoreVoucher(uid, prototype, uses));
        TrackPending(task);
        var record = task.GetAwaiter().GetResult();

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(uid, out var data))
            data.Vouchers.Add(record.Id, record);

        // Send updated inventory to player
        SendUpdatedPlayerData(uid, false, true, false);

        // Return item
        return record;
    }

    /// <summary>
    ///     Set the number of uses left on an voucher.
    /// </summary>
    /// <param name="item">The voucher to modify</param>
    /// <param name="uses">Remaining uses</param>
    private void SetPlayerVoucherUses(CurrencyStoreVoucher item, int uses)
    {
        // Update database
        var task = Task.Run(() => _db.SetVoucherUses(item.Id, uses));
        TrackPending(task);
        task.GetAwaiter().GetResult();

        // Update uses on voucher
        item.UsesLeft = uses;

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var data)
            && data.Vouchers.TryGetValue(item.Id, out var cachedItem))
            cachedItem.UsesLeft = uses;

        // Send updated inventory to player
        SendUpdatedPlayerData(item.Owner, false, true, false);
    }

    /// <summary>
    ///     Set a voucher's owner
    /// </summary>
    /// <param name="item">The voucher to modify</param>
    /// <param name="owner">New owner</param>
    private void SetPlayerVoucherOwner(CurrencyStoreVoucher item, NetUserId owner)
    {
        // Update database
        var task = Task.Run(() => _db.SetVoucherOwner(item.Id, owner));
        TrackPending(task);
        task.GetAwaiter().GetResult();

        // Update cache if players are connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var oldData))
            oldData.Vouchers.Remove(item.Id);

        if (_cachedPlayerData.TryGetValue(owner, out var newData))
            newData.Vouchers.Add(item.Id, item);

        // Send previous player updated inventory
        SendUpdatedPlayerData(item.Owner, false, true, false);

        // Update item
        item.Owner = owner;

        // Send new owner updated inventory
        SendUpdatedPlayerData(item.Owner, false, true, false);
    }

    /// <summary>
    ///     Remove a voucher from a player's inventory
    /// </summary>
    /// <param name="item">The voucher to remove</param>
    private void RemovePlayerVoucher(CurrencyStoreVoucher item)
    {
        // Update database
        var task = Task.Run(() => _db.RemoveStoreVoucher(item.Id));
        TrackPending(task);
        task.GetAwaiter().GetResult();

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var data))
            data.Vouchers.Remove(item.Id);

        // Send updated player inventory
        SendUpdatedPlayerData(item.Owner, false, true, false);
    }

    #endregion

    #region Permanent Items

    /// <summary>
    ///     Returns a hash set containing all permanent items a player owns
    /// </summary>
    /// <param name="uid">The player's user ID</param>
    /// <param name="cache">Get data from the cache if available</param>
    private HashSet<ProtoId<CurrencyStoreItemPrototype>> GetPlayerPermanentItemsInternal(NetUserId uid, bool cache = true)
    {
        // Try cache if allowed
        if (_cachedPlayerData.TryGetValue(uid, out var data) && cache)
            return data.PermanentItems;

        // Get uncached data
        var fresh  = Task.Run(() => _db.GetPlayerOwnedPermanentItems(uid)).GetAwaiter().GetResult().ToHashSet();

        // Maybe update cache
        if (data != null)
            data.PermanentItems = fresh;

        return fresh;
    }

    /// <summary>
    ///     Check if a player owns a permanent item
    /// </summary>
    /// <param name="uid">The player's user ID</param>
    /// <param name="proto">Permanent item prototype ID</param>
    /// <param name="cache">Use the cache?</param>
    /// <returns></returns>
    private bool GetPlayerPermanentItemOwnership(NetUserId uid,
        ProtoId<CurrencyStoreItemPrototype> proto,
        bool cache = true)
    {
        // Check cache if allowed
        if (_cachedPlayerData.TryGetValue(uid, out var cachedData) && cache)
            return cachedData.PermanentItems.Contains(proto);

        // Get value from DB
        var owns = Task.Run(() => _db.GetPermanentItemOwnership(uid, proto)).GetAwaiter().GetResult();

        // Update cache
        if (cachedData != null)
        {
            if (owns)
                cachedData.PermanentItems.Add(proto);
            else
                cachedData.PermanentItems.Remove(proto);
        }

        return owns;
    }

    /// <summary>
    ///     Mark a player as owning a permanent item
    /// </summary>
    /// <param name="uid">Player user ID</param>
    /// <param name="proto">Item prototype</param>
    private bool SetPlayerPermanentItemOwnership(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        // Update database
        var task = Task.Run(() => _db.AddPermanentItemOwnership(uid, proto));
        TrackPending(task);
        var result = task.GetAwaiter().GetResult();

        // Update cache
        if (_cachedPlayerData.TryGetValue(uid, out var data))
            data.PermanentItems.Add(proto);

        // Send inventory
        SendUpdatedPlayerData(uid, false, false, true);

        return result;
    }

    /// <summary>
    ///     Mark a player as not owning a permanent item
    /// </summary>
    /// <param name="uid">Player user ID</param>
    /// <param name="proto">Item prototype</param>
    private bool ClearPlayerPermanentItemOwnership(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        // Update database
        var task = Task.Run(() => _db.RemovePermanentItemOwnership(uid, proto));
        TrackPending(task);
        var result = task.GetAwaiter().GetResult();

        // Update cache
        if (_cachedPlayerData.TryGetValue(uid, out var data))
            data.PermanentItems.Remove(proto);

        // Send inventory
        SendUpdatedPlayerData(uid, false, false, true);

        return result;
    }

    #endregion

    #region Dynamic item data

    /// <summary>
    ///     Modify an item's price while clamping it to any constraints specified in it's
    ///     prototype
    /// </summary>
    /// <param name="item">The item to modify</param>
    /// <param name="adjustment">How much to increase/decrease the price</param>
    /// <param name="broadcast">Whether to notify clients that the price has been updated</param>
    private void ModifyItemPrice(CurrencyStoreItemPrototype item, int adjustment, bool broadcast = true)
    {
        SetItemPrice(item, Math.Max(GetItemPrice(item, false) + adjustment, item.Price), broadcast);
    }

    /// <summary>
    ///     Get an item's dynamic price, or it's static price if it's dynamic price is not yet set.
    /// </summary>
    /// <param name="proto">Item prototype</param>
    /// <param name="cache">Should we use cached values?</param>
    /// <returns>The price of the item</returns>
    private int GetItemPrice(CurrencyStoreItemPrototype proto, bool cache = true)
    {
        // Check database if we are skipping the cache or don't have a value
        // We check for the cache skip second, because we may still need to update
        // the cached value.
        if (!_cachedItemData.TryGetValue(proto, out var cachedData) || !cache)
        {
            // Get the price from the database or the default price in the prototype if the database returns null
            var price = Task.Run(() => _db.GetItemData(proto)).GetAwaiter().GetResult()?.Price ?? proto.Price;

            // Add the item to the cache if we don't have it already
            if (cachedData == null)
                _cachedItemData[proto] = cachedData = new CurrencyStoreItemData { Price = proto.Price };

            // Send updated data to client if our value is out of date
            if (cachedData.Price != price)
            {
                cachedData.Price = price;
                MarkItemDataUpdated(proto);
                SendUpdatedItemData();
            }

            return price;
        }

        // Return cached price
        return cachedData.Price;
    }

    /// <summary>
    ///     Set an item's dynamic price.
    /// </summary>
    /// <param name="item">Item prototype</param>
    /// <param name="price">The new price</param>
    /// <param name="broadcast">Whether to notify clients that the price has been updated</param>
    private void SetItemPrice(CurrencyStoreItemPrototype item, int price, bool broadcast = true)
    {
        // Update cache
        if (!_cachedItemData.TryGetValue(item, out var data))
            _cachedItemData[item] = data = new CurrencyStoreItemData();
        data.Price = price;

        // Update price in database
        var task = Task.Run(() => _db.UpdateItemData(item, price));
        TrackPending(task);
        task.GetAwaiter().GetResult();
        MarkItemDataUpdated(item);

        // Notify clients
        if (broadcast)
            SendUpdatedItemData();
    }

    #endregion

    #region Server Currency

    /// <summary>
    ///     Get a player's new balance after purchasing an item
    /// </summary>
    /// <param name="uid">User ID</param>
    /// <param name="item">The item being purchased</param>
    /// <param name="cache">Use cached values</param>
    /// <returns>The new player balance, will be negative if they can not afford the item</returns>
    private int GetBalanceAfterPurchase(NetUserId uid, CurrencyStoreItemPrototype item, bool cache = true)
    {
        return _currency.GetBalance(uid) - GetItemPrice(item, cache);
    }

    #endregion

    #region Internal

    /// <summary>
    ///     Track a database save task to make sure we block server shutdown on it.
    /// </summary>
    private async void TrackPending(Task task)
    {
        _pendingSaveTasks.Add(task);

        try
        {
            await task;
        }
        finally
        {
            _pendingSaveTasks.Remove(task);
        }
    }

    /// <summary>
    ///     Track a database save task to make sure we block server shutdown on it.
    /// </summary>
    private async void TrackPending<TResult>(Task<TResult> task)
    {
        _pendingSaveTasks.Add(task);

        try
        {
            await task;
        }
        finally
        {
            _pendingSaveTasks.Remove(task);
        }
    }

    #endregion
}
