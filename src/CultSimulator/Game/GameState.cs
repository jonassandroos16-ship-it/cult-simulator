using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class GameState
{
    public string CultName { get; set; } = "";
    public int Followers { get; set; }
    public double Faith { get; set; }
    public double Gold { get; set; }
    public int PreachCount { get; set; }
    public Dictionary<BuildingType, int> Buildings { get; set; } = new();
    public List<UpgradeId> Upgrades { get; set; } = new();
    public long StartedAt { get; set; }

    [JsonIgnore]
    public string? ActiveEventId { get; set; }

    public bool HasUpgrade(UpgradeId id) => Upgrades.Contains(id);
}
