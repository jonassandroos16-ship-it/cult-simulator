using System.Linq;

namespace CultSimulator.Game;

/// <summary>
/// Shared battle calculations used by BattleEngine, LocalCultBattleEngine, and RivalCultEngine.
/// Eliminates duplicated attack/defense/stealth logic across all three battle systems.
/// </summary>
public static class BattleCommon
{
    public static double CalculateAttack(List<DeployedAgent> squad, ShadowWarState sw, GameState state)
    {
        double strength = ShadowWarEngine.AgentStrength(sw, state);
        strength *= state.Occult.ElixirWarStrengthMult;
        double attack = 0;
        foreach (var slot in squad)
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def != null) attack += def.Attack * slot.Count * strength;
        }
        return attack;
    }

    public static double CalculateDefense(List<DeployedAgent> squad)
    {
        double defense = 0;
        foreach (var slot in squad)
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def != null) defense += def.Defense * slot.Count;
        }
        return defense;
    }

    public static double CalculateStealth(List<DeployedAgent> squad)
    {
        int total = squad.Sum(d => d.Count);
        if (total == 0) return 0;
        double stealth = 0;
        foreach (var slot in squad)
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def != null) stealth += def.Stealth * slot.Count;
        }
        return stealth / total;
    }

    public static double CalculateFaithRegen(List<DeployedAgent> squad)
    {
        double regen = 0;
        foreach (var slot in squad)
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def != null && def.Type == AgentType.Scholar)
                regen += def.FaithRegen * slot.Count;
        }
        return regen;
    }

    public static (double rivalDamage, double playerDamage) ExchangeDamage(
        List<DeployedAgent> squad, double rivalAttack, ShadowWarState sw, GameState state, double deltaSec)
    {
        double attack = CalculateAttack(squad, sw, state);
        double defense = CalculateDefense(squad);
        double stealth = CalculateStealth(squad);

        double rivalDamage = rivalAttack * (1.0 - stealth * 0.3) * deltaSec;
        double playerDamage = attack * deltaSec;
        double mitigated = Math.Max(0, rivalDamage - defense * 0.1 * deltaSec);

        return (mitigated, playerDamage);
    }

    public static void AppendLog(List<string> log, string message, int maxEntries)
    {
        log.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
        if (log.Count > maxEntries) log.RemoveAt(0);
    }
}
