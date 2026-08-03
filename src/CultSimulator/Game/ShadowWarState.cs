using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class ShadowWarState
{
    public double TotalAgents { get; set; }
    public double DeployedAgents { get; set; }
    public double SpentAgents { get; set; }
    public bool VictoryAchieved { get; set; }
    public double PrestigeMultiplier { get; set; } = 1.0;
    public int TotalControlled { get; set; }

    /// <summary>Recruited battle agents available to deploy, keyed by AgentType enum value.</summary>
    public Dictionary<AgentType, int> RecruitedAgents { get; set; } = new();

    [JsonIgnore]
    public double AvailableAgents => Math.Floor(TotalAgents - DeployedAgents - SpentAgents);
}
