using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     All dynamic data for a store item.
/// </summary>
[Serializable, NetSerializable]
public sealed class CurrencyStoreItemData
{
    /// <summary>
    ///     The current price of this item.
    /// </summary>
    public int Price;
}
