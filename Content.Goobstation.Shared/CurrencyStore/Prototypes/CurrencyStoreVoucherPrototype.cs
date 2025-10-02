using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Prototypes;

/// <summary>
///     Vouchers allow players to purchase one of any item for free.
/// </summary>
[Prototype]
public sealed partial class CurrencyStoreVoucherPrototype : CurrencyStoreBaseItemPrototype
{
    /// <summary>
    ///     Tags that can be purchased with this voucher.
    /// </summary>
    [DataField]
    public HashSet<string> Tags { get; private set; } = new();

    /// <summary>
    ///     Categories that can be purchased with this voucher.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<CurrencyStoreCategoryPrototype>> Categories { get; private set; } = new();

    /// <summary>
    ///     Maximum number of times this voucher can be used.
    /// </summary>
    [DataField]
    public int MaxUses { get; private set; } = 1;
}
