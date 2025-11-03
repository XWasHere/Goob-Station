using Content.Goobstation.Server.CurrencyStore.Managers;
using Content.Goobstation.Server.CurrencyStore.Systems;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Syntax;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Goobstation.Server.CurrencyStore.Commands;

// TODO(XWH): Add actual toolshed errors.
/// <summary>
///     Misc store commands. Lets you forcibly activate, redeem or transfer items.
/// </summary>
/// <remarks>
///     All of these are debug commands intended for store development, are not reliable, and are
///     not equivalent to a player executing these operations. Do not use these commands in scripts
///     for any reason.
/// </remarks>
[ToolshedCommand, AdminCommand(AdminFlags.FullAdmin | AdminFlags.Round | AdminFlags.Debug)]
public sealed class CurrencyStoreCommand : ToolshedCommand
{
    [Dependency] private readonly IServerCurrencyStoreManager _manager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [CommandImplementation("transfer")]
    public void Transfer(IInvocationContext ctx,
        [PipedArgument]   IEnumerable<CurrencyStoreInventoryItem> items,
        [CommandArgument] ICommonSession to)
    {
        foreach (var item in items)
        {
            if (!_manager.TryTransferItem(item, to.UserId, out var result))
                ctx.WriteLine($"error transferring item {item.Id}: {result}");
        }
    }

    [CommandImplementation("transfer")]
    public void Transfer(IInvocationContext ctx,
        [PipedArgument]   IEnumerable<CurrencyStoreVoucher> vouchers,
        [CommandArgument] ICommonSession to)
    {
        foreach (var voucher in vouchers)
        {
            if (!_manager.TryTransferVoucher(voucher, to.UserId, out var result))
                ctx.WriteLine($"error transferring item {voucher.Id}: {result}");
        }
    }

    [CommandImplementation("activate")]
    public void Activate(IInvocationContext ctx, [PipedArgument] IEnumerable<CurrencyStoreInventoryItem> items)
    {
        foreach (var item in items)
        {
            if (!GetSys<ServerCurrencyStoreSystem>().TryActivateItem(item, out var result))
            {
                ctx.WriteLine($"Failed to activate item [{item.Id} {item.Prototype}]: {result}");
            }
        }
    }

    [CommandImplementation("redeem")]
    public void Redeem(IInvocationContext ctx,
        [PipedArgument]   IEnumerable<CurrencyStoreVoucher> vouchers,
        [CommandArgument] ProtoId<CurrencyStoreItemPrototype> item)
    {
        foreach (var voucher in vouchers)
        {
            if (!_manager.TryRedeemVoucher(voucher, item, out var result))
                ctx.WriteLine($"error redeeming voucher {voucher.Id} for {item}: {result}");
        }
    }

    [CommandImplementation("setprice")]
    public void SetPrice(IInvocationContext ctx,
        [CommandArgument] ProtoId<CurrencyStoreItemPrototype> item,
        [CommandArgument] int price)
    {
        _manager.SetDynamicItemPrice(item, price);
    }
}
