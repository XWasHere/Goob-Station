using Content.Client.Eui;
using Content.Shared.Eui;

namespace Content.Goobstation.Client.CurrencyStore.UI;

/// <remarks>
///     For my game, I'm going to add three completely separate UI systems,
///     and I'm going to document none of them in any meaningful capacity.
///     - Adolf Hitler
/// </remarks>
public sealed class CurrencyStoreEui : BaseEui
{
    public CurrencyStoreEui()
    {
        StoreWindow = new CurrencyStoreWindow();
        StoreWindow.OnClose += () => SendMessage(new CloseEuiMessage());
    }

    private CurrencyStoreWindow? StoreWindow { get; set; }

    public override void Opened()
    {
        base.Opened();

        StoreWindow?.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        StoreWindow?.Close();
    }
}
