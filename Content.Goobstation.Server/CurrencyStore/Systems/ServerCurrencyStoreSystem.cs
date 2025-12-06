using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Content.Goobstation.Server.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Content.Goobstation.Shared.CurrencyStore.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Popups;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.CurrencyStore.Systems;

/// <summary>
///     Manages all simulation-side store functionality
/// </summary>
/// <remarks>
///     This is a mountain of heavily nested slop. It needs a complete refactor.
/// </remarks>
/// <seealso cref="Managers.ServerCurrencyStoreManager"/>
public sealed class ServerCurrencyStoreSystem : SharedCurrencyStoreSystem
{
    [Dependency] private readonly IServerCurrencyStoreManager _manager = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    /// <summary>
    ///     Mapping of item modification reasons to their localization string name parts.
    ///     Anything not in this dictionary will be ignored by the DisplayMessage methods.
    /// </summary>
    /// <remarks>
    ///     Activation messages are handled by <see cref="TryActivateItemInternal"/>
    /// </remarks>
    private readonly Dictionary<ItemModificationReason, string> _eventTypesToLocType = new()
    {
        { ItemModificationReason.Admin, "admin" },
        { ItemModificationReason.Purchase, "purchase" },
        { ItemModificationReason.Transfer, "transfer" },
        { ItemModificationReason.Other, "generic" }
    };

    #region Lifecycle

    public override void Initialize()
    {
        // WARNING: These can activate items, if any of this is fired from async code WE'RE FUCKED!!!!
        // Attach to manager events.
        _manager.ItemAdded += OnManagerItemAdded;
        _manager.ItemRemoved += OnManagerItemRemoved;
        _manager.VoucherAdded += OnManagerVoucherAdded;
        _manager.VoucherRemoved += OnManagerVoucherRemoved;
        _manager.PermanentItemAdded += OnManagerPermanentItemAdded;
        _manager.PermanentItemRemoved += OnManagerPermanentItemRemoved;

        // Activate items when the round state changes / adjust prices after a round
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);

        // Handle players who join the round late.
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        base.Initialize();
    }

    public override void Shutdown()
    {
        _manager.ItemAdded -= OnManagerItemAdded;
        _manager.ItemRemoved -= OnManagerItemRemoved;
        _manager.VoucherAdded -= OnManagerVoucherRemoved;
        _manager.VoucherRemoved -= OnManagerVoucherRemoved;
        _manager.PermanentItemAdded -= OnManagerPermanentItemAdded;
        _manager.PermanentItemRemoved -= OnManagerPermanentItemRemoved;

        base.Shutdown();
    }

    #endregion

    #region Public Interface

    /// <summary>
    ///     Try activating an item.
    /// </summary>
    /// <param name="item">The item to activate</param>
    /// <param name="result">Any errors to display to the user</param>
    /// <returns>If the item was activated successfully</returns>
    public bool TryActivateItem(CurrencyStoreInventoryItem item, out string result)
    {
        if (!_proto.TryIndex(item.Prototype, out var proto))
        {
            result = Loc.GetString("currencystore-error-prototype");
            return false;
        }

        return TryActivateItemInternal(item.Owner, item, proto, out result);
    }

    #endregion

    #region Event Handling

    private void OnGameRunLevelChanged(GameRunLevelChangedEvent args)
    {
        switch (args.New)
        {
            case GameRunLevel.InRound:
            {
                foreach (var player in _player.Sessions.Where(_ticker.UserHasJoinedGame))
                    ActivateImmediateItems(player, CurrencyStoreRoundState.InRound);

                break;
            }
            case GameRunLevel.PreRoundLobby:
            {
                foreach (var player in _player.Sessions.Where(s => s.Status == SessionStatus.InGame))
                    ActivateImmediateItems(player, CurrencyStoreRoundState.PreRound);

                // TODO(XWH):
                // This technically gets called at server startup as well, but it should be fine. There's way more pressing issues
                // with the price increase/decrease system anyways, the most notable of which being that when a round on any server
                // ends, the price will decrease for ALL servers. There's not really anything that can be done about that without
                // making prices server specific. It's probably going to be best to leave this decision up to admins/maints.
                foreach (var item in _proto.EnumeratePrototypes<CurrencyStoreItemPrototype>().Where(i => i.PriceDecrease != 0))
                    _manager.ModifyDynamicItemPrice(item, -item.PriceDecrease);

                break;
            }
        }
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // If we're starting a round, that will be handled in OnGameRunLevelChanged
        if (_ticker.RunLevel != GameRunLevel.InRound)
            return;

        ActivateImmediateItems(args.Player, CurrencyStoreRoundState.InRound);
    }

    private void OnManagerItemAdded(CurrencyStoreInventoryItem item, ItemModificationReason reason, NetUserId? actor)
    {
        // Get item prototype
        if (!_proto.TryIndex(item.Prototype, out var proto))
            return; // If we don't have the prototype, we can't do anything anyways. Just ignore it.

        // Send popup to player
        DisplayAddedMessage("item", proto.Name, item.Owner, reason, actor);

        // Activate immediate items
        if (item.Immediate && !TryActivateItem(item, out _))
            NotifyUser(item.Owner, Loc.GetString("currencystore-error-immediatefailure"));
    }

    private void OnManagerItemRemoved(CurrencyStoreInventoryItem item, ItemModificationReason reason, NetUserId? actor)
    {
        // Get prototype
        if (!_proto.TryIndex(item.Prototype, out var proto))
            return;

        DisplayRemovedMessage("item", proto.Name, item.Owner, reason, actor);
    }

    private void OnManagerVoucherAdded(CurrencyStoreVoucher voucher, ItemModificationReason reason, NetUserId? actor)
    {
        if (!_proto.TryIndex(voucher.Prototype, out var proto))
            return;

        DisplayAddedMessage("voucher", proto.Name, voucher.Owner, reason, actor);
    }

    private void OnManagerVoucherRemoved(CurrencyStoreVoucher voucher, ItemModificationReason reason, NetUserId? actor)
    {
        if (!_proto.TryIndex(voucher.Prototype, out var proto))
            return;

        DisplayRemovedMessage("voucher", proto.Name, voucher.Owner, reason, actor);
    }

    private void OnManagerPermanentItemAdded(NetUserId uid,
        ProtoId<CurrencyStoreItemPrototype> item,
        ItemModificationReason reason,
        NetUserId? actor)
    {
        if (!_proto.TryIndex(item, out var proto))
            return;

        DisplayAddedMessage("permanent", proto.Name, uid, reason, actor);
    }

    private void OnManagerPermanentItemRemoved(NetUserId uid,
        ProtoId<CurrencyStoreItemPrototype> item,
        ItemModificationReason reason,
        NetUserId? actor)
    {
        if (!_proto.TryIndex(item, out var proto))
            return;

        DisplayRemovedMessage("permanent", proto.Name, uid, reason, actor);
    }

    private void DisplayAddedMessage(string locType,
        string name,
        NetUserId ownerUid,
        ItemModificationReason reason,
        NetUserId? actorUid)
    {
        // Display message to user, if they are online
        if (!_player.TryGetSessionById(ownerUid, out var owner))
            return;

        // We don't need to care about who the actor is, if we can't find them, display a placeholder name.
        _player.TryGetSessionById(actorUid, out var actor);
        if (_eventTypesToLocType.TryGetValue(reason, out var reasonString) &&
            Loc.TryGetString($"currencystore-event-{locType}-add-{reasonString}", out var message,
                ("item", Loc.GetString(name)),
                ("actor", GetLocalizedPlayerName(actor)),
                ("owner", GetLocalizedPlayerName(owner))))
            NotifyUser(ownerUid, message);
    }

    private void DisplayRemovedMessage(string locType, string name, NetUserId ownerUid, ItemModificationReason reason, NetUserId? actorUid)
    {
        // Get owner if online, otherwise forget about it
        if (!_player.TryGetSessionById(ownerUid, out var owner))
            return;

        _player.TryGetSessionById(actorUid, out var actor);

        if (_eventTypesToLocType.TryGetValue(reason, out var reasonString) &&
            Loc.TryGetString($"currencystore-event-{locType}-remove-{reasonString}", out var message,
                ("item", Loc.GetString(name)),
                ("actor", GetLocalizedPlayerName(actor)),
                ("owner", GetLocalizedPlayerName(owner))))
            // When an item is transferred, it's owner is changed prior to us receiving the event.
            NotifyUser(reason == ItemModificationReason.Transfer ? actor?.UserId ?? ownerUid : ownerUid, message);
    }

    #endregion

    #region Item Activation

    /// <summary>
    ///     Try to activate a player's immediate items
    /// </summary>
    /// <param name="player">Player to process</param>
    /// <param name="allowedRoundStates">Which kinds of items to activate</param>
    private void ActivateImmediateItems(ICommonSession player, CurrencyStoreRoundState allowedRoundStates)
    {
        foreach (var item in _manager.GetInventory(player.UserId))
        {
            if (!item.Immediate ||
                !_proto.TryIndex(item.Prototype, out var proto) ||
                (proto.Redeemable & allowedRoundStates) == 0)
                continue;

            if (!TryActivateItemInternal(player.UserId, item, proto, out _))
                NotifyUser(item.Owner, Loc.GetString("currencystore-error-immediatefailure"));
        }
    }

    /// <summary>
    ///     Try activating an item.
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

        // Notify the player the item was activated
        NotifyUser(uid, Loc.GetString("currencystore-item-activated", ("item", Loc.GetString(proto.Name))));

        // Decrement item uses if the item is not infinite
        if (item.UsesLeft != -1 && item.UsesLeft != 1)
            _manager.SetItemUses(item, item.UsesLeft - 1);
        else if (item.UsesLeft == 1) // If the item has one use left, remove it instead
            _manager.RemoveItem(item, ItemModificationReason.Activation, uid);

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

        // Check that the owner is online
        if (!_player.ValidSessionId(uid))
        {
            result = Loc.GetString("currencystore-error-offline");
            return false;
        }

        // Check game state
        switch (proto.Redeemable)
        {
            // Check round has not started yet
            case CurrencyStoreRoundState.PreRound:
            {
                if (_ticker.RunLevel != GameRunLevel.PreRoundLobby)
                {
                    result = Loc.GetString("currencystore-error-roundstate");
                    return false;
                }

                break;
            }
            // Check round is running and player is inround
            case CurrencyStoreRoundState.InRound:
            {
                if (_ticker.RunLevel != GameRunLevel.InRound || !_ticker.UserHasJoinedGame(uid))
                {
                    result = Loc.GetString("currencystore-error-roundstate");
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
            if (condition.EvaluateCondition(uid, EntityManager))
                continue;

            result = Loc.GetString("currencystore-error-condition", ("reason", condition.GetLocalizedDescription()));
            return false;
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

    #region Utility

    /// <summary>
    ///     Display a message to a player if they are ingame
    /// </summary>
    /// <param name="user">Player user id</param>
    /// <param name="message">Message to display</param>
    private void NotifyUser(NetUserId user, string message)
    {
        if (!_player.TryGetSessionById(user, out var session))
            return;

        // Popup for the player
        if (session.AttachedEntity.HasValue)
        {
            _popup.PopupEntity(message, session.AttachedEntity.Value, session.AttachedEntity.Value, PopupType.Medium);
        }
        else
        {
            var wrapped = Loc.GetString("currencystore-chat-notification-message-wrap", ("message", message));
            _chat.ChatMessageToOne(ChatChannel.Server, message, wrapped, EntityUid.Invalid, false, session.Channel);
        }
    }

    /// <summary>
    ///     Get the username of a player or a localized placeholder name.
    /// </summary>
    /// <param name="session">User session</param>
    /// <returns>Localized name</returns>
    private string GetLocalizedPlayerName(ICommonSession? session)
    {
        return session != null ? session.Name : Loc.GetString("currencystore-unknown-user");
    }

    #endregion
}

