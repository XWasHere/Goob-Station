using Content.Goobstation.Shared.CurrencyStore;
using Content.Server.Mind;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Effects;

public sealed partial class CurrencyStoreGiveItemEffect : CurrencyStoreEffect
{
    [DataField(required: true)]
    public EntProtoId Prototype = default!;

    public override CurrencyStoreRoundState AllowedRoundState => CurrencyStoreRoundState.InRound;

    public override void ExecuteEffect(NetUserId player, IEntityManager entityManager)
    {
        // Get player entity
        var playerEnt = IoCManager.Resolve<IPlayerManager>().GetSessionById(player).AttachedEntity;
        if (playerEnt == null)
            return;

        // Create the item
        var thing = entityManager.SpawnEntity(Prototype, entityManager.GetComponent<TransformComponent>(playerEnt.Value).Coordinates);

        // Put the item in the player's hand
        entityManager.System<SharedHandsSystem>().PickupOrDrop(playerEnt.Value, thing);
    }
}
