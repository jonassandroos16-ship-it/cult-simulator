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

    public bool HasUpgrade(UpgradeId id) => Upgrades.Contains(id);
}
