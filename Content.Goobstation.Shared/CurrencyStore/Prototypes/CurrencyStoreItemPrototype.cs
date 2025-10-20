using Content.Goobstation.Shared.CurrencyStore.Managers;
using Content.Goobstation.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Prototypes;

/// <summary>
///     An item in the game store.
/// </summary>
[Prototype]
public sealed partial class CurrencyStoreItemPrototype : CurrencyStoreBaseItemPrototype
{
    /// <summary>
    ///     The store category this item belongs to
    /// </summary>
    [DataField(required: true)]
    public ProtoId<CurrencyStoreCategoryPrototype> Category { get; private set; }

    /// <summary>
    ///     When the item can be redeemed.
    /// </summary>
    [DataField]
    public CurrencyStoreRoundState Redeemable { get; private set;  } = CurrencyStoreRoundState.Always;

    /// <summary>
    ///     The amount of currency it costs to buy this item.
    /// </summary>
    [DataField(required: true)]
    public int Price { get; private set; }

    /// <summary>
    ///     Whether an item is permanent or not.
    /// </summary>
    /// <remarks>
    ///     Permanent items, while defined with the same prototype due to their similarity
    ///     to regular items, function completely differently internally. Permanent items,
    ///     rather than being activatable, are stored as a record with their prototype name,
    ///     other systems can query the <see cref="ISharedCurrencyStoreManager"/> to check
    ///     if a user owns it, and can then act on that information to, for example, allow the
    ///     player to use a unique trait or equip an unique item in their loadout menu.
    /// </remarks>
    [DataField]
    public bool Permanent { get; private set; } = false;

    /// <summary>
    ///     The amount that the cost increases each time this item is purchased.
    /// </summary>
    [DataField]
    public int PriceIncrease { get; private set; } = 0;

    /// <summary>
    ///     The amount that the cost decreases each round until reaching the base price.
    /// </summary>
    [DataField]
    public int PriceDecrease { get; private set; } = 0;

    /// <summary>
    ///     If set to true, the item will be automatically used as soon as the conditions to activate it are met.
    /// </summary>
    [DataField]
    public bool Immediate { get; private set; } = false;

    /// <summary>
    ///     The maximum number of times this item can be used before it is removed from the player's inventory.
    /// </summary>
    [DataField]
    public int MaxUses { get; private set; } = 1;

    /// <summary>
    ///     Tags associated with this item.
    /// </summary>
    [DataField]
    public HashSet<string> Tags = new();

    /// <summary>
    ///     Conditions evaluated before activating an item. If any condition fails, the item will not be used.
    /// </summary>
    [DataField(customTypeSerializer: typeof(OptionalAbstractTypeSerializer<CurrencyStoreCondition>))]
    public CurrencyStoreCondition[] Conditions = [];

    /// <summary>
    ///     Effects executed after activating an item.
    /// </summary>
    [DataField(required: true, serverOnly: true)]
    public CurrencyStoreEffect[] Effects = default!;
}
