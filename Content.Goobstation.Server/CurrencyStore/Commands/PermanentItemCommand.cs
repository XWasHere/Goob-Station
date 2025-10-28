using Content.Goobstation.Server.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Goobstation.Server.CurrencyStore.Commands;

/// <summary>
///     Commands for manipulating permanent items
/// </summary>
[ToolshedCommand, AdminCommand(AdminFlags.FullAdmin | AdminFlags.Round)]
public sealed class PermanentItemCommand : ToolshedCommand
{
    [Dependency] private readonly IServerCurrencyStoreManager _manager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [CommandImplementation("inv")]
    public IEnumerable<ProtoId<CurrencyStoreItemPrototype>> Inv(IInvocationContext ctx, [PipedArgument] ICommonSession session)
    {
        return _manager.GetPurchasedPermanentItems(session.UserId);
    }

    [CommandImplementation("add")]
    public void Add(IInvocationContext ctx,
        [PipedArgument]   IEnumerable<ICommonSession> sessions,
        [CommandArgument] ProtoId<CurrencyStoreItemPrototype> prototype)
    {
        var proto = _proto.Index(prototype);

        if (!proto.Permanent)
        {
            ctx.WriteLine("can not add normal item as permanent item");
            return;
        }

        foreach (var session in sessions)
        {
            _manager.SetPurchasedPermanentItem(session.UserId, proto, ItemModificationReason.Admin, ctx.User);
        }
    }

    [CommandImplementation("remove")]
    public void Remove(IInvocationContext ctx,
        [PipedArgument]   IEnumerable<ICommonSession> sessions,
        [CommandArgument] ProtoId<CurrencyStoreItemPrototype> prototype)
    {
        var proto = _proto.Index(prototype);

        if (!proto.Permanent)
        {
            ctx.WriteLine("can not add normal item as permanent item");
            return;
        }

        foreach (var session in sessions)
        {
            _manager.ClearPurchasedPermanentItem(session.UserId, prototype, ItemModificationReason.Admin, ctx.User);
        }
    }

    [CommandImplementation("dump")]
    public void Dump(IInvocationContext ctx, [PipedArgument] IEnumerable<ProtoId<CurrencyStoreItemPrototype>> items)
    {
        foreach (var item in items)
        {
            ctx.WriteLine($"perm {item}");
        }
    }
}
