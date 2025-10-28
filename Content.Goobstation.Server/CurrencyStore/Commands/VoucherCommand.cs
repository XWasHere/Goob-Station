using System.Linq;
using Content.Goobstation.Server.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Goobstation.Server.CurrencyStore.Commands;

/// <summary>
///     Commands for manipulating player vouchers. Most of these are only useful for debugging.
/// </summary>
[ToolshedCommand, AdminCommand(AdminFlags.FullAdmin | AdminFlags.Round)]
public sealed class VoucherCommand : ToolshedCommand
{
    [Dependency] private readonly IServerCurrencyStoreManager _manager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [CommandImplementation("inv")]
    public IEnumerable<CurrencyStoreVoucher> Inv(IInvocationContext ctx, [PipedArgument] ICommonSession session)
    {
        return _manager.GetVouchers(session.UserId);
    }

    [CommandImplementation("add")]
    public IEnumerable<CurrencyStoreVoucher> Add(IInvocationContext ctx,
        [PipedArgument]   IEnumerable<ICommonSession> players,
        [CommandArgument] ProtoId<CurrencyStoreVoucherPrototype> prototype,
        [CommandArgument] int uses = 0)
    {
        var proto = _proto.Index(prototype);
        if (uses <= 0)
            uses = proto.MaxUses;

        // Always returns value if valid prototype
        return players.Select(s => _manager.AddVoucher(s.UserId, proto, uses, ItemModificationReason.Admin, ctx.User)!);
    }

    [CommandImplementation("remove")]
    public void Remove(IInvocationContext ctx, [PipedArgument] IEnumerable<CurrencyStoreVoucher> vouchers)
    {
        foreach (var voucher in vouchers)
        {
            _manager.RemoveVoucher(voucher, ItemModificationReason.Admin, ctx.User);
        }
    }

    [CommandImplementation("dump")]
    public void Dump(IInvocationContext ctx, [PipedArgument] IEnumerable<CurrencyStoreVoucher> vouchers)
    {
        foreach (var voucher in vouchers)
        {
            ctx.WriteLine($"""
                voucher {voucher.Id}:
                    owner: {voucher.Owner}
                    prototype: {voucher.Prototype}
                    uses: {voucher.UsesLeft}
                """);
        }
    }
}
