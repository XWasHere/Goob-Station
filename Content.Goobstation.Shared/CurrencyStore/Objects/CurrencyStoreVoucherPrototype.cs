using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Objects;

/// <summary>
///     Vouchers allow players to purchase one of any item for free.
/// </summary>
[Prototype()]
public sealed partial class CurrencyStoreVoucherPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Localized name of the voucher.
    /// </summary>
    [DataField(required: true)]
    public LocId Name { get; private set; }

    /// <summary>
    ///     Localized description of the voucher.
    /// </summary>
    [DataField(required: true)]
    public LocId Description { get; private set; }

    /// <summary>
    ///     Tags that can be purchased with this voucher.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<CurrencyStoreTagPrototype>> Tags { get; private set; } = new();

    /// <summary>
    ///     Categories that can be purchased with this voucher.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<CurrencyStoreCategoryPrototype>> Categories { get; private set; } = new();
}
