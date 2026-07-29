using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class Minion
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public PromotedRole Role { get; set; }
    public string TraitId { get; set; } = "";
    public string Name { get; set; } = "";

    [JsonIgnore]
    public MinionTraitDef? Trait => OccultData.Trait(TraitId);
}

public class CovenMember
{
    public CouncilRole Role { get; set; }
    public string Name { get; set; } = "";
    public string MinionId { get; set; } = "";
    public PromotedRole OriginalRole { get; set; }
    public string TraitId { get; set; } = "";
    [JsonIgnore]
    public MinionTraitDef? Trait => OccultData.Trait(TraitId);
}

public class MapNodeState
{
    public string NodeId { get; set; } = "";
    public bool Conquered { get; set; }
    public NodeStance Stance { get; set; } = NodeStance.Harvest;
    public double RaidTimer { get; set; }
}

public class OccultState
{
    public double LifetimeFaith { get; set; }
    public int SermonPowerLevel { get; set; }
    public double EldritchFavor { get; set; }
    public int GrandSacrificeCount { get; set; }
    public int Acolytes { get; set; }
    public List<Minion> Minions { get; set; } = new();
    public List<CovenMember> HighCouncil { get; set; } = new();
    public List<TechId> UnlockedTechs { get; set; } = new();
    public List<string> SocketedArtifacts { get; set; } = new();
    public List<string> OwnedArtifacts { get; set; } = new();
    public List<MapNodeState> MapNodes { get; set; } = new();
    public double Suspicion { get; set; }
    public double ArmyPower { get; set; }
    public Dictionary<MaterialKind, int> Materials { get; set; } = new();
    public double FrenzyTimer { get; set; }
    public double MassHysteriaTimer { get; set; }
    public double ElixirTapMult { get; set; } = 1.0;
    public double ElixirFaithMult { get; set; } = 1.0;
    public double ElixirSuspicionMult { get; set; } = 1.0;
    public double ElixirTimer { get; set; }
    public List<string[]> LeyLines { get; set; } = new();

    [JsonIgnore]
    public bool IsFrenzyActive => FrenzyTimer > 0;

    [JsonIgnore]
    public bool IsMassHysteriaActive => MassHysteriaTimer > 0;

    [JsonIgnore]
    public int UnlockedSocketCount
    {
        get
        {
            int count = OccultBalance.BaseSockets;
            if (UnlockedTechs.Contains(TechId.SecondSocket)) count++;
            if (UnlockedTechs.Contains(TechId.ThirdSocket)) count++;
            return Math.Min(count, OccultBalance.MaxSockets);
        }
    }
}
