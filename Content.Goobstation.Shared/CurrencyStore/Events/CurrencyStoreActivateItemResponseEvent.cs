using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore.Events;

/// <summary>
///     Event raised by the server indicating if an item was successfully activated or not.
/// </summary>
[Serializable, NetSerializable]
public sealed class CurrencyStoreActivateItemResponseEvent : EntityEventArgs
{
    /// <summary>
    ///     If the item was activated successfully.
    /// </summary>
    public bool Success;

    /// <summary>
    ///     An optional error message to be displayed to the user
    /// </summary>
    public string? Reason;
}
