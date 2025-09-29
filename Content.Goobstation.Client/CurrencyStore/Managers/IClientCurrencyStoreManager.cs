using Content.Goobstation.Shared.CurrencyStore.Managers;

namespace Content.Goobstation.Client.CurrencyStore.Managers;

public interface IClientCurrencyStoreManager : ISharedCurrencyStoreManager
{
    #region Lifecycle

    public void Initialize();

    public void Shutdown();

    #endregion
}

