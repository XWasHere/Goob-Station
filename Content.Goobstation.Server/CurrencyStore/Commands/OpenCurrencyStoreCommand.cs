using Content.Goobstation.Server.CurrencyStore.UI;
using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;

namespace Content.Goobstation.Server.CurrencyStore.Commands;

[ToolshedCommand, AnyCommand]
public sealed class OpenCurrencyStoreCommand : ToolshedCommand
{
    [Dependency] private readonly EuiManager _eui = default!;

    [CommandImplementation]
    public void OpenCurrencyStore(IInvocationContext ctx)
    {
        if (ctx.Session == null)
        {
            ctx.WriteError(new NotForServerConsoleError());
            return;
        }

        _eui.OpenEui(new CurrencyStoreEui(), ctx.Session);
    }
}
