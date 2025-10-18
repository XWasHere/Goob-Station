using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore.Events;

/// <summary>
///     Event raised to request activation of an item.
/// </summary>
[Serializable, NetSerializable]
public sealed class CurrencyStoreActivateItemRequestEvent : EntityEventArgs
{
    /// <summary>
    ///     The database ID of the item to activate
    /// </summary>
    public int Item;
}
