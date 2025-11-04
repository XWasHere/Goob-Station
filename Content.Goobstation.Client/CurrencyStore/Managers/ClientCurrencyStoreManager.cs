using System.Linq;
using System.Threading.Tasks;
using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.CurrencyStore;
using Content.Goobstation.Shared.CurrencyStore.Messages;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.CurrencyStore.Managers;

public sealed class ClientCurrencyStoreManager : IClientCurrencyStoreManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ICommonCurrencyManager _currency = default!;

    private ISawmill _sawmill = default!;

    /// <summary>
    ///     Permanent items sent by the server
    /// </summary>
    private HashSet<ProtoId<CurrencyStoreItemPrototype>> _cachedPermanentInventory = new();

    /// <summary>
    ///     Item data sent from the server.
    /// </summary>
    private Dictionary<ProtoId<CurrencyStoreItemPrototype>, CurrencyStoreItemData> _cachedItemData = new();

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
        if (message.PermanentItems != null)
            _cachedPermanentInventory = message.PermanentItems.ToHashSet();
    }

    private void OnRefreshStore(CurrencyStoreScRefreshStoreMessage message)
    {
        foreach (var record in message.UpdatedItems)
        {
            _cachedItemData[record.Key] = record.Value;
        }
    }

    #endregion

    #region Public Interface

    public List<CurrencyStoreInventoryItem> GetInventory()
    {
        throw new NotImplementedException();
    }

    public bool CanAfford(ProtoId<CurrencyStoreItemPrototype> item)
    {
        return _currency.CanAfford(null, GetItemDynamicPrice(item), out _);
    }

    public bool CanPurchaseItem(ProtoId<CurrencyStoreItemPrototype> item)
    {
        throw new NotImplementedException();
    }

    public HashSet<ProtoId<CurrencyStoreItemPrototype>> GetPurchasedPermanentItems()
    {
        return _cachedPermanentInventory;
    }

    public bool CheckPurchasedPermanentItem(ProtoId<CurrencyStoreItemPrototype> proto)
    {
        return _cachedPermanentInventory.Contains(proto);
    }

    public void RequestPurchaseItem(ProtoId<CurrencyStoreItemPrototype> proto)
    {
        throw new NotImplementedException();
    }

    public void RequestTransferItem(CurrencyStoreInventoryItem item, string target)
    {
        throw new NotImplementedException();
    }

    public List<CurrencyStoreVoucher> GetVouchers()
    {
        throw new NotImplementedException();
    }

    public bool CanRedeemVoucher(CurrencyStoreVoucher voucher, ProtoId<CurrencyStoreItemPrototype> proto)
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

    public int GetItemDynamicPrice(ProtoId<CurrencyStoreItemPrototype> proto)
    {
        if (_cachedItemData.TryGetValue(proto, out var value))
            return value.Price;

        if (_proto.TryIndex(proto, out var prototype))
            return prototype.Price;

        return -1;
    }

    # endregion
}
