namespace CultSimulator.Game;

public static class ShadowWarEngine
{
    public static ShadowWarState CreateInitialState()
    {
        var state = new ShadowWarState();
        foreach (var def in ShadowWarData.Institutions)
        {
            state.Institutions.Add(new InstitutionState
            {
                Id = def.Id,
                Status = def.Prerequisites == null || def.Prerequisites.Length == 0
                    ? InstitutionStatus.Unlocked
                    : InstitutionStatus.Locked,
                DefenseRemaining = def.Defense
            });
        }
        return state;
    }

    // ── Computed bonuses from controlled institutions ──

    public static IReadOnlyList<InstitutionDef> ControlledInstitutions(ShadowWarState sw) =>
        ShadowWarData.Institutions.Where(d => sw.GetInstitution(d.Id)?.Status == InstitutionStatus.Controlled).ToList();

    public static bool IsTerritoryControlled(ShadowWarState sw, string territoryId)
    {
        var t = ShadowWarData.Territory(territoryId);
        if (t == null) return false;
        return t.InstitutionIds.All(id => sw.GetInstitution(id)?.Status == InstitutionStatus.Controlled);
    }

    public static IReadOnlyList<TerritoryDef> ControlledTerritories(ShadowWarState sw) =>
        ShadowWarData.Territories.Where(t => IsTerritoryControlled(sw, t.Id)).ToList();

    public static bool IsAllTerritoriesControlled(ShadowWarState sw) =>
        ShadowWarData.Territories.All(t => IsTerritoryControlled(sw, t.Id));

    public static double SuspicionDecay(ShadowWarState sw) =>
        ControlledInstitutions(sw).Where(i => i.Type == InstitutionType.Police).Sum(i => i.RewardValue);

    public static double DetectionMultiplier(ShadowWarState sw)
    {
        var reduction = ControlledInstitutions(sw).Where(i => i.Type == InstitutionType.Media).Sum(i => i.RewardValue);
        return Math.Max(0.3, 1.0 - reduction);
    }

    public static double ReconRiskMultiplier(ShadowWarState sw)
    {
        var reduction = ControlledInstitutions(sw).Where(i => i.Type == InstitutionType.Intelligence).Sum(i => i.RewardValue);
        return Math.Max(0.3, 1.0 - reduction);
    }

    public static double AgentStrength(ShadowWarState sw, GameState state)
    {
        double baseStrength = 1.0;
        var militaryBonus = ControlledInstitutions(sw).Where(i => i.Type == InstitutionType.Military).Sum(i => i.RewardValue);
        baseStrength *= 1.0 + militaryBonus;
        var armyPower = state.Covens.Where(c => c.Converted).Sum(c => c.Occult.ArmyPower);
        baseStrength *= 1.0 + armyPower * 0.001;
        return baseStrength;
    }

    public static double AgentProductionPerSec(ShadowWarState sw, GameState state)
    {
        double total = 0;
        foreach (var coven in state.Covens)
        {
            if (!coven.Converted) continue;
            total += coven.Occult.Acolytes * 0.02;
        }
        var govBonus = ControlledInstitutions(sw).Where(i => i.Type == InstitutionType.Government).Sum(i => i.RewardValue);
        total *= 1.0 + govBonus;
        var territories = ControlledTerritories(sw);
        double agentMult = territories.Aggregate(1.0, (m, t) => m * t.AgentMultiplier);
        total *= agentMult;
        return total;
    }

    public static double FaithMultiplierBonus(ShadowWarState sw) =>
        ControlledInstitutions(sw).Where(i => i.Type == InstitutionType.Finance).Sum(i => i.RewardValue);

    // ── Actions ──

    public static (bool success, string message) StartRecon(ShadowWarState sw, GameState state, string institutionId, int agentCount)
    {
        var inst = sw.GetInstitution(institutionId);
        var def = ShadowWarData.Institution(institutionId);
        if (inst == null || def == null) return (false, "Unknown institution.");
        if (inst.Status != InstitutionStatus.Unlocked) return (false, "Cannot recon this institution now.");
        if (agentCount < 1 || agentCount > 3) return (false, "Send 1-3 agents for recon.");
        if (sw.AvailableAgents < agentCount) return (false, "Not enough available agents.");

        // Recon risk: chance to lose agents
        var risk = def.ReconRisk * ReconRiskMultiplier(sw);
        if (Random.Shared.NextDouble() < risk)
        {
            sw.TotalAgents -= agentCount;
            sw.Heat += 5 + agentCount * 2;
            return (false, $"Recon team detected! Lost {agentCount} agents. Heat +{5 + agentCount * 2}.");
        }

        sw.TotalAgents -= agentCount;
        inst.Status = InstitutionStatus.Recon;
        inst.AssignedAgents = agentCount;
        inst.ReconProgress = 0;
        return (true, $"Recon team of {agentCount} deployed to {def.Name}.");
    }

    public static (bool success, string message) SendInfiltrationWave(ShadowWarState sw, GameState state, string institutionId, int waveSize)
    {
        var inst = sw.GetInstitution(institutionId);
        var def = ShadowWarData.Institution(institutionId);
        if (inst == null || def == null) return (false, "Unknown institution.");
        if (inst.Status != InstitutionStatus.Recon && inst.Status != InstitutionStatus.Infiltrating)
            return (false, "Must recon before infiltrating.");
        if (waveSize < 1) return (false, "Wave must have at least 1 agent.");
        if (sw.AvailableAgents < waveSize) return (false, "Not enough available agents.");

        sw.TotalAgents -= waveSize;
        inst.AssignedAgents += waveSize;
        inst.Status = InstitutionStatus.Infiltrating;
        return (true, $"Wave of {waveSize} agents sent to {def.Name}.");
    }

    public static (bool success, string message) WithdrawAgents(ShadowWarState sw, string institutionId)
    {
        var inst = sw.GetInstitution(institutionId);
        var def = ShadowWarData.Institution(institutionId);
        if (inst == null || def == null) return (false, "Unknown institution.");
        if (inst.AssignedAgents <= 0) return (false, "No agents deployed here.");
        if (inst.Status == InstitutionStatus.Controlled) return (false, "Institution is controlled.");

        sw.TotalAgents += inst.AssignedAgents;
        inst.AssignedAgents = 0;
        inst.Status = InstitutionStatus.Unlocked;
        inst.ReconProgress = 0;
        inst.Detection = Math.Max(0, inst.Detection - 20);
        return (true, $"Agents withdrawn from {def.Name}.");
    }

    public static (bool success, string message) AssignDefenders(ShadowWarState sw, string institutionId, int count)
    {
        var inst = sw.GetInstitution(institutionId);
        var def = ShadowWarData.Institution(institutionId);
        if (inst == null || def == null) return (false, "Unknown institution.");
        if (inst.Status != InstitutionStatus.Investigated) return (false, "Not under investigation.");
        if (sw.AvailableAgents < count) return (false, "Not enough available agents.");

        sw.TotalAgents -= count;
        inst.AssignedAgents += count;
        return (true, $"{count} agents assigned to defend {def.Name}.");
    }

    // ── Tick ──

    public static void Tick(ShadowWarState sw, GameState state, double deltaSec)
    {
        if (sw.VictoryAchieved) return;

        sw.TotalAgents += AgentProductionPerSec(sw, state) * deltaSec;
        sw.Heat = Math.Max(0, sw.Heat - SuspicionDecay(sw) * deltaSec);

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var def in ShadowWarData.Institutions)
        {
            var inst = sw.GetInstitution(def.Id);
            if (inst == null) continue;
            ProcessInstitution(sw, inst, def, state, deltaSec, now);
        }

        UpdateLocks(sw);
        MaybeTriggerInvestigation(sw, now);

        if (IsAllTerritoriesControlled(sw) && !sw.VictoryAchieved)
        {
            sw.VictoryAchieved = true;
            sw.PrestigeMultiplier = 5.0;
        }

        sw.TotalControlled = ControlledInstitutions(sw).Count;
    }

    private static void ProcessInstitution(ShadowWarState sw, InstitutionState inst, InstitutionDef def, GameState state, double deltaSec, long now)
    {
        switch (inst.Status)
        {
            case InstitutionStatus.Recon:
            {
                inst.ReconProgress += 15 * deltaSec * inst.AssignedAgents;
                if (inst.ReconProgress >= 100)
                {
                    inst.ReconProgress = 100;
                    inst.Status = InstitutionStatus.Infiltrating;
                }
                break;
            }
            case InstitutionStatus.Infiltrating:
            {
                double strength = AgentStrength(sw, state);
                double damage = inst.AssignedAgents * strength * 2 * deltaSec;
                inst.DefenseRemaining = Math.Max(0, inst.DefenseRemaining - damage);
                inst.ControlProgress = ((def.Defense - inst.DefenseRemaining) / def.Defense) * 100;

                double detectionGain = def.DetectionRate * inst.AssignedAgents * deltaSec * DetectionMultiplier(sw);
                inst.Detection = Math.Min(100, inst.Detection + detectionGain);

                if (inst.Detection >= 100)
                {
                    int lost = inst.AssignedAgents;
                    inst.AssignedAgents = 0;
                    inst.Detection = 100;
                    inst.Status = InstitutionStatus.Alerted;
                    inst.CooldownUntil = now + 30_000;
                    sw.Heat += 20 + lost * 5;
                    break;
                }

                if (inst.DefenseRemaining <= 0)
                {
                    inst.Status = InstitutionStatus.Controlled;
                    inst.Detection = 0;
                    inst.AssignedAgents = 0;
                    inst.ControlProgress = 100;
                    inst.InvestigationDefense = 0;
                }
                break;
            }
            case InstitutionStatus.Alerted:
            {
                inst.Detection = Math.Max(0, inst.Detection - 5 * deltaSec);
                if (now >= inst.CooldownUntil)
                {
                    inst.Status = InstitutionStatus.Unlocked;
                    inst.Detection = 0;
                    inst.DefenseRemaining = def.Defense;
                    inst.ControlProgress = 0;
                }
                break;
            }
            case InstitutionStatus.Investigated:
            {
                double defend = inst.AssignedAgents * 3 * deltaSec;
                double decay = 5 * deltaSec;
                inst.InvestigationDefense = Math.Max(0, inst.InvestigationDefense + defend - decay);

                if (inst.InvestigationDefense <= 0)
                {
                    inst.Status = InstitutionStatus.Unlocked;
                    inst.AssignedAgents = 0;
                    inst.DefenseRemaining = def.Defense;
                    inst.Detection = 0;
                    inst.ControlProgress = 0;
                    sw.Heat += 15;
                }
                else if (inst.InvestigationDefense >= 100)
                {
                    inst.Status = InstitutionStatus.Controlled;
                    inst.AssignedAgents = 0;
                    inst.InvestigationDefense = 0;
                }
                break;
            }
        }
    }

    private static void UpdateLocks(ShadowWarState sw)
    {
        foreach (var def in ShadowWarData.Institutions)
        {
            var inst = sw.GetInstitution(def.Id);
            if (inst == null || inst.Status != InstitutionStatus.Locked) continue;
            if (def.Prerequisites == null || def.Prerequisites.Length == 0)
            {
                inst.Status = InstitutionStatus.Unlocked;
                continue;
            }
            bool allMet = def.Prerequisites.All(pid => sw.GetInstitution(pid)?.Status == InstitutionStatus.Controlled);
            if (allMet) inst.Status = InstitutionStatus.Unlocked;
        }
    }

    private static long _nextInvestigationTime;

    private static void MaybeTriggerInvestigation(ShadowWarState sw, long now)
    {
        if (now < _nextInvestigationTime) return;
        var controlled = ControlledInstitutions(sw);
        if (controlled.Count == 0)
        {
            _nextInvestigationTime = now + 60_000;
            return;
        }
        double chance = Math.Min(0.8, 0.15 + controlled.Count * 0.03);
        if (Random.Shared.NextDouble() < chance)
        {
            var target = controlled[Random.Shared.Next(controlled.Count)];
            var inst = sw.GetInstitution(target.Id);
            if (inst != null && inst.Status == InstitutionStatus.Controlled)
            {
                inst.Status = InstitutionStatus.Investigated;
                inst.InvestigationDefense = 50;
                inst.AssignedAgents = 0;
            }
        }
        long interval = Math.Max(15_000, 60_000 - controlled.Count * 2000);
        _nextInvestigationTime = now + interval;
    }
}