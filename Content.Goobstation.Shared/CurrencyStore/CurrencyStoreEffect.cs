namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     An effect that is executed when a token is activated.
/// </summary>
/// <remarks>
///     <para>
///     When implementing an effect, first check to make sure that there are no
///     other effects that already do the same thing as you. If you are making a
///     complex effect, it may be helpful to break it into multiple effects so that
///     they can be reused for other purposes.
///     </para>
///     <para>
///     Effects are not reversible, do not attempt to request a user confirm
///     that they want to use the effect, that is handled exclusively by the store
///     system. By the time effects are executing, the item is already used, and
///     can not be returned to the user without admin intervention.
///     </para>
/// </remarks>
[ImplicitDataDefinitionForInheritors]
public abstract partial class CurrencyStoreEffect
{
    /// <summary>
    ///     Executes the effect.
    /// </summary>
    /// <param name="player">The player entity</param>
    /// <param name="entityManager">EntityManager system</param>
    public abstract void ExecuteEffect(EntityUid player, IEntityManager entityManager);

    /// <summary>
    ///     Get allowed round states for the effect to be executed.
    /// </summary>
    public abstract CurrencyStoreRoundState GetAllowedRoundStates();
}
