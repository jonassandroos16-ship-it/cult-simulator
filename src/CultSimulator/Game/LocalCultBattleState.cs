using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public enum LocalCultBattlePhase { Deploy, Fighting, Victory }
public enum LocalCultBattleStatus { NotStarted, Active, Victory, Defeat }

public class LocalCultBattleState
{
    public string CultId { get; set; } = "";
    public string RivalName { get; set; } = "";
    public LocalCultBattlePhase Phase { get; set; } = LocalCultBattlePhase.Deploy;
    public LocalCultBattleStatus Status { get; set; } = LocalCultBattleStatus.NotStarted;
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
