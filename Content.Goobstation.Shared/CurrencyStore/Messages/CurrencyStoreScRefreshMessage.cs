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
/// <remarks>
///     Resending every instance of a certain item every refresh certainly isn't
///     efficient, though because every item is only a handful of bytes, it should
///     be ok.
/// </remarks>
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
        if (inventoryLength >= 0)
        {
            Inventory = [];
            for (var i = 0; i < inventoryLength; i++)
            {
                Inventory.Add(new CurrencyStoreInventoryItem
                {
                    Id = buffer.ReadInt32(),
                    Owner = new NetUserId(buffer.ReadGuid()),
                    Prototype = buffer.ReadString(),
                    Immediate = buffer.ReadBoolean(),
                    UsesLeft = buffer.ReadInt32(),
                });
            }
        }

        // Read vouchers
        var vouchersLength = buffer.ReadVariableInt32();
        if (vouchersLength >= 0)
        {
            Vouchers = [];
            for (var i = 0; i < vouchersLength; i++)
            {
                Vouchers.Add(new CurrencyStoreVoucher
                {
                    Id = buffer.ReadInt32(),
                    Owner = new NetUserId(buffer.ReadGuid()),
                    Prototype = buffer.ReadString(),
                    UsesLeft = buffer.ReadInt32(),
                });
            }
        }

        // Read permanent items
        var permanentLength = buffer.ReadVariableInt32();
        if (permanentLength >= 0)
        {
            PermanentItems = [];
            for (var i = 0; i < permanentLength; i++)
            {
                PermanentItems.Add(buffer.ReadString());
            }
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        // Write inventory
        buffer.WriteVariableInt32(Inventory?.Count ?? -1);
        foreach (var item in Inventory ?? [])
        {
            buffer.Write(item.Id);
            buffer.Write(item.Owner);
            buffer.Write(item.Prototype);
            buffer.Write(item.Immediate);
            buffer.Write(item.UsesLeft);
        }

        // Write vouchers
        buffer.WriteVariableInt32(Vouchers?.Count ?? -1);
        foreach (var item in Vouchers ?? [])
        {
            buffer.Write(item.Id);
            buffer.Write(item.Owner);
            buffer.Write(item.Prototype);
            buffer.Write(item.UsesLeft);
        }

        // Write permanent items
        buffer.WriteVariableInt32(PermanentItems?.Count ?? -1);
        foreach (var item in PermanentItems ?? [])
        {
            buffer.Write(item);
        }
    }
}
