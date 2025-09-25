using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CurrencyStore.Objects;

/// <summary>
///     Currency store category.
/// </summary>
/// <remarks>
///     Be careful when adding new categories, the item you are adding will almost
///     always fit into an already existing one without cluttering the UI.
/// </remarks>
[Prototype]
public sealed partial class CurrencyStoreCategoryPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Localized name of the category.
    /// </summary>
    [DataField(required: true)]
    public LocId Name { get; private set; }

    /// <summary>
    ///     Localized description of the category.
    /// </summary>
    [DataField(required: true)]
    public LocId Description { get; private set; }

    /// <summary>
    ///     If true, this category will be displayed in the store's category list.
    /// </summary>
    [DataField]
    public bool InStore { get; private set; } = true;
}
