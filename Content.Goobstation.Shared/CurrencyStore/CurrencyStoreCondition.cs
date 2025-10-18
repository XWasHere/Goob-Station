using Robust.Shared.Network;

namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     The base class for all store conditions. Any class that inherits from this can be used
///     as a condition in a CurrencyStoreItem.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class CurrencyStoreCondition
{
    /// <summary>
    ///     When the condition can be executed.
    /// </summary>
    public abstract CurrencyStoreRoundState AllowedRoundState { get; }

    /// <summary>
    ///     Evaluate the condition. If this returns false, the item it is attached to can not be used.
    /// </summary>
    /// <param name="player">The player user id</param>
    /// <returns>If the condition passes</returns>
    public abstract bool EvaluateCondition(NetUserId player, IEntityManager entityManager);

    /// <summary>
    ///     Get the description of the condition that is displayed to the user. If a condition fails,
    ///     the description will be presented to the user in a message. The description should be
    ///     clear and concise and should be able to be displayed in a single line.
    /// </summary>
    public abstract string GetLocalizedDescription();
}
