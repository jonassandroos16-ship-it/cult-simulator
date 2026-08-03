namespace CultSimulator.Game;

/// <summary>
/// Per-coven mutable state. The home coven (Id == "skanor") is the player's
/// own coven; others are rival covens that can be converted.
/// </summary>
public class CovenState
{
    public string Id { get; set; } = "";
    public int Followers { get; set; }
    public double Faith { get; set; }
    public double Gold { get; set; }
    public int PreachCount { get; set; }
    public Dictionary<BuildingType, int> Buildings { get; set; } = new();
    public List<UpgradeId> Upgrades { get; set; } = new();

    /// <summary>
    /// True after this rival coven has been converted through the
    /// narrative siege. Replaces the former TakenOver field.
    /// </summary>
    public bool Converted { get; set; }

    /// <summary>
    /// Backwards-compat: older saves stored TakenOver. Kept so deserialization
    /// does not lose data; migrated to Converted on load.
    /// </summary>
    public bool TakenOver { get => Converted; set => Converted = value; }

    /// <summary>
    /// Base faith multiplier for this coven, sourced from the coven's
    /// WorldLocationDef.BaseMultiplier. Multiplies all faith generation
    /// (passive income, preaching, acolytes, map nodes). Defaults to 1.0
    /// for the home coven and for older saves that lack the field.
    /// </summary>
    public double BaseMultiplier { get; set; } = 1.0;

    public bool HasUpgrade(UpgradeId id) => Upgrades.Contains(id);

    /// <summary>
    /// Per-coven occult state. Each coven has its own acolytes, disciples,
    /// tech tree, map nodes, materials, and ley lines — so every
    /// coven is its own mini-game that must be managed independently.
    /// </summary>
    public OccultState Occult { get; set; } = new();
}
