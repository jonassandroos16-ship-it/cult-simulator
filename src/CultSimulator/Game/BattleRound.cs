namespace CultSimulator.Game;

public class EnemyUnitSlot
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Count { get; set; }
    public int MaxCount { get; set; }
    public double Attack { get; set; }
    public double Defense { get; set; }
    public double Stealth { get; set; }
    public bool IsSupport { get; set; }
    public double FaithRegen { get; set; }
    public string RoleKey { get; set; } = "";
}

public class UnitAttack
{
    public string UnitName { get; set; } = "";
    public string Icon { get; set; } = "";
    public int Count { get; set; }
    public double Damage { get; set; }
    public string TargetName { get; set; } = "";
}

public class BattleRound
{
    public int RoundNumber { get; set; }
    public List<UnitAttack> PlayerAttacks { get; set; } = new();
    public List<UnitAttack> EnemyAttacks { get; set; } = new();
    public string Summary { get; set; } = "";
    public double PlayerDamageTotal { get; set; }
    public double EnemyDamageTotal { get; set; }
    public double MomentumShift { get; set; }
    public bool EnemyReinforced { get; set; }
    public string? EnemyAction { get; set; }
}

public enum ThreatLevel { Low, Moderate, High, Extreme }

public class EnemyRecon
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public double EstimatedHp { get; set; }
    public double EstimatedAttack { get; set; }
    public ThreatLevel Threat { get; set; }
    public List<EnemyUnitSlot> Composition { get; set; } = new();
    public string Recommendation { get; set; } = "";
}
