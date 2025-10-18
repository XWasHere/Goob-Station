using System.Threading;
using Content.Goobstation.Server.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Content.Goobstation.Shared.CurrencyStore.Systems;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Systems;

/// <summary>
///     Manages all simulation-side store functionality
/// </summary>
/// <seealso cref="Managers.ServerCurrencyStoreManager"/>
public sealed class ServerCurrencyStoreSystem : SharedCurrencyStoreSystem
{
    [Dependency] private readonly IServerCurrencyStoreManager _manager = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    /// <summary>
    ///     Events received from ServerCurrencyStoreManager, safe to interact with from ECS.
    /// </summary>
    private Queue<(bool, object, ItemModificationReason, NetUserId?)> _itemChangesMailbox = [];
    private readonly object _itemChangesMailboxLock = new(); // NOTE(XWH): I highly doubt this needs a lock.

    #region Lifecycle

    public override void Initialize()
    {
        // Attach to manager events
        _manager.ItemAdded += OnManagerItemAdded;
        _manager.ItemRemoved += OnManagerItemRemoved;

        base.Initialize();
    }

    public override void Shutdown()
    {
        // Detach from manager events
        _manager.ItemAdded -= OnManagerItemAdded;
        _manager.ItemRemoved -= OnManagerItemRemoved;

        base.Shutdown();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Process events from StoreManager
        while (true)
        {
            // This is horrible.
            (bool, object, ItemModificationReason, NetUserId?) data;
            lock (_itemChangesMailboxLock)
                if (!_itemChangesMailbox.TryDequeue(out data))
                    break;
            var (added, item, reason, actor) = data;

            // Run ECS-safe event handlers
            switch (item)
            {
                case CurrencyStoreInventoryItem inventoryItem:
                {
                    if (added)
                        ProcessItemAdded(inventoryItem, reason, actor);
                    else
                        ProcessItemRemoved(inventoryItem, reason, actor);
                    break;
                }
            }
        }
    }

    #endregion

    #region StoreManager Event Handling

    private void OnManagerItemAdded(CurrencyStoreInventoryItem item, ItemModificationReason reason, NetUserId? actor)
    {
        lock (_itemChangesMailboxLock)
            _itemChangesMailbox.Enqueue((true, item, reason, actor));
    }

    private void OnManagerItemRemoved(CurrencyStoreInventoryItem item, ItemModificationReason reason, NetUserId? actor)
    {
        lock (_itemChangesMailboxLock)
            _itemChangesMailbox.Enqueue((false, item, reason, actor));
    }

    #endregion

    #region Event Handling

    private void ProcessItemAdded(CurrencyStoreInventoryItem item, ItemModificationReason reason, NetUserId? actorUid)
    {
        // Get item prototype
        if (!_proto.TryIndex(item.Prototype, out var proto))
            return; // If we don't have the prototype, we can't do anything anyways. Just ignore it.

        // Display message to user, if they are online
        if (!_player.TryGetSessionById(item.Owner, out var owner))
            return;

        var localizedItem = Loc.GetString(proto.Name);
        var actorName = _player.TryGetSessionById(actorUid, out var actor)
            ? actor.Name
            : "an unknown user."; // TODO(XWH): Localization

        // TODO(XWH): Localization
        var message = reason switch
        {
            ItemModificationReason.Admin or ItemModificationReason.Transfer => $"You got a {localizedItem} from {actorName}",
            ItemModificationReason.Purchase => $"Purchased {localizedItem}",
            ItemModificationReason.Activation or ItemModificationReason.Other => $"You got a {localizedItem}",
        };

        _popup.PopupClient(message, owner.AttachedEntity!.Value, owner.AttachedEntity!.Value, PopupType.Medium);
    }

    // And god taketh away
    private void ProcessItemRemoved(CurrencyStoreInventoryItem item, ItemModificationReason reason, NetUserId? actor) {}

    #endregion

    #region Item Activation

    /// <summary>
    ///     Try activating an item
    /// </summary>
    /// <param name="uid">The user ID</param>
    /// <param name="item">The item</param>
    /// <param name="proto">The prototype of the item</param>
    /// <param name="result">A localized string describing why an item could not be used</param>
    /// <returns>True if the item was successfully activated</returns>
    private bool TryActivateItemInternal(NetUserId uid,
        CurrencyStoreInventoryItem item,
        CurrencyStoreItemPrototype proto,
        out string result)
    {
        result = "";

        // Can we activate the item?
        if (!CanActivateItemInternal(uid, item, proto, out result))
            return false;

        // Run the item's effects
        ExecuteItemEffects(uid, item, proto, out result);

        // Decrement item uses if the item is not infinite
        if (item.UsesLeft != -1 && item.UsesLeft != 1)
        {
            _manager.SetItemUses(item, item.UsesLeft - 1);
        }
        else if (item.UsesLeft == 1) // If the item has one use left, remove it instead
        {
            _manager.RemoveItem(item, ItemModificationReason.Activation, uid);
        }

        return true;
    }

    /// <summary>
    ///     Check if an item can be used. This includes executing its conditions.
    /// </summary>
    /// <param name="uid">The user ID</param>
    /// <param name="item">The item</param>
    /// <param name="proto">The prototype of the item</param>
    /// <param name="result">A message that can be displayed to users describing why the item could not be used.</param>
    /// <returns>True if the item can be used</returns>
    private bool CanActivateItemInternal(NetUserId uid,
        CurrencyStoreInventoryItem item,
        CurrencyStoreItemPrototype proto,
        out string result)
    {
        result = "";

        // Check game state
        switch (proto.Redeemable)
        {
            // Check round has not started yet
            case CurrencyStoreRoundState.PreRound:
            {
                if (_ticker.RunLevel != GameRunLevel.PreRoundLobby)
                {
                    result = "You can not use this item right now.";
                    return false;
                }

                break;
            }
            // Check round is running and player is inround
            case CurrencyStoreRoundState.InRound:
            {
                if (_ticker.RunLevel != GameRunLevel.InRound || !_ticker.UserHasJoinedGame(uid))
                {
                    result = "You can not use this item right now.";
                    return false;
                }

                break;
            }
            // Always is always ok
            case CurrencyStoreRoundState.Always:
            {
                break;
            }
        }

        // Run conditions
        if (!ExecuteItemConditions(uid, item, proto, out result))
            return false;

        // Item can be activated
        return true;
    }

    /// <summary>
    ///     Execute the conditions on an item to determine if it can be used
    /// </summary>
    /// <param name="uid">The user ID of the person using the item</param>
    /// <param name="item">The item</param>
    /// <param name="proto">The item prototype</param>
    /// <param name="result">A localized string describing why a condition failed, if it did</param>
    /// <returns>If the conditions executed successfully</returns>
    private bool ExecuteItemConditions(NetUserId uid,
        CurrencyStoreInventoryItem item,
        CurrencyStoreItemPrototype proto,
        out string result)
    {
        result = "";

        foreach (var condition in proto.Conditions)
        {
            if (!condition.EvaluateCondition(uid, EntityManager))
            {
                result = $"You can not activate this item right now: {condition.GetLocalizedDescription()}";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Execute the effects of an item
    /// </summary>
    /// <param name="uid">The user ID of the person using the item</param>
    /// <param name="item">The item</param>
    /// <param name="proto">The item prototype</param>
    /// <param name="result">A localized string to be displayed to the user if an effect fails to execute</param>
    private void ExecuteItemEffects(NetUserId uid,
        CurrencyStoreInventoryItem item,
        CurrencyStoreItemPrototype proto,
        out string result)
    {
        result = "";

        foreach (var effect in proto.Effects)
        {
            effect.ExecuteEffect(uid, EntityManager);
        }
    }

    #endregion
}

