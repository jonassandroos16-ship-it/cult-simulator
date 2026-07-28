namespace CultSimulator.Game;

/// <summary>
/// Per-coven mutable state. The home coven (Id == "skanor") is the player's
/// own coven; others are rival covens that can be taken over.
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
    public bool TakenOver { get; set; }

    public bool HasUpgrade(UpgradeId id) => Upgrades.Contains(id);
}
