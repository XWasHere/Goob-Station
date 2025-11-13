using Content.Goobstation.Client.CurrencyStore.UI;
using Content.Shared.Administration;
using Robust.Client.UserInterface;
using Robust.Shared.Console;

namespace Content.Goobstation.Client.CurrencyStore.Commands;

/// <summary>
///     This is slop. Opens the currency store window.
/// </summary>
[AnyCommand]
public sealed class OpenCurrencyStoreCommand : LocalizedCommands
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    public override string Command => "opencurrencystore";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _ui.GetUIController<CurrencyStoreUIController>().Open();
    }
}
