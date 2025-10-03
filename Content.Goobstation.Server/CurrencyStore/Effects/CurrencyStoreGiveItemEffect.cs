using Content.Goobstation.Shared.CurrencyStore;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Effects;

public sealed partial class CurrencyStoreGiveItemEffect : CurrencyStoreEffect
{
    [DataField(required: true)]
    public EntProtoId Prototype = default!;

    public override void ExecuteEffect(EntityUid player, IEntityManager entityManager)
    {
        // Create the item
        var thing = entityManager.SpawnEntity(Prototype, entityManager.GetComponent<TransformComponent>(player).Coordinates);

        // Put the item in the player's hand
        entityManager.System<SharedHandsSystem>().PickupOrDrop(player, thing);
    }

    public override CurrencyStoreRoundState GetAllowedRoundStates()
    {
        return CurrencyStoreRoundState.InRound;
    }
}
