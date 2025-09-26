using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Objects;

/// <summary>
///     An item in the game store.
/// </summary>
[Prototype]
public sealed partial class CurrencyStoreItemPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     The name displayed to the user.
    /// </summary>
    [DataField(required: true)]
    public LocId Name { get; private set; }

    /// <summary>
    ///     The description displayed to the user.
    /// </summary>
    [DataField(required: true)]
    public LocId Description { get; private set; }

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
    public HashSet<ProtoId<CurrencyStoreTagPrototype>> Tags = new();

    /// <summary>
    ///     Conditions evaluated before activating an item. If any condition fails, the item will not be used.
    /// </summary>
    [DataField]
    public List<CurrencyStoreCondition> Conditions = new();

    /// <summary>
    ///     Effects executed after activating an item.
    /// </summary>
    [DataField(required: true)]
    public List<CurrencyStoreEffect> Effects = new();
}
