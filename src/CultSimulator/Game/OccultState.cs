namespace CultSimulator.Game;

public class OccultState
{
    public int Initiates { get; set; }
    public int SermonPowerLevel { get; set; }
    public double LifetimeFaith { get; set; }
    public int GrandSacrificeCount { get; set; }
    public double EldritchFavor { get; set; }
    public List<string> OwnedArtifacts { get; set; } = new();
    public List<string> SocketedArtifacts { get; set; } = new();
    public int UnlockedSocketCount { get; set; } = OccultBalance.BaseSockets;
    public List<TechId> UnlockedTechs { get; set; } = new();
    public List<Minion> Minions { get; set; } = new();
    public List<CouncilSeat> HighCouncil { get; set; } = new();
    public List<MapNodeState> MapNodes { get; set; } = new();
    public Dictionary<MaterialKind, int> Materials { get; set; } = new();
    public double FrenzyTimer { get; set; }
    public double MassHysteriaTimer { get; set; }
    public double DarkVigilTimer { get; set; }
    public double WhisperChoirTimer { get; set; }
    public double CovenBlessingTimer { get; set; }
    public double ElixirTimer { get; set; }
    public double ElixirTapMult { get; set; } = 1.0;
    public double ElixirFaithMult { get; set; } = 1.0;
    public double ElixirPreachMult { get; set; } = 1.0;
    public double ElixirWarStrengthMult { get; set; } = 1.0;

    public bool IsFrenzyActive => FrenzyTimer > 0;
    public bool IsMassHysteriaActive => MassHysteriaTimer > 0;
    public bool IsDarkVigilActive => DarkVigilTimer > 0;
    public bool IsWhisperChoirActive => WhisperChoirTimer > 0;
    public bool IsCovenBlessingActive => CovenBlessingTimer > 0;
}
