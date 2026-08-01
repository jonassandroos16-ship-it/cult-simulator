using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class InstitutionState
{
    public string Id { get; set; } = "";
    public InstitutionStatus Status { get; set; } = InstitutionStatus.Locked;
    public double DefenseRemaining { get; set; }
    public double Detection { get; set; }
    public int AssignedAgents { get; set; }
    public double ReconProgress { get; set; }
    public double ControlProgress { get; set; }
    public double InvestigationDefense { get; set; }
    public long CooldownUntil { get; set; }
}

public class ShadowWarState
{
    public double Heat { get; set; }
    public double TotalAgents { get; set; }
    public double DeployedAgents { get; set; }
    public double SpentAgents { get; set; }
    public List<InstitutionState> Institutions { get; set; } = new();
    public bool VictoryAchieved { get; set; }
    public double PrestigeMultiplier { get; set; } = 1.0;
    public int TotalControlled { get; set; }

    /// <summary>Recruited battle agents available to deploy, keyed by AgentType enum value.</summary>
    public Dictionary<AgentType, int> RecruitedAgents { get; set; } = new();

    [JsonIgnore]
    public double AvailableAgents => Math.Floor(TotalAgents - DeployedAgents - SpentAgents);

    [JsonIgnore]
    public List<InstitutionState> ControlledInstitutions =>
        Institutions.Where(i => i.Status == InstitutionStatus.Controlled).ToList();

    public InstitutionState? GetInstitution(string id) =>
        Institutions.FirstOrDefault(i => i.Id == id);
}
