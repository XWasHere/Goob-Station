namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     When an item or condition can be used
/// </summary>
[Flags]
[Serializable]
public enum CurrencyStoreRoundState
{
    /// <summary>
    ///     Items can be used before a round
    /// </summary>
    PreRound = 1,

    /// <summary>
    ///     Items can be used during a round, while the player is playing or observing that round.
    /// </summary>
    InRound = 2,

    /// <summary>
    ///     Item can be used at any time.
    /// </summary>
    Always = PreRound | InRound,
}
