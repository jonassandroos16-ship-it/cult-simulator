namespace CultSimulator.Game;

public static class ShadowWarEngine
{
    public const double BaseAgentProduction = 0.03;
    public const double BaseAgentStrength = 1.0;

    public static ShadowWarState CreateInitialState() => new();

    public static ShadowWarState EnsureInitialized(GameState state)
    {
        state.ShadowWar ??= CreateInitialState();
        return state.ShadowWar;
    }

    public static double AgentProductionPerSec(ShadowWarState sw, GameState state)
    {
        double baseRate = BaseAgentProduction;
        double zealotMult = CultistHierarchy.AgentProductionMult(state.Occult);
        double buildingMult = 1.0;
        foreach (var coven in state.Covens)
        {
            if (!coven.TakenOver) continue;
            int guilds = coven.Buildings.GetValueOrDefault(BuildingType.ShadowGuild);
            buildingMult += guilds * GameBalance.ShadowGuildAgentSpeedBonus;
        }
        return baseRate * sw.PrestigeMultiplier * zealotMult * buildingMult * GrandSacrifice.GlobalProductionMult(state);
    }

    public static int AgentPoolCap(GameState state)
    {
        int cap = GameBalance.AgentPoolBaseCap;
        foreach (var coven in state.Covens)
        {
            if (!coven.TakenOver) continue;
            int safehouses = coven.Buildings.GetValueOrDefault(BuildingType.Safehouse);
            cap += (int)(safehouses * GameBalance.SafehouseAgentCapBonus);
        }
        return cap;
    }

    public static double AgentStrength(ShadowWarState sw, GameState state)
    {
        double strength = BaseAgentStrength;
        if (state.Occult.UnlockedTechs.Contains(TechId.ShadowTactics))
            strength *= 1.25;
        return strength;
    }

    public static double FaithMultiplierBonus(ShadowWarState sw) => 0.0;

    public static void Tick(ShadowWarState sw, GameState state, WorldLocationService locations, double deltaSec)
    {
        double agentProd = AgentProductionPerSec(sw, state);
        int cap = AgentPoolCap(state);
        double effectiveCap = cap + sw.DeployedAgents + sw.SpentAgents;
        double potentialNew = sw.TotalAgents + agentProd * deltaSec;
        sw.TotalAgents = Math.Min(potentialNew, effectiveCap);
    }
}
