using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore.Messages;

/// <summary>
///     Request sent from the client to the server to purchase an item.
/// </summary>
public sealed class CurrencyStoreCsRequestPurchaseMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    /// <summary>
    ///     The item to purchase
    /// </summary>
    public ProtoId<CurrencyStoreItemPrototype> Item;

    /// <summary>
    ///     The voucher to use to purchase this item, if redeeming a voucher.
    /// </summary>
    public int? Voucher;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Item = buffer.ReadString();
        Voucher = buffer.ReadBoolean() ? buffer.ReadVariableInt32() : null;
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Item);
        buffer.Write(Voucher.HasValue);
        if (Voucher.HasValue)
        {
            buffer.Write(Voucher.Value);
        }
    }
}
