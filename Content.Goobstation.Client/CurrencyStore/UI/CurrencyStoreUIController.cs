using System.Linq;
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Goobstation.Client.CurrencyStore.Managers;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CurrencyStore.UI;

public sealed class CurrencyStoreUIController : UIController, IOnStateChanged<GameplayState>, IOnStateChanged<LobbyState>
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

        _currencyStore.OnItemUpdate += OnItemUpdate;
        _currency.ClientBalanceChange += OnBalanceChange;
    }

    private void OnItemUpdate(ProtoId<CurrencyStoreItemPrototype> id, CurrencyStoreItemData data)
    {
        // Don't bother updating the window if it's closed, it will be
        // rebuilt when it's reopened
        if (_window == null || !_window.IsOpen)
            return;

        _window.UpdateItem(id, data);
    }

    private void OnBalanceChange()
    {
        if (_window == null || !_window.IsOpen)
            return;

        _window.SetBalance(_currency.GetBalance());
    }

    public void OnStateEntered(GameplayState state)
    {
        InitWindows();
    }

    public void OnStateExited(GameplayState state)
    {
        ShutdownWindows();
    }

    public void OnStateEntered(LobbyState state)
    {
        InitWindows();
    }

    public void OnStateExited(LobbyState state)
    {
        ShutdownWindows();
    }

    private void InitWindows()
    {
        _window = UIManager.CreateWindow<CurrencyStoreWindow>();
        _window.OnCategoryButtonPressed += OnCategoryButtonPressed;
        _window.OnPurchaseButtonPressed += OnPurchaseButtonPressed;
    }

    private void ShutdownWindows()
    {
        if (_window == null)
            return;

        _window.Close();
        _window.OnCategoryButtonPressed -= OnCategoryButtonPressed;
        _window.OnPurchaseButtonPressed -= OnPurchaseButtonPressed;
        _window = null;
    }

    private void OnCategoryButtonPressed(BaseButton.ButtonEventArgs args, ProtoId<CurrencyStoreCategoryPrototype> proto)
    {
        if (_window == null)
            return;

        _window.CurrentCategory = proto;
        PopulateStoreListings();
    }

    private void OnPurchaseButtonPressed(BaseButton.ButtonEventArgs args, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        if (_window == null)
            return;

        // No confirmation. Not a check in sight. Just people living in the moment.
        _currencyStore.RequestPurchaseItem(proto);
    }

    public void Open()
    {
        if (_window == null)
            return;

        // Reset the window state every time we open it.
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
