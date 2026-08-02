using Xunit;
using CultSimulator.Game;

namespace CultSimulator.Tests;

public class BattleRoundTests
{
    private static List<DeployedAgent> MakeSquad(params (AgentType, int)[] units) =>
        units.Select(u => new DeployedAgent { Type = u.Item1, Count = u.Item2 }).ToList();

    [Fact]
    public void BuildRecon_ReturnsValidIntel()
    {
        var recon = BattleRoundEngine.BuildRecon(
            "Test Enemy", "⚔️", "Test description",
            500, 10.0, RivalCultArchetype.TheOrderOfTheDawn, 1.5, 30);

        Assert.Equal("Test Enemy", recon.Name);
        Assert.Equal(500, recon.EstimatedHp);
        Assert.Equal(10.0, recon.EstimatedAttack);
        Assert.True(recon.Composition.Count > 0);
        Assert.NotEmpty(recon.Recommendation);
    }

    [Fact]
    public void ClassifyThreat_ReturnsCorrectLevels()
    {
        Assert.Equal(ThreatLevel.Low, BattleRoundEngine.ClassifyThreat(100, 3));
        Assert.Equal(ThreatLevel.Moderate, BattleRoundEngine.ClassifyThreat(500, 4));
        Assert.Equal(ThreatLevel.High, BattleRoundEngine.ClassifyThreat(1000, 10));
        Assert.Equal(ThreatLevel.Extreme, BattleRoundEngine.ClassifyThreat(5000, 20));
    }

    [Fact]
    public void EnemyComposition_HasUnitsForEachArchetype()
    {
        foreach (var archetype in Enum.GetValues<RivalCultArchetype>())
        {
            var comp = EnemyCompositionBuilder.BuildComposition(archetype, 1.0, 20);
            Assert.True(comp.Count > 0, $"{archetype} should produce enemy units");
            Assert.True(comp.All(u => u.Count > 0));
        }
    }

    [Fact]
    public void EnemyComposition_ScalesWithContinent()
    {
        var lowScale = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 1.0, 20);
        var highScale = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 4.0, 20);
        var lowTotal = lowScale.Sum(u => u.Count);
        var highTotal = highScale.Sum(u => u.Count);
        Assert.True(highTotal > lowTotal, "Higher scale should produce more enemy units");
    }

    [Fact]
    public void ExecuteRound_GeneratesAttacksAndSummary()
    {
        var squad = MakeSquad((AgentType.Zealot, 3), (AgentType.Scholar, 1));
        var enemies = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 1.0, 20);

        var round = BattleRoundEngine.ExecuteRound(1, squad, enemies, 30, 15, 10, 1.0, 2.5);

        Assert.Equal(1, round.RoundNumber);
        Assert.True(round.PlayerAttacks.Count > 0);
        Assert.NotEmpty(round.Summary);
        Assert.Contains("Round 1", round.Summary);
    }

    [Fact]
    public void ExecuteRound_PlayerDamageIsPositive_WhenSquadDeployed()
    {
        var squad = MakeSquad((AgentType.Zealot, 5));
        var enemies = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 1.0, 20);

        var round = BattleRoundEngine.ExecuteRound(1, squad, enemies, 50, 10, 15, 0.5, 2.5);

        Assert.True(round.PlayerDamageTotal > 0);
    }

    [Fact]
    public void ExecuteRound_EnemyAttacks_TargetPlayerUnits()
    {
        var squad = MakeSquad((AgentType.Initiate, 10), (AgentType.Zealot, 2));
        var enemies = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheCrimsonConclave, 2.0, 30);

        var round = BattleRoundEngine.ExecuteRound(1, squad, enemies, 40, 20, 12, 1.0, 2.5);

        Assert.True(round.EnemyAttacks.Count > 0);
        Assert.All(round.EnemyAttacks, a => Assert.NotEmpty(a.TargetName));
    }

    [Fact]
    public void TryEnemyReinforce_TriggersEveryThirdRound()
    {
        var enemies = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 1.0, 20);
        var initialCount = enemies.Sum(u => u.Count);

        var (reinforced, _) = BattleRoundEngine.TryEnemyReinforce(enemies, RivalCultArchetype.TheOrderOfTheDawn, 1.0, 3);
        Assert.True(reinforced);
        Assert.True(enemies.Sum(u => u.Count) > initialCount);
    }

    [Fact]
    public void TryEnemyReinforce_DoesNotTriggerOnOtherRounds()
    {
        var enemies = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 1.0, 20);

        var (reinforced, _) = BattleRoundEngine.TryEnemyReinforce(enemies, RivalCultArchetype.TheOrderOfTheDawn, 1.0, 2);
        Assert.False(reinforced);

        (reinforced, _) = BattleRoundEngine.TryEnemyReinforce(enemies, RivalCultArchetype.TheOrderOfTheDawn, 1.0, 5);
        Assert.False(reinforced);
    }

    [Fact]
    public void TryEnemyTactic_ReturnsNullOnNonTriggerRounds()
    {
        var squad = MakeSquad((AgentType.Mage, 2));
        Assert.Null(BattleRoundEngine.TryEnemyTactic(RivalCultArchetype.TheOrderOfTheDawn, squad, 1));
    }

    [Fact]
    public void TryEnemyTactic_ReturnsActionOnFourthRound()
    {
        var squad = MakeSquad((AgentType.Mage, 2));
        var tactic = BattleRoundEngine.TryEnemyTactic(RivalCultArchetype.TheOrderOfTheDawn, squad, 4);
        Assert.NotNull(tactic);
    }

    [Fact]
    public void BuildRecommendation_IncludesThreatSpecificAdvice()
    {
        var comp = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheObsidianCircle, 3.0, 50);
        var rec = BattleRoundEngine.BuildRecommendation(ThreatLevel.Extreme, comp);

        Assert.Contains("Overwhelming force", rec);
    }

    [Fact]
    public void EnemyComposition_TotalAttackScalesWithScale()
    {
        var lowComp = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 1.0, 20);
        var highComp = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheOrderOfTheDawn, 4.0, 20);

        Assert.True(
            EnemyCompositionBuilder.TotalAttack(highComp) > EnemyCompositionBuilder.TotalAttack(lowComp),
            "Higher scale enemies should have higher total attack");
    }

    [Fact]
    public void ExecuteRound_MomentumShift_ReflectsDamageDifference()
    {
        var strongSquad = MakeSquad((AgentType.Mage, 5), (AgentType.Scholar, 5));
        var weakEnemies = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheSilentChoir, 0.5, 10);

        var round = BattleRoundEngine.ExecuteRound(1, strongSquad, weakEnemies, 200, 5, 20, 2.0, 2.5);

        Assert.True(round.MomentumShift > 0, "Winning round should have positive momentum");
    }
}
