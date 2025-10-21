using System.Threading.Tasks;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Messages;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CurrencyStore.Managers;

public sealed class ClientCurrencyStoreManager : IClientCurrencyStoreManager
{
    [Dependency] private readonly INetManager _net = default!;

    private ISawmill _sawmill = default!;

    #region Lifecycle

    public void Initialize()
    {
        // Register network messages
        _net.RegisterNetMessage<CurrencyStoreScRefreshMessage>(OnRefresh);
        _net.RegisterNetMessage<CurrencyStoreScRefreshStoreMessage>(OnRefreshStore);
        _net.RegisterNetMessage<CurrencyStoreScResultMessage>();
        _net.RegisterNetMessage<CurrencyStoreCsRequestPurchaseMessage>();
        _net.RegisterNetMessage<CurrencyStoreCsRequestTransferMessage>();

        // Get sawmill
        _sawmill = Logger.GetSawmill("currency_store");
    }

    public void Shutdown()
    {

    }

    #endregion

    #region Netcode

    private void OnRefresh(CurrencyStoreScRefreshMessage message)
    {
        _sawmill.Debug("got player refresh from server");
        foreach (var item in message.Inventory ?? [])
        {
            _sawmill.Debug($"Got item data [{item.Id} {item.Prototype} {item.UsesLeft} {item.Immediate}]");
        }
    }

    private void OnRefreshStore(CurrencyStoreScRefreshStoreMessage message)
    {
        // TODO: Replace this with real code
        _sawmill.Debug("got store refresh from server");
        foreach (var record in message.UpdatedItems)
        {
            _sawmill.Debug($"Added item data [{record.Key}] = {record.Value.Price}");
        }
    }

    #endregion

    #region Public Interface

    public List<CurrencyStoreInventoryItem> GetInventory(NetUserId uid)
    {
        throw new NotImplementedException();
    }

    public bool CanAfford(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item)
    {
        throw new NotImplementedException();
    }

    public bool CanPurchaseItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> item)
    {
        throw new NotImplementedException();
    }

    public bool CanActivateItem(CurrencyStoreInventoryItem item)
    {
        throw new NotImplementedException();
    }

    public Task<HashSet<ProtoId<CurrencyStoreItemPrototype>>> GetPurchasedPermanentItems(NetUserId uid)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckPurchasedPermanentItem(NetUserId uid, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public void RequestPurchaseItem(ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public void RequestActivateItem(CurrencyStoreInventoryItem item)
    {
        throw new NotImplementedException();
    }

    public void RequestTransferItem(CurrencyStoreInventoryItem item, string target)
    {
        throw new NotImplementedException();
    }

    public Task<List<CurrencyStoreVoucher>> GetVouchers(NetUserId uid)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CanRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public void RequestRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public void RequestTransferVoucher(CurrencyStoreVoucher voucher, string target)
    {
        throw new NotImplementedException();
    }

    # endregion
}
