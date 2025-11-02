using Content.Client.Eui;
using Content.Shared.Eui;

namespace Content.Goobstation.Client.CurrencyStore.UI;

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
