using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CurrencyStore.UI;

public sealed class CurrencyStoreItemListing : CurrencyStoreListing
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public event Action<BaseButton.ButtonEventArgs, ProtoId<CurrencyStoreItemPrototype>>? OnPurchaseButtonPressed;

    public ProtoId<CurrencyStoreItemPrototype> Prototype;
    public int Price;

    public CurrencyStoreItemListing(ProtoId<CurrencyStoreItemPrototype> proto, int price)
    {
        IoCManager.InjectDependencies(this);

        Prototype = proto;
        Price = price;

        ActivateButton.OnPressed += args => OnPurchaseButtonPressed?.Invoke(args, Prototype);

        Refresh();
    }

    public void SetPrice(int price)
    {
        Price = price;
        Refresh();
    }

    public void Refresh()
    {
        if (!_proto.TryIndex(Prototype, out var proto))
            return;

        ApplyItemPrototype(proto);

        ActivateButton.Text = Loc.GetString("server-currency-name-amount", ("amount", Price));
        if (Price != proto.Price)
        {
            ItemBasePrice.Visible = true;
            ItemBasePrice.Text = Loc.GetString("currencystore-ui-base-price",
                ("price", Loc.GetString("server-currency-name-amount", ("amount", proto.Price))));
        }
        else
        {
            ItemBasePrice.Visible = false;
        }

        AddTrait(Loc.GetString("currencystore-item-trait-uses", ("uses", proto.MaxUses)));
    }
}
