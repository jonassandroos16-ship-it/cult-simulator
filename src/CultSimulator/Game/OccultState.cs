using System.Text.Json.Serialization;

namespace CultSimulator.Game;

/// <summary>
/// A promoted Tier 1 minion with a randomized trait.
/// </summary>
public class Minion
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public PromotedRole Role { get; set; }
    public string TraitId { get; set; } = "";
    public string Name { get; set; } = "";

    [JsonIgnore]
    public MinionTraitDef? Trait => OccultData.Trait(TraitId);
}

/// <summary>
/// A High Council member granting a global aura buff.
/// </summary>
public class CouncilMember
{
    public CouncilRole Role { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>
/// A conquered map node with a current stance and timer state.
/// </summary>
public class MapNodeState
{
    public string NodeId { get; set; } = "";
    public bool Conquered { get; set; }
    public NodeStance Stance { get; set; } = NodeStance.Harvest;
    public double RaidTimer { get; set; }
}

/// <summary>
/// Root occult state — lives inside GameState as a single nested object so
/// serialization stays clean and migration is straightforward.
/// </summary>
public class OccultState
{
    // Tier 1: Devotion (replaces Faith for occult actions; Faith still used for legacy)
    public double Devotion { get; set; }
    public double LifetimeDevotion { get; set; }
    public double ClickPower { get; set; } = 1.0;
    public int ClickPowerLevel { get; set; }

    // Tier 2: Forbidden Knowledge
    public double ForbiddenKnowledge { get; set; }

    // Tier 3: Eldritch Favor (prestige)
    public double EldritchFavor { get; set; }
    public int GrandSacrificeCount { get; set; }

    // Cultist hierarchy
    public int Acolytes { get; set; }
    public List<Minion> Minions { get; set; } = new();
    public List<CouncilMember> HighCouncil { get; set; } = new();

    // Tech tree
    public List<TechId> UnlockedTechs { get; set; } = new();

    // Grimoire
    public List<string> SocketedArtifacts { get; set; } = new();
    public List<string> OwnedArtifacts { get; set; } = new();

    // World map
    public List<MapNodeState> MapNodes { get; set; } = new();
    public double Suspicion { get; set; }
    public double ArmyPower { get; set; }

    // Cauldron
    public Dictionary<MaterialKind, int> Materials { get; set; } = new();

    // Active effects
    public double FrenzyTimer { get; set; }
    public double MassHysteriaTimer { get; set; }
    public double ElixirTapMult { get; set; } = 1.0;
    public double ElixirFkMult { get; set; } = 1.0;
    public double ElixirSuspicionMult { get; set; } = 1.0;
    public double ElixirTimer { get; set; }

    // Ley lines / Great Seals
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
