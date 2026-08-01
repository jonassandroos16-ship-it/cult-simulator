using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class RivalBattleState
{
    public string RivalId { get; set; } = "";
    public string ContinentId { get; set; } = "";
    public RivalBattlePhase Phase { get; set; } = RivalBattlePhase.Available;
    public double RivalHp { get; set; }
    public double RivalMaxHp { get; set; }
    public double PlayerHp { get; set; }
    public double PlayerMaxHp { get; set; }
    public List<DeployedAgent> DeployedSquad { get; set; } = new();
    public long LastTickAt { get; set; }
    public List<string> Log { get; set; } = new();

    [JsonIgnore]
    public int TotalDeployed => DeployedSquad.Sum(d => d.Count);
}

public enum RivalBattlePhase
{
    /// <summary>Rival is active and can be attacked.</summary>
    Available,
    /// <summary>Player is deploying agents.</summary>
    Deploy,
    /// <summary>Battle in progress.</summary>
    Fighting,
    /// <summary>Battle won — rival destroyed.</summary>
    Victory,
    /// <summary>Battle lost — can retry.</summary>
    Defeat
}
