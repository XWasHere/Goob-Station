using System.Linq;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Messages;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Content.Server.Database;
using Content.Server.GameTicking;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Managers;

/// <summary>
///     Manages all non-activation related store functionality.
/// </summary>
/// <remarks>
///     There is no async code here. All database code blocks the main thread. God help us all.
/// </remarks>
/// <seealso cref="Systems.ServerCurrencyStoreSystem"/>
public sealed class ServerCurrencyStoreManager : IServerCurrencyStoreManager
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ITaskManager _task = default!;
    [Dependency] private readonly ICommonCurrencyManager _currency = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IServerNetManager _net = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public event Action<CurrencyStoreInventoryItem, ItemModificationReason, NetUserId?>? ItemAdded;
    public event Action<CurrencyStoreInventoryItem, ItemModificationReason, NetUserId?>? ItemRemoved;

    /// <summary>
    ///     Tasks blocking the server shutting down.
    /// </summary>
    private readonly List<Task> _pendingSaveTasks = new();

    /// <summary>
    ///     Dynamic item data like their current price.
    /// </summary>
    /// <remarks>
    ///     Do not use these values for purchasing items, they may have been changed
    ///     by other servers.
    /// </remarks>
    private Dictionary<ProtoId<CurrencyStoreItemPrototype>, CurrencyStoreItemData> _cachedItemData = default!;

    /// <summary>
    ///     Dynamic item data that has been updated.
    /// </summary>
    /// <seealso cref="MarkItemDataUpdated"/>
    /// <seealso cref="SendUpdatedItemData"/>
    private HashSet<ProtoId<CurrencyStoreItemPrototype>> _updatedItemData = default!;

    /// <summary>
    ///     Cached player data
    /// </summary>
    /// <remarks>
    ///     DO NOT USE THIS FOR ACTIVATING ITEMS.
    ///     ALWAYS CHECK THE DATABASE FIRST WHEN ACTIVATING ITEMS.
    /// </remarks>
    private Dictionary<NetUserId, StorePlayerData> _cachedPlayerData = default!;

    private ISawmill _sawmill = default!;

    #region Lifecycle

    public void Initialize()
    {
        // Register NetworkManager event handlers
        _net.Connected += OnPlayerConnect;
        _net.Disconnect += OnPlayerDisconnect;

        // Register currency store packets with netmanager
        _net.RegisterNetMessage<CurrencyStoreScRefreshMessage>();
        _net.RegisterNetMessage<CurrencyStoreScRefreshStoreMessage>();
        _net.RegisterNetMessage<CurrencyStoreScResultMessage>();
        _net.RegisterNetMessage<CurrencyStoreCsRequestPurchaseMessage>(OnRequestPurchaseMessage);
        _net.RegisterNetMessage<CurrencyStoreCsRequestTransferMessage>(OnRequestTransferMessage);

        // Get logger
        _sawmill = Logger.GetSawmill("currency_store");

        // Get current item prices from the server database
        _cachedItemData = Task.Run(() => _db.GetAllItemData()).GetAwaiter().GetResult();
        _updatedItemData = [];
        _cachedPlayerData = [];
    }

    public void Shutdown()
    {
        // Remove event handlers
        _net.Connected -= OnPlayerConnect;

        // Block server shutdown until database transactions are complete
        _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
    }

    private void OnPlayerConnect(object? sender, NetChannelArgs args)
    {
        // Load player data into cache
        LoadPlayerData(args.Channel.UserId);

        // Send store data
        _sawmill.Debug($"sending data to  {args.Channel.UserId} ({args.Channel.UserName})");
        _net.ServerSendMessage(
            new CurrencyStoreScRefreshStoreMessage { UpdatedItems = _cachedItemData },
            args.Channel);

        var refresh = new CurrencyStoreScRefreshMessage
        {
            Inventory = _cachedPlayerData[args.Channel.UserId].Inventory.Values.ToList(),
            Vouchers = _cachedPlayerData[args.Channel.UserId].Vouchers.Values.ToList(),
            PermanentItems = _cachedPlayerData[args.Channel.UserId].PermanentItems.ToList(),
        };
        _net.ServerSendMessage(refresh, args.Channel);
    }

    private void OnPlayerDisconnect(object? sender, NetChannelArgs args)
    {
        // Unload cached player data
        UnloadPlayerData(args.Channel.UserId);
    }

    #endregion

    #region Public Interface

    public List<CurrencyStoreInventoryItem> GetInventory(NetUserId uid)
    {
        return GetPlayerInventoryInternal(uid).Values.ToList();
    }

    public bool CanAfford(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item)
    {
        if (!_proto.TryIndex(item, out var proto))
            return false;

        return GetBalanceAfterPurchase(uid, proto) >= 0;
    }

    public bool CanPurchaseItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item)
    {
        if (!_proto.TryIndex(item, out var proto))
            return false;

        return CanPurchaseItemInternal(uid, proto, out _) >= 0;
    }

    public CurrencyStoreInventoryItem? AddItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> protoId, bool immediate, int uses, ItemModificationReason reason, NetUserId? actor)
    {
        if (!_proto.TryIndex(protoId, out var proto))
            return null;

        var item = AddPlayerInventoryItem(uid, proto, immediate, uses);
        ItemAdded?.Invoke(item, reason, actor);
        return item;
    }

    public void RemoveItem(CurrencyStoreInventoryItem item, ItemModificationReason reason, NetUserId? actor)
    {
        RemovePlayerInventoryItem(item);
        ItemRemoved?.Invoke(item, reason, actor);
    }

    public bool TryTransferItem(CurrencyStoreInventoryItem item, NetUserId toUid, out string result)
    {
        return TryTransferItemInternal(item, toUid, out result);
    }

    public void SetItemUses(CurrencyStoreInventoryItem item, int uses)
    {
        SetPlayerInventoryItemUses(item, uses);
    }

    public async Task<HashSet<ProtoId<CurrencyStoreItemPrototype>>> GetPurchasedPermanentItems(NetUserId uid)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CheckPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public async Task<List<CurrencyStoreVoucher>> GetVouchers(NetUserId uid)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CanRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public async Task SetPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public async Task ClearPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public async Task AddVoucher(NetUserId uid, ProtoId<CurrencyStoreVoucherPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public async Task RemoveVoucher(CurrencyStoreVoucher voucher)
    {
        throw new NotImplementedException();
    }

    public void TryRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> item, out string result)
    {
        result = "";

        throw new NotImplementedException();
    }

    public void TryTransferVoucher(CurrencyStoreVoucher voucher, NetUserId toUid, out string result)
    {
        result = "";

        throw new NotImplementedException();
    }

    #endregion

    #region Netcode

    private void OnRequestPurchaseMessage(CurrencyStoreCsRequestPurchaseMessage message)
    {
        // Get item prototype
        if (!_proto.TryIndex(message.Item, out var prototype, false))
        {
            NetFailGeneric(message.MsgChannel, "Invalid item prototype.");
            return;
        }

        // Try to purchase the item
        NetSendResult(message.MsgChannel,
            TryPurchaseItemInternal(message.MsgChannel.UserId, prototype, out string result),
            result);
    }

    private void OnRequestTransferMessage(CurrencyStoreCsRequestTransferMessage message)
    {
        // Get target user
        if (!_player.TryGetUserId(message.Target, out var targetUid))
        {
            NetFailGeneric(message.MsgChannel, "Unable to find target player (are they online?)");
            return;
        }

        // Item specific logic
        switch (message.Type)
        {
            case CurrencyStoreCsRequestTransferMessage.TransferType.Item:
            {
                // Get item
                var item = GetPlayerInventoryItem(message.Id);
                if (item == null || item.Owner != message.MsgChannel.UserId)
                {
                    NetFailGeneric(message.MsgChannel, "You do not own this item.");
                    return;
                }

                // Block immediate items if the server is configured to prevent doing that
                if (item.Immediate && !_cfg.GetCVar(GoobCVars.CurrencyStoreAllowTransferImmediate))
                {
                    NetFailGeneric(message.MsgChannel, "Server does not allow transferring items pending activation.");
                    return;
                }

                // Transfer item
                NetSendResult(message.MsgChannel,
                    TryTransferItemInternal(item, targetUid, out string result),
                    result);

                return;
            }
            default:
            {
                _sawmill.Warning($"Tried to transfer item of unknown type {(byte) message.Type}");
                NetFailGeneric(message.MsgChannel, "Internal error: Malformed request");
                return;
            }
        }
    }

    /// <summary>
    ///     Send a failure response message to the client specified by <paramref name="to"/>.
    ///     Does nothing if <paramref name="to"/> is null.
    /// </summary>
    /// <param name="to">The client to send the message to</param>
    /// <param name="reason">An optional reason indicating what went wrong</param>
    private void NetFailGeneric(INetChannel? to, string reason = "")
    {
        if (to == null)
            return;

        var msg = new CurrencyStoreScResultMessage
        {
            Outcome = CurrencyStoreScResultMessage.CurrencyStoreScResultValue.Failure,
            Reason = reason,
        };

        _net.ServerSendMessage(msg, to);
    }

    /// <summary>
    ///     Send a success response message to the client specified by to.
    ///     Does nothing if to is null.
    /// </summary>
    /// <param name="to">The client to send the message to</param>
    /// <param name="reason">An optional reason</param>
    private void NetSucceedGeneric(INetChannel? to, string reason = "")
    {
        if (to == null)
            return;

        var msg = new CurrencyStoreScResultMessage
        {
            Outcome = CurrencyStoreScResultMessage.CurrencyStoreScResultValue.Success,
            Reason = reason,
        };

        _net.ServerSendMessage(msg, to);
    }

    /// <summary>
    ///     Sends a success or failure message depending on ok to the client specified by to
    ///     Does nothing if to is null
    /// </summary>
    /// <param name="to">The client to send the message to</param>
    /// <param name="ok">If the server is sending a success</param>
    /// <param name="reason">Reason to display to the user</param>
    private void NetSendResult(INetChannel? to, bool ok, string reason = "")
    {
        if (to == null)
            return;

        if (ok)
            NetSucceedGeneric(to, reason);
        else
            NetFailGeneric(to, reason);
    }

    /// <summary>
    ///     Mark that an item's dynamic price has been updated and to send it to clients next
    ///     refresh.
    /// </summary>
    /// <param name="proto">Prototype ID of the updated item</param>
    private void MarkItemDataUpdated(ProtoId<CurrencyStoreItemPrototype> proto)
    {
        _updatedItemData.Add(proto);
    }

    /// <summary>
    ///     Send updated item data to all clients
    /// </summary>
    private void SendUpdatedItemData()
    {
        var message = new CurrencyStoreScRefreshStoreMessage
        {
            UpdatedItems = [],
        };

        // Add all updated item data
        foreach (var item in _updatedItemData)
        {
            message.UpdatedItems.Add(item, _cachedItemData[item]);
        }

        // Clear updated items set
        _updatedItemData.Clear();

        // Send message to all clients
        _net.ServerSendToAll(message);
    }

    #endregion

    #region Items

    /// <summary>
    ///     Try to purchase an item.
    /// </summary>
    /// <param name="uid">User ID</param>
    /// <param name="proto">The item to purchase</param>
    /// <param name="channel">The channel to send errors over</param>
    /// <returns>If the item was successfully purchased</returns>
    private bool TryPurchaseItemInternal(NetUserId uid,
        CurrencyStoreItemPrototype proto,
        out string result)
    {
        result = "";

        // Check if we can purchase the item.
        var newBalance = CanPurchaseItemInternal(uid, proto, out result);
        if (newBalance < 0)
            return false;

        // Take the money from the player
        _currency.SetBalance(uid, newBalance);

        // Give the player the item
        var item = AddPlayerInventoryItem(uid, proto, proto.Immediate, proto.MaxUses);

        // Send item added event
        ItemAdded?.Invoke(item, ItemModificationReason.Purchase, uid);

        // Update dynamic price
        ModifyItemPrice(proto, proto.PriceIncrease, true);

        // If the item is immediate, try to activate it.
        // It's ok if we fail to activate. The item was purchased, that means we were successful
        // TODO: Fire an event that CurrencyStoreSystem can use instead.
        //if (proto.Immediate)
        //    TryActivateItemInternal(uid, item, proto, out _);

        // Item was redeemed successfully
        return true;
    }

    /// <summary>
    ///     Check to see if a user can purchase an item.
    /// </summary>
    /// <param name="uid">The user's ID</param>
    /// <param name="proto">The item prototype</param>
    /// <param name="channel">The channel to send errors to, if any</param>
    /// <returns>
    ///     If the user can purchase the item, the player's new balance after
    ///     purchasing the item. Otherwise, returns -1.
    /// </returns>
    private int CanPurchaseItemInternal(NetUserId uid,
        CurrencyStoreItemPrototype proto,
        out string result)
    {
        result = "";

        // If we are purchasing a permanent item, make sure the player doesn't already own it.
        if (proto.Permanent && GetPlayerPermanentItemOwnership(uid, proto, false))
        {
            result = "You already own this item";
            return -1;
        }

        // Check that the item isn't in a hidden category
        if (!_proto.TryIndex(proto.Category, out var category, true) || !category.InStore)
        {
            result = "Cannot purchase from hidden categories";
            return -1;
        }

        // Check that we can afford the item
        var newBalance = GetBalanceAfterPurchase(uid, proto);
        if (newBalance < 0)
        {
            result = "You cannot afford this item";
            return -1;
        }

        return newBalance;
    }

    /// <summary>
    ///     Try to transfer an item to another player
    /// </summary>
    /// <remarks>
    ///     THIS DOES NOT CHECK IF THE PLAYER OWNS THE ITEM<br/>
    ///     THIS DOES NOT CHECK IF THE PLAYER OWNS THE ITEM<br/>
    ///     THIS DOES NOT CHECK IF THE PLAYER OWNS THE ITEM
    /// </remarks>
    /// <param name="item">The item to transfer</param>
    /// <param name="to">The player to transfer the item to</param>
    /// <param name="channel">An optional channel to send errors over</param>
    /// <returns>True if the item was transferred successfully or the player already owns this item</returns>
    private bool TryTransferItemInternal(CurrencyStoreInventoryItem item, NetUserId to, out string result)
    {
        var prev = item.Owner;
        result = "";

        // Check that we don't already own the item
        if (item.Owner == to)
            return true;

        // Send item removed event
        ItemRemoved?.Invoke(item, ItemModificationReason.Transfer, prev);

        // Transfer the item
        SetPlayerItemOwner(item, to);

        // Send item added event
        ItemAdded?.Invoke(item, ItemModificationReason.Transfer, prev);

        return true;
    }

    #endregion

    #region Vouchers

    #endregion

    #region DB

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
        var record = TrackPending(Task.Run(() => _db.AddPlayerInventoryItem(uid, item, immediate, maxUses))).GetAwaiter().GetResult();

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(uid, out var data))
            data.Inventory.Add(record.Id, record);

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
        TrackPending(Task.Run(() => _db.SetPlayerInventoryItemUses(item.Id, uses))).GetAwaiter().GetResult();

        // Update uses on item
        item.UsesLeft = uses;

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var data)
            && data.Inventory.TryGetValue(item.Id, out var cachedItem))
            cachedItem.UsesLeft = uses;
    }

    /// <summary>
    ///     Set an item's owner
    /// </summary>
    /// <param name="item">The item to modify</param>
    /// <param name="owner">New owner</param>
    private void SetPlayerItemOwner(CurrencyStoreInventoryItem item, NetUserId owner)
    {
        // Update database
        TrackPending(Task.Run(() => _db.SetPlayerItemOwner(item.Id, owner))).GetAwaiter().GetResult();

        // Update cache if original item owner is connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var oldData))
            oldData.Inventory.Remove(item.Id);

        // Update cache if new item owner is connected
        if (_cachedPlayerData.TryGetValue(owner, out var newData))
            newData.Inventory.Add(item.Id, item);

        // Update item
        item.Owner = owner;
    }

    /// <summary>
    ///     Remove an item from a player's inventory
    /// </summary>
    /// <param name="item">The item to remove</param>
    private void RemovePlayerInventoryItem(CurrencyStoreInventoryItem item)
    {
        // Update database
        TrackPending(Task.Run(() => _db.DeletePlayerInventoryItem(item.Id))).GetAwaiter().GetResult();

        // Update cache if player is connected
        if (_cachedPlayerData.TryGetValue(item.Owner, out var data))
            data.Inventory.Remove(item.Id);
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
        SetItemPrice(item, Math.Clamp(GetItemPrice(item, false) + adjustment, item.Price, int.MaxValue), broadcast);
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
        TrackPending(Task.Run(() => _db.UpdateItemData(item, price))).GetAwaiter().GetResult();
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

    #endregion

    #region Internal

    /// <summary>
    ///     Track a database save task to make sure we block server shutdown on it.
    /// </summary>
    private async Task TrackPending(Task task)
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
    private async Task<TResult> TrackPending<TResult>(Task<TResult> task)
    {
        TResult result;

        _pendingSaveTasks.Add(task);

        try
        {
            result = await task;
        }
        finally
        {
            _pendingSaveTasks.Remove(task);
        }

        return result;
    }

    #endregion

    /// <summary>
    ///     Cached player data
    /// </summary>
    private sealed class StorePlayerData
    {
        /// <summary>
        ///     The player's inventory
        /// </summary>
        public required Dictionary<int, CurrencyStoreInventoryItem> Inventory;

        /// <summary>
        ///     The player's vouchers
        /// </summary>
        public required Dictionary<int, CurrencyStoreVoucher> Vouchers;

        /// <summary>
        ///     Permanent items that the player owns
        /// </summary>
        public required HashSet<ProtoId<CurrencyStoreItemPrototype>> PermanentItems;
    };
}
