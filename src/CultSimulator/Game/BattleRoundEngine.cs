using System.Linq;

namespace CultSimulator.Game;

public static class BattleRoundEngine
{
    public const double RoundIntervalSec = 2.5;

    public static EnemyRecon BuildRecon(
        string name, string icon, string description,
        double rivalHp, double rivalAttack,
        RivalCultArchetype? archetype, double scale, double rivalPower)
    {
        var threat = ClassifyThreat(rivalHp, rivalAttack);
        List<EnemyUnitSlot> composition = archetype != null
            ? EnemyCompositionBuilder.BuildComposition(archetype.Value, scale, rivalPower)
            : EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, scale, rivalHp / 50);

        return new EnemyRecon
        {
            Name = name,
            Icon = icon,
            Description = description,
            EstimatedHp = rivalHp,
            EstimatedAttack = rivalAttack,
            Threat = threat,
            Composition = composition,
            Recommendation = BuildRecommendation(threat, composition)
        };
    }

    public static ThreatLevel ClassifyThreat(double hp, double attack)
    {
        double score = hp * attack;
        if (score < 500) return ThreatLevel.Low;
        if (score < 3000) return ThreatLevel.Moderate;
        if (score < 15000) return ThreatLevel.High;
        return ThreatLevel.Extreme;
    }

    public static string BuildRecommendation(ThreatLevel threat, List<EnemyUnitSlot> composition)
    {
        int hasSupport = composition.Count(u => u.IsSupport);
        int hasHighStealth = composition.Count(u => u.Stealth > 2.0);
        int hasHighAttack = composition.Count(u => u.Attack > 8.0);

        var parts = new List<string>();
        parts.Add(threat switch
        {
            ThreatLevel.Low => "Light resistance — Initiates and a few Zealots should suffice.",
            ThreatLevel.Moderate => "Moderate force recommended — bring Zealots and Infiltrators.",
            ThreatLevel.High => "Strong enemy — field Mages (with Scholar support) and Infiltrators for stealth.",
            ThreatLevel.Extreme => "Overwhelming force — maximize Mages, Infiltrators, and bring Scholars for healing.",
            _ => "Assess your forces carefully."
        });

        if (hasSupport > 0)
            parts.Add("Enemy has healers — focus fire to break their sustain.");
        if (hasHighStealth > 0)
            parts.Add("Enemy uses stealth — your Infiltrators can counter their evasion.");
        if (hasHighAttack > 0)
            parts.Add("Enemy has high-damage casters — bring defense or overwhelming offense.");

        return string.Join(" ", parts);
    }

    public static BattleRound ExecuteRound(
        int roundNumber,
        List<DeployedAgent> playerSquad,
        List<EnemyUnitSlot> enemyUnits,
        double playerAttack,
        double enemyAttack,
        double playerDefense,
        double stealth,
        double deltaSec)
    {
        var round = new BattleRound { RoundNumber = roundNumber };

        var playerAttacks = GeneratePlayerAttacks(playerSquad, playerAttack, enemyUnits, deltaSec);
        round.PlayerAttacks = playerAttacks;
        round.PlayerDamageTotal = playerAttacks.Sum(a => a.Damage);

        var enemyAttacks = GenerateEnemyAttacks(enemyUnits, enemyAttack, playerSquad, stealth, deltaSec);
        round.EnemyAttacks = enemyAttacks;
        round.EnemyDamageTotal = enemyAttacks.Sum(a => a.Damage) * Math.Max(0, 1.0 - stealth * 0.3);

        double mitigatedEnemy = Math.Max(0, round.EnemyDamageTotal - playerDefense * 0.1 * deltaSec);
        round.EnemyDamageTotal = mitigatedEnemy;

        round.MomentumShift = round.PlayerDamageTotal - round.EnemyDamageTotal;

        round.Summary = BuildRoundSummary(roundNumber, playerAttacks, enemyAttacks);
        return round;
    }

    private static List<UnitAttack> GeneratePlayerAttacks(
        List<DeployedAgent> squad, double totalAttack, List<EnemyUnitSlot> enemyUnits, double deltaSec)
    {
        var attacks = new List<UnitAttack>();
        var activeEnemies = enemyUnits.Where(u => u.Count > 0).ToList();
        if (activeEnemies.Count == 0) return attacks;

        double squadTotalAttack = Math.Max(1, squad.Sum(s => BattleData.AgentDef(s.Type)?.Attack * s.Count ?? 0));
        foreach (var slot in squad.Where(s => s.Count > 0))
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def == null) continue;

            double unitAttack = def.Attack * slot.Count * (totalAttack / squadTotalAttack) * deltaSec;
            if (unitAttack <= 0 && def.Attack > 0)
                unitAttack = def.Attack * slot.Count * deltaSec;

            var target = activeEnemies[Random.Shared.Next(activeEnemies.Count)];
            attacks.Add(new UnitAttack
            {
                UnitName = def.Name,
                Icon = def.Icon,
                Count = slot.Count,
                Damage = Math.Round(unitAttack, 1),
                TargetName = target.Name
            });
        }
        return attacks;
    }

    private static List<UnitAttack> GenerateEnemyAttacks(
        List<EnemyUnitSlot> enemies, double totalAttack, List<DeployedAgent> playerSquad, double playerStealth, double deltaSec)
    {
        var attacks = new List<UnitAttack>();
        var activePlayer = playerSquad.Where(s => s.Count > 0).ToList();
        if (activePlayer.Count == 0) return attacks;

        foreach (var unit in enemies.Where(u => u.Count > 0))
        {
            if (unit.IsSupport) continue;

            double unitAttack = unit.Attack * unit.Count * deltaSec;
            var target = activePlayer[Random.Shared.Next(activePlayer.Count)];
            var targetDef = BattleData.AgentDef(target.Type);
            attacks.Add(new UnitAttack
            {
                UnitName = unit.Name,
                Icon = unit.Icon,
                Count = unit.Count,
                Damage = Math.Round(unitAttack, 1),
                TargetName = targetDef?.Name ?? "your forces"
            });
        }
        return attacks;
    }

    private static string BuildRoundSummary(int round, List<UnitAttack> playerAttacks, List<UnitAttack> enemyAttacks)
    {
        var parts = new List<string> { $"Round {round}:" };

        if (playerAttacks.Count > 0)
        {
            var topAtk = playerAttacks.OrderByDescending(a => a.Damage).First();
            parts.Add($"Your {topAtk.Count} {topAtk.UnitName} strike {topAtk.TargetName} for {topAtk.Damage:F0} dmg.");
        }

        if (enemyAttacks.Count > 0)
        {
            var topEnemy = enemyAttacks.OrderByDescending(a => a.Damage).First();
            parts.Add($"Enemy {topEnemy.Count} {topEnemy.UnitName} counter {topEnemy.TargetName} for {topEnemy.Damage:F0} dmg.");
        }

        return string.Join(" ", parts);
    }

    public static (bool reinforced, string? action) TryEnemyReinforce(
        List<EnemyUnitSlot> enemyUnits, RivalCultArchetype archetype, double scale, int roundNumber)
    {
        if (roundNumber <= 0 || roundNumber % 3 != 0) return (false, null);
        if (enemyUnits.Count == 0) return (false, null);

        var target = enemyUnits[Random.Shared.Next(enemyUnits.Count)];
        int reinforceCount = Math.Max(1, (int)(2 * scale));
        target.Count += reinforceCount;
        target.MaxCount = Math.Max(target.MaxCount, target.Count);

        return (true, $"Enemy reinforces {target.Name} with +{reinforceCount} units!");
    }

    public static string? TryEnemyTactic(RivalCultArchetype archetype, List<DeployedAgent> playerSquad, int roundNumber)
    {
        if (roundNumber <= 0 || roundNumber % 4 != 0) return null;

        int playerMages = playerSquad.FirstOrDefault(s => s.Type == AgentType.Mage)?.Count ?? 0;
        int playerZealots = playerSquad.FirstOrDefault(s => s.Type == AgentType.Zealot)?.Count ?? 0;

        return archetype switch
        {
            RivalCultArchetype.TheOrderOfTheDawn when playerMages > 0 => "Enemy shifts to anti-caster formation!",
            RivalCultArchetype.TheCrimsonConclave when playerZealots > 2 => "Enemy commits reserves to counter your Zealots!",
            RivalCultArchetype.TheSilentChoir => "Enemy disperses into shadow — reducing your accuracy!",
            RivalCultArchetype.TheObsidianCircle => "Enemy raises defensive wards!",
            _ => null
        };
    }
}
