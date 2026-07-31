namespace CultSimulator.Game;

public static class ShadowWarEngine
{
    public static double AgentRecruitCost(OccultState o, AgentDef def)
        => Math.Ceiling(OccultBalance.AgentCostBase * Math.Pow(OccultBalance.AgentCostGrowth, def.AgentCost - OccultBalance.AgentCostBase + AgentCount(o, def.Type)));

    public static int AgentCount(OccultState o, AgentType type)
        => OccultData.GetShadowAgent(o, type)?.Count ?? 0;

    public static bool CanRecruit(GameState state, AgentDef def)
    {
        var o = state.Occult;
        if (o.Agents < def.AgentCost) return false;
        if (AgentCount(o, def.Type) >= OccultBalance.MaxAgentsPerType) return false;
        return true;
    }

    public static bool Recruit(GameState state, AgentDef def)
    {
        if (!CanRecruit(state, def)) return false;
        var o = state.Occult;
        o.Agents -= def.AgentCost;
        var existing = OccultData.GetShadowAgent(o, def.Type);
        if (existing != null) existing.Count++;
        else o.ShadowAgents.Add(new ShadowAgent { Type = def.Type, Count = 1 });
        return true;
    }

    public static double TotalAgentFaithPerSec(OccultState o)
    {
        double total = 0;
        foreach (var sa in o.ShadowAgents)
        {
            var def = OccultData.Agent(sa.Type);
            total += def.FaithPerSec * sa.Count;
        }
        return total;
    }

    public static double TotalAgentSuspicionPerSec(OccultState o)
    {
        double total = 0;
        foreach (var sa in o.ShadowAgents)
        {
            var def = OccultData.Agent(sa.Type);
            total += def.SuspicionPerSec * sa.Count;
        }
        return total;
    }

    public static double TotalAgentArmyPowerPerSec(OccultState o)
    {
        double total = 0;
        foreach (var sa in o.ShadowAgents)
        {
            var def = OccultData.Agent(sa.Type);
            total += def.ArmyPowerPerSec * sa.Count;
        }
        return total;
    }

    public static int TotalAgentCount(OccultState o)
    {
        int total = 0;
        foreach (var sa in o.ShadowAgents) total += sa.Count;
        return total;
    }
}
