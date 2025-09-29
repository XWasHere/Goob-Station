using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     Class representing a single owned item in a player's inventory.
///
///     See GoobCurrencyStoreInventoryItem for the in-database structure.
/// </summary>
public sealed class CurrencyStoreInventoryItem
{
    /// <summary>
    ///     The database id of this item.
    /// </summary>
    public int Id;

    /// <summary>
    ///     The owner of this item.
    /// </summary>
    public NetUserId Owner;

    /// <summary>
    ///     The prototype for this item.
    /// </summary>
    public ProtoId<CurrencyStoreItemPrototype> Prototype;

    /// <summary>
    ///     If this item should be used immediately
    /// </summary>
    /// <remarks>
    ///     This may not always line up with what's specified in the prototype,
    ///     if an admin gives a player an item, it won't have this property set
    ///     so that a player can redeem this item whenever they want.
    /// </remarks>
    public bool Immediate;

    /// <summary>
    ///     How many uses are left.
    /// </summary>
    public int UsesLeft;
}
