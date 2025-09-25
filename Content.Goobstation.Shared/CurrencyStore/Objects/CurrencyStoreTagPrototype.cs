using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Objects;

/// <summary>
///     A tag for a store item, used by vouchers to determine if an item can
///     be redeemed.
/// </summary>
/// <remarks>
///     Store tags are kept separate from entity tags, as while they serve a similar
///     purpose, keeping store tags independent allows them to use names that may be used
///     by existing entity tags.
/// </remarks>
[Prototype]
public sealed partial class CurrencyStoreTagPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;
}
