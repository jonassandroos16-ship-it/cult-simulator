using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class RivalCultState
{
    public string Id { get; set; } = "";
    public RivalCultStatus Status { get; set; } = RivalCultStatus.Dormant;
    public double Power { get; set; }
    public double TerritoryControl { get; set; }
    public List<string> ControlledInstitutions { get; set; } = new();
    public long NextActionAt { get; set; }
}

public class RivalCultSystemState
{
    public List<RivalCultState> Rivals { get; set; } = new();
    public long ActivatedAt { get; set; }
    public bool IsActive { get; set; }

    public RivalCultState? GetRival(string id) =>
        Rivals.FirstOrDefault(r => r.Id == id);
}
