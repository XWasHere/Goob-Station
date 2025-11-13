using System.Linq;
using Content.Client.Gameplay;
using Content.Goobstation.Client.CurrencyStore.Managers;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CurrencyStore.UI;

public sealed class CurrencyStoreUIController : UIController, IOnStateChanged<GameplayState>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly ICommonCurrencyManager _currency = default!;
    [Dependency] private readonly IClientCurrencyStoreManager _currencyStore = default!;

    private CurrencyStoreWindow? _window;

    private bool _showHiddenItems;

    public override void Initialize()
    {
        base.Initialize();

        _config.OnValueChanged(GoobCVars.CurrencyStoreAllowPurchaseHidden, val => _showHiddenItems = val, true);
    }

    public void OnStateEntered(GameplayState state)
    {
        _window = UIManager.CreateWindow<CurrencyStoreWindow>();
        _window.OnCategoryButtonPressed += OnCategoryButtonPressed;
    }

    public void OnStateExited(GameplayState state)
    {
        if (_window == null)
            return;

        _window.Close();
        _window.OnCategoryButtonPressed -= OnCategoryButtonPressed;
        _window = null;
    }

    private void OnCategoryButtonPressed(BaseButton.ButtonEventArgs buttonEventArgs, ProtoId<CurrencyStoreCategoryPrototype> protoId)
    {
        if (_window == null)
            return;

        _window.CurrentCategory = protoId;
        PopulateStoreListings();
    }

    public void Open()
    {
        if (_window == null || _window.IsOpen)
            return;

        _window.OpenCentered();
        _window.CurrentCategory = string.Empty;

        PopulateStore();
        UpdateBalance();
    }

    private void UpdateBalance()
    {
        if (_window == null)
            return;

        _window.SetBalance(_currency.GetBalance());
    }

    private void PopulateStore()
    {
        if (_window == null)
            return;

        _window.PopulateStoreCategories(
            _proto.EnumeratePrototypes<CurrencyStoreCategoryPrototype>()
                .Where(c => c.InStore || _showHiddenItems)
                .OrderBy(c => c.Priority));

        PopulateStoreListings();
    }

    private void PopulateStoreListings()
    {
        if (_window == null)
            return;

        _window.PopulateStoreListings(
            _proto.EnumeratePrototypes<CurrencyStoreItemPrototype>()
                .Where(i => i.Category == _window.CurrentCategory && (!i.Permanent || !_currencyStore.CheckPurchasedPermanentItem(i)))
                .OrderBy(i => Loc.GetString(i.Name)));
    }
}
