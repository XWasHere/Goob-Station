using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     Structure representing a single owned voucher.
///
///     See GoobCurrencyStoreVoucher for the in-database structure.
/// </summary>
public sealed class CurrencyStoreVoucher
{
    /// <summary>
    ///     The database id of this voucher
    /// </summary>
    public int Id;

    /// <summary>
    ///     The owner of this voucher
    /// </summary>
    public NetUserId Owner;

    /// <summary>
    ///     The prototype for this voucher.
    /// </summary>
    public ProtoId<CurrencyStoreVoucherPrototype> Prototype;
}
