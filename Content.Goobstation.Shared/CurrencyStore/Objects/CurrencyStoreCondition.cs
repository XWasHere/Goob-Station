namespace Content.Goobstation.Shared.CurrencyStore.Objects;

/// <summary>
///     The base class for all store conditions. Any class that inherits from this can be used
///     as a condition in a CurrencyStoreItem.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class CurrencyStoreCondition
{
    /// <summary>
    ///     The description displayed to the user, can be modified by items if they want to display
    ///     a different description.
    /// </summary>
    [DataField("description")]
    public abstract LocId LocalizedDescription { get; protected set; }

    /// <summary>
    ///     Evaluate the condition. If this returns false, the item it is attached to can not be used.
    /// </summary>
    /// <param name="player">The player entity</param>
    /// <param name="entityManager">EntityManager system</param>
    /// <returns>If the condition passes</returns>
    public abstract bool EvaluateCondition(EntityUid player, IEntityManager entityManager);

    /// <summary>
    ///     Get the description of the condition that is displayed to the user. If an effect fails to
    ///     activate, it will be presented to the user in a message. The description should be clear and concise,
    ///     and should be able to be displayed in a single line.
    /// </summary>
    public string GetLocalizedDescription()
    {
        return Loc.GetString(LocalizedDescription);
    }

    /// <summary>
    ///     Get allowed round states for the condition to be executed. Returns <see cref="CurrencyStoreRoundState.Always">Always</see>
    ///     if not overridden
    /// </summary>
    public CurrencyStoreRoundState GetAllowedRoundStates()
    {
        return CurrencyStoreRoundState.Always;
    }
}
