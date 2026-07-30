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
    public List<InstitutionState> Institutions { get; set; } = new();
    public bool VictoryAchieved { get; set; }
    public double PrestigeMultiplier { get; set; } = 1.0;
    public int TotalControlled { get; set; }

    [JsonIgnore]
    public double AvailableAgents => Math.Floor(TotalAgents - DeployedAgents);

    public InstitutionState? GetInstitution(string id) =>
        Institutions.FirstOrDefault(i => i.Id == id);
}
