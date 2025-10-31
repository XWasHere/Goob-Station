using Content.Goobstation.Shared.CurrencyStore;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Presets;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CurrencyStore.Effects;

/// <summary>
///     Do not use this in real code. It is a joke effect used for debugging.
/// </summary>
public sealed partial class CurrencyStoreSetPresetEffect : CurrencyStoreEffect
{
    [DataField(required: true)]
    public ProtoId<GamePresetPrototype> prototype = default!;

    public override CurrencyStoreRoundState AllowedRoundState => CurrencyStoreRoundState.PreRound;

    public override void ExecuteEffect(NetUserId player, IEntityManager entityManager)
    {
        entityManager.System<GameTicker>().SetGamePreset(prototype);

        if (IoCManager.Resolve<IPlayerManager>().TryGetSessionById(player, out var session))
            IoCManager.Resolve<IChatManager>().DispatchServerAnnouncement($"{session.Name} set the gamemode to {prototype}.");
    }
}
