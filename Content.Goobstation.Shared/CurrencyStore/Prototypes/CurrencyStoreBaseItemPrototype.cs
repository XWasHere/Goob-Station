using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Prototypes;

public abstract class CurrencyStoreBaseItemPrototype : IPrototype
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
}
