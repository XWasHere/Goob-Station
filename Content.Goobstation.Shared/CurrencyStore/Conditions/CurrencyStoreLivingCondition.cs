using Content.Shared.Ghost;

namespace Content.Goobstation.Shared.CurrencyStore.Conditions;

public sealed partial class CurrencyStoreLivingCondition : CurrencyStoreCondition
{
    public override CurrencyStoreRoundState AllowedRoundState => CurrencyStoreRoundState.InRound;

    public override bool EvaluateCondition(EntityUid player, IEntityManager entityManager)
    {
        return !entityManager.HasComponent<GhostComponent>(player);
    }

    public override string GetLocalizedDescription()
    {
        // TODO: Description
        return "";
    }
}
