using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class RivalBattleState
{
    public string RivalId { get; set; } = "";
    public string ContinentId { get; set; } = "";
    public RivalBattlePhase Phase { get; set; }
    public RivalBattleStatus Status { get; set; }
    public double RivalHp { get; set; }
    public double RivalMaxHp { get; set; }
    public double PlayerHp { get; set; }
    public double PlayerMaxHp { get; set; }
    public List<DeployedAgent> DeployedSquad { get; set; } = new();
    public long LastTickAt { get; set; }
    public List<string> Log { get; set; } = new();
    public List<EnemyUnitSlot> EnemyUnits { get; set; } = new();
    public List<BattleRound> RecentRounds { get; set; } = new();
    public int RoundNumber { get; set; }
    public double RoundTimer { get; set; }
    public double Momentum { get; set; }
    public RivalCultArchetype? EnemyArchetype { get; set; }

    [JsonIgnore]
    public int TotalDeployed => DeployedSquad.Sum(d => d.Count);
}
