using System.IO;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore.Messages;

/// <summary>
///     A message sent from the server to the client to notify it that the server-wide
///     currency store has been updated. This is kept separate from the rest of the
///     store state updates, as instead of sending every item if it's been updated, it
///     instead only sends the items that have been changed.
/// </summary>
public sealed class CurrencyStoreScRefreshStoreMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    /// <summary>
    ///     Any items that have been updated. When this message is recieved, the client
    ///     should update any records that it already has, and add new ones if it doesn't
    ///     have them. When handling this message, the client should not discard records
    ///     for items that are not on the list.
    /// </summary>
    public Dictionary<ProtoId<CurrencyStoreItemPrototype>, CurrencyStoreItemData> UpdatedItems = default!;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        // Read updated items
        UpdatedItems = [];
        var count = buffer.ReadVariableInt32();
        for (int i = 0; i < count; i++)
        {
            // Read proto
            var proto = buffer.ReadString();

            // Read data
            var length = buffer.ReadVariableInt32();
            using var stream = new MemoryStream();
            buffer.ReadAlignedMemory(stream, length);
            serializer.DeserializeDirect(stream, out CurrencyStoreItemData data);

            // Add to dictionary
            UpdatedItems.Add(proto, data);
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        // Write updated items
        buffer.WriteVariableInt32(UpdatedItems.Count);
        foreach (var pair in UpdatedItems)
        {
            // Serialize prototype
            buffer.Write(pair.Key);

            // Serialize data
            using var stream = new MemoryStream();
            serializer.SerializeDirect(stream, pair.Value);
            buffer.WriteVariableInt32((int) stream.Length);
            stream.TryGetBuffer(out var segment);
            buffer.Write(segment);
        }
    }
}
