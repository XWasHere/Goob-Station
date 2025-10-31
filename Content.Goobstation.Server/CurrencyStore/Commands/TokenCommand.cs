using System.Linq;
using Content.Goobstation.Server.CurrencyStore.Systems;
using Content.Goobstation.Server.CurrencyStore.Managers;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Goobstation.Server.CurrencyStore.Commands;

/// <summary>
///     Currency store item related commands. Most of these are only useful for debugging.
/// </summary>
/// <remarks>
///     "item" seems like a command that would come from upstream, so we use "token" instead.
/// </remarks>
[ToolshedCommand, AdminCommand(AdminFlags.FullAdmin | AdminFlags.Round)]
public sealed class TokenCommand : ToolshedCommand
{
    [Dependency] private readonly IServerCurrencyStoreManager _manager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [CommandImplementation("inv")]
    public IEnumerable<CurrencyStoreInventoryItem> Inv(IInvocationContext ctx, [PipedArgument] ICommonSession session)
    {
        return _manager.GetInventory(session.UserId);
    }

    [CommandImplementation("add")]
    public IEnumerable<CurrencyStoreInventoryItem> Add(IInvocationContext ctx,
        [PipedArgument]   IEnumerable<ICommonSession> sessions,
        [CommandArgument] ProtoId<CurrencyStoreItemPrototype> prototype,
        [CommandArgument] int uses = 0,
        [CommandArgument] bool immediate = false)
    {
        var proto = _proto.Index(prototype);

        if (uses <= 0)
            uses = proto.MaxUses;

        // This always returns a value if the prototype is valid.
        return sessions.Select(s => _manager.AddItem(s.UserId, prototype, immediate, uses, ItemModificationReason.Admin, ctx.User)!);
    }

    [CommandImplementation("remove")]
    public void Remove(IInvocationContext ctx, [PipedArgument] IEnumerable<CurrencyStoreInventoryItem> items)
    {
        foreach (var item in items)
        {
            _manager.RemoveItem(item, ItemModificationReason.Admin, ctx.User);
        }
    }

    [CommandImplementation("dump")]
    public void Dump(IInvocationContext ctx, [PipedArgument] IEnumerable<CurrencyStoreInventoryItem> items)
    {
        foreach (var item in items)
        {
            ctx.WriteLine($"""
                item {item.Id}:
                    owner: {item.Owner}
                    prototype: {item.Prototype}
                    immediate: {item.Immediate}
                    uses: {item.UsesLeft}
                """);
        }
    }
}

