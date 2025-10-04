using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore.Messages;

/// <summary>
///     Generic request result type. Used to indicate success or
///     failure of an item redemption/voucher redemption/item transfer
/// </summary>
public sealed class CurrencyStoreScResultMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    /// <summary>
    ///     The outcome of the request.
    /// </summary>
    public CurrencyStoreScResultValue Outcome;

    /// <summary>
    ///     An string describing the outcome to the user.
    /// </summary>
    public string Reason = null!;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Outcome = (CurrencyStoreScResultValue) buffer.ReadSByte();
        Reason = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write((sbyte) Outcome);
        buffer.Write(Reason);
    }

    public enum CurrencyStoreScResultValue : sbyte
    {
        /// <summary>
        ///     Generic failure
        /// </summary>
        Failure = -1,

        /// <summary>
        ///     Generic success
        /// </summary>
        Success =  1,
    }
}
