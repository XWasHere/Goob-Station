using Content.Shared.Ghost;
using Content.Shared.Mind;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.CurrencyStore.Conditions;

public sealed partial class CurrencyStoreLivingCondition : CurrencyStoreCondition
{
    public override CurrencyStoreRoundState AllowedRoundState => CurrencyStoreRoundState.InRound;

    public override bool EvaluateCondition(NetUserId player, IEntityManager entityManager)
    {
        var playerManager = IoCManager.Resolve<ISharedPlayerManager>();

        // Get player
        var playerEnt = playerManager.GetSessionById(player).AttachedEntity;
        if (playerEnt == null)
            return false;

        // Return true if player is not a ghost.
        return !entityManager.HasComponent<GhostComponent>(playerEnt.Value);
    }

    public override string GetLocalizedDescription()
    {
        // TODO(XWH): Description
        return "";
    }
}
