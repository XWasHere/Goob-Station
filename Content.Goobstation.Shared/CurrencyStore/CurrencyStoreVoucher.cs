using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     Structure representing a single owned voucher.
/// </summary>
[Serializable, NetSerializable]
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

    /// <summary>
    ///     The number of uses remaining on this voucher
    /// </summary>
    public int UsesLeft;
}
