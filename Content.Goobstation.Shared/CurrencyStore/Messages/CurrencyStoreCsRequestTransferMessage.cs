using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore.Messages;

/// <summary>
///     Message sent from the client to the server requesting to transfer an item to
///     another player.
/// </summary>
public sealed class CurrencyStoreCsRequestTransferMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    /// <summary>
    ///     The type of item being transferred. Permanent items are non-transferrable.
    /// </summary>
    public TransferType Type;

    /// <summary>
    ///     The database ID of the item to transfer.
    /// </summary>
    public int Id;

    /// <summary>
    ///     The user id of the user to transfer the item to.
    /// </summary>
    public NetUserId Target;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Type = (TransferType) buffer.ReadByte();
        Id = buffer.ReadVariableInt32();
        Target = new NetUserId(buffer.ReadGuid());
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write((byte) Type);
        buffer.WriteVariableInt32(Id);
        buffer.Write(Target);
    }

    /// <summary>
    ///     Enum representing the type of item to transfer
    /// </summary>
    public enum TransferType : byte
    {
        Item, Voucher
    }
}
