using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public record TerritoryLossEvent(
    string ContinentId,
    string RivalName,
    long OccurredAt);

public class DeployedAgent
{
    public AgentType Type { get; set; } = AgentType.Initiate;
    public int Count { get; set; }
}

public class BattleState
{
    public string ContinentId { get; set; } = "";
    public BattlePhase Phase { get; set; } = BattlePhase.NoThreat;
    public double RivalHp { get; set; }
    public double RivalMaxHp { get; set; }
    public double PlayerHp { get; set; }
    public double PlayerMaxHp { get; set; }
    public List<DeployedAgent> DeployedSquad { get; set; } = new();
    public long CooldownUntil { get; set; }
    public long LastTickAt { get; set; }
    public BattleStatus Status { get; set; } = BattleStatus.NotStarted;
    public List<string> Log { get; set; } = new();
    public List<TerritoryLossEvent> RecentLosses { get; set; } = new();
    public List<EnemyUnitSlot> EnemyUnits { get; set; } = new();
    public List<BattleRound> RecentRounds { get; set; } = new();
    public int RoundNumber { get; set; }
    public double RoundTimer { get; set; }
    public double Momentum { get; set; }
    public RivalCultArchetype? EnemyArchetype { get; set; }

    [JsonIgnore]
    public int TotalDeployed => DeployedSquad.Sum(d => d.Count);
}

public class BattleSystemState
{
    public List<BattleState> Battles { get; set; } = new();
    public long LastEventAt { get; set; }

    public BattleState? GetBattle(string continentId) =>
        Battles.FirstOrDefault(b => b.ContinentId == continentId);
}
