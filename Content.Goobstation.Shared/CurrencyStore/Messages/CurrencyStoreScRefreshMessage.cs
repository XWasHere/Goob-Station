using System.IO;
using Content.Goobstation.Shared.CurrencyStore.Prototypes;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.CurrencyStore.Messages;

/// <summary>
///     A message sent from the server to the client notifying it that a player's
///     inventory has been updated. Null fields have not been changed, while populated
///     fields should be updated by the client CurrencyStoreManager.
/// </summary>
public sealed class CurrencyStoreScRefreshMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Core;

    /// <summary>
    ///     All items in a player's inventory, if they have changed
    /// </summary>
    public List<CurrencyStoreInventoryItem>? Inventory;

    /// <summary>
    ///     All vouchers in a player's inventory, if they have changed
    /// </summary>
    public List<CurrencyStoreVoucher>? Vouchers;

    /// <summary>
    ///     All permanent items owned by a player, if they have changed
    /// </summary>
    public List<ProtoId<CurrencyStoreItemPrototype>>? PermanentItems;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        // Read inventory
        var inventoryLength = buffer.ReadVariableInt32();
        if (inventoryLength != 0)
        {
            using var stream = new MemoryStream(inventoryLength);
            buffer.ReadAlignedMemory(stream, inventoryLength);
            serializer.DeserializeDirect(stream, out Inventory);
        }

        // Read vouchers
        var vouchersLength = buffer.ReadVariableInt32();
        if (vouchersLength != 0)
        {
            using var stream = new MemoryStream(vouchersLength);
            buffer.ReadAlignedMemory(stream, vouchersLength);
            serializer.DeserializeDirect(stream, out Vouchers);
        }

        // Read permanent items
        var permanentLength = buffer.ReadVariableInt32();
        if (permanentLength != 0)
        {
            using var stream = new MemoryStream(permanentLength);
            buffer.ReadAlignedMemory(stream, permanentLength);
            serializer.DeserializeDirect(stream, out List<string> strings);

            // Convert strings to items
            PermanentItems = new List<ProtoId<CurrencyStoreItemPrototype>>();
            foreach (var str in strings)
            {
                PermanentItems.Add(str);
            }
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        // Write inventory
        if (Inventory != null)
        {
            using var stream = new MemoryStream();
            serializer.SerializeDirect(stream, Inventory);
            buffer.WriteVariableInt32((int) stream.Length);
            stream.TryGetBuffer(out var segment);
            buffer.Write(segment);
        }
        else
        {
            buffer.WriteVariableInt32(0);
        }

        // Write vouchers
        if (Vouchers != null)
        {
            using var stream = new MemoryStream();
            serializer.SerializeDirect(stream, Vouchers);
            buffer.WriteVariableInt32((int) stream.Length);
            stream.TryGetBuffer(out var segment);
            buffer.Write(segment);
        }
        else
        {
            buffer.WriteVariableInt32(0);
        }

        // Write permanent items
        if (PermanentItems != null)
        {
            // Convert items to strings
            List<string> strings = [];
            foreach (var proto in PermanentItems)
            {
                strings.Add(proto);
            }

            using var stream = new MemoryStream();
            serializer.SerializeDirect(stream, strings);
            buffer.WriteVariableInt32((int) stream.Length);
            stream.TryGetBuffer(out var segment);
            buffer.Write(segment);
        }
        else
        {
            buffer.WriteVariableInt32(0);
        }
    }
}
