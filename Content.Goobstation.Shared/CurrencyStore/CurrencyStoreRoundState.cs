namespace Content.Goobstation.Shared.CurrencyStore;

/// <summary>
///     When an item or condition can be used
/// </summary>
[Serializable]
public enum CurrencyStoreRoundState
{
    /// <summary>
    ///     Item can be used at any time.
    /// </summary>
    Always,

    /// <summary>
    ///     Items can be used before a round
    /// </summary>
    PreRound,

    /// <summary>
    ///     Items can be used during a round, while the player is playing or observing that round.
    /// </summary>
    InRound
}
