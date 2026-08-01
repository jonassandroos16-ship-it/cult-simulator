namespace CultSimulator.Game;

public static class ShadowWarEngine
{
    public const double BaseAgentProduction = 0.03;
    public const double BaseAgentStrength = 1.0;
    public const double ReconBaseTime = 10.0;
    public const double InfiltrationBaseTime = 5.0;
    public const double AlertedCooldownSec = 60.0;
    public const double InvestigationDecayPerSec = 0.5;

    public static ShadowWarState CreateInitialState()
    {
        var sw = new ShadowWarState();
        foreach (var def in ShadowWarData.Institutions)
        {
            sw.Institutions.Add(new InstitutionState
            {
                Id = def.Id,
                Status = def.Prerequisites == null || def.Prerequisites.Length == 0
                    ? InstitutionStatus.Unlocked
                    : InstitutionStatus.Locked,
                DefenseRemaining = def.Defense
            });
        }
        return sw;
    }

    public static ShadowWarState EnsureInitialized(GameState state)
    {
        state.ShadowWar ??= CreateInitialState();
        return state.ShadowWar;
    }

    public static double AgentProductionPerSec(ShadowWarState sw, GameState state)
    {
        double baseRate = BaseAgentProduction;
        double govBonus = 1.0;
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status != InstitutionStatus.Controlled) continue;
            var def = ShadowWarData.Institution(inst.Id);
            if (def != null && def.Type == InstitutionType.Government)
                govBonus += def.RewardValue;
        }
        return baseRate * govBonus * sw.PrestigeMultiplier;
    }

    public static double AgentStrength(ShadowWarState sw, GameState state)
    {
        double strength = BaseAgentStrength;
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status != InstitutionStatus.Controlled) continue;
            var def = ShadowWarData.Institution(inst.Id);
            if (def != null && (def.Type == InstitutionType.Military || def.Type == InstitutionType.Intelligence))
                strength += def.RewardValue;
        }
        if (state.Occult.UnlockedTechs.Contains(TechId.ShadowTactics))
            strength *= 1.25;
        return strength;
    }

    public static double FaithMultiplierBonus(ShadowWarState sw) => 0.0;

    public static double ReconRiskMultiplier(ShadowWarState sw)
    {
        double mult = 1.0;
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status != InstitutionStatus.Controlled) continue;
            var def = ShadowWarData.Institution(inst.Id);
            if (def != null && def.Type == InstitutionType.Intelligence)
                mult -= def.RewardValue * 0.1;
        }
        return Math.Max(0.1, mult);
    }

    public static double DetectionMultiplier(ShadowWarState sw)
    {
        double mult = 1.0;
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status != InstitutionStatus.Controlled) continue;
            var def = ShadowWarData.Institution(inst.Id);
            if (def != null && def.Type == InstitutionType.Media)
                mult -= def.RewardValue * 0.15;
        }
        return Math.Max(0.1, mult);
    }

    public static int CovensInContinent(GameState state, WorldLocationService locations, string continentId)
    {
        return state.Covens.Count(c => c.Converted &&
            string.Equals(locations.Find(c.Id)?.Continent, continentId, StringComparison.OrdinalIgnoreCase));
    }

    public static double CovenContinentBonus(int covenCount) => covenCount switch
    {
        1 => 1.0,
        2 => 1.2,
        3 => 1.5,
        >= 4 => 2.0,
        _ => 1.0
    };

    public static List<string> ControlledInstitutions(ShadowWarState sw)
        => sw.Institutions.Where(i => i.Status == InstitutionStatus.Controlled).Select(i => i.Id).ToList();

    public static void Tick(ShadowWarState sw, GameState state, WorldLocationService locations, double deltaSec)
    {
        double agentProd = AgentProductionPerSec(sw, state);
        sw.TotalAgents += agentProd * deltaSec;

        foreach (var inst in sw.Institutions)
        {
            var def = ShadowWarData.Institution(inst.Id);
            if (def == null) continue;

            if (inst.Status == InstitutionStatus.Locked)
            {
                bool prereqsMet = def.Prerequisites == null || def.Prerequisites.All(
                    prereqId => sw.GetInstitution(prereqId)?.Status == InstitutionStatus.Controlled);
                if (prereqsMet) inst.Status = InstitutionStatus.Unlocked;
            }

            if (inst.Status == InstitutionStatus.Alerted)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (now >= inst.CooldownUntil) inst.Status = InstitutionStatus.Unlocked;
            }

            if (inst.Status == InstitutionStatus.Recon)
            {
                double reconRate = AgentStrength(sw, state) * ReconRiskMultiplier(sw) * inst.AssignedAgents;
                inst.ReconProgress += reconRate * deltaSec;
                inst.Detection = Math.Min(1.0, inst.Detection + 0.001 * DetectionMultiplier(sw) * deltaSec);

                if (inst.ReconProgress >= def.Defense)
                {
                    inst.Status = InstitutionStatus.Infiltrating;
                    inst.ReconProgress = 0;
                }
            }

            if (inst.Status == InstitutionStatus.Infiltrating)
            {
                double infiltrateRate = AgentStrength(sw, state) * inst.AssignedAgents;
                inst.ControlProgress += infiltrateRate * deltaSec;
                inst.Detection = Math.Min(1.0, inst.Detection + 0.002 * DetectionMultiplier(sw) * deltaSec);

                if (inst.ControlProgress >= def.Defense)
                {
                    inst.Status = InstitutionStatus.Controlled;
                    inst.ControlProgress = 0;
                    sw.DeployedAgents -= inst.AssignedAgents;
                    inst.AssignedAgents = 0;
                    sw.TotalControlled++;
                    sw.PrestigeMultiplier = 1.0 + sw.TotalControlled * 0.05;
                }
            }

            if (inst.Status == InstitutionStatus.Investigated)
            {
                inst.InvestigationDefense = Math.Max(0, inst.InvestigationDefense - InvestigationDecayPerSec * deltaSec);
                if (inst.InvestigationDefense <= 0)
                {
                    inst.Status = InstitutionStatus.Unlocked;
                    inst.InvestigationDefense = 0;
                }
            }
        }
    }

    public static (bool, string) StartRecon(ShadowWarState sw, GameState state, WorldLocationService locations, string institutionId, int agentCount)
    {
        var inst = sw.GetInstitution(institutionId);
        if (inst == null) return (false, "Institution not found.");
        if (inst.Status != InstitutionStatus.Unlocked) return (false, "Institution is not available for recon.");
        if (sw.AvailableAgents < agentCount) return (false, "Not enough available agents.");

        sw.DeployedAgents += agentCount;
        inst.AssignedAgents = agentCount;
        inst.Status = InstitutionStatus.Recon;
        inst.ReconProgress = 0;
        return (true, $"Recon started on {institutionId} with {agentCount} agents.");
    }

    public static (bool, string) SendInfiltrationWave(ShadowWarState sw, GameState state, WorldLocationService locations, string institutionId, int waveSize)
    {
        var inst = sw.GetInstitution(institutionId);
        if (inst == null) return (false, "Institution not found.");
        if (inst.Status != InstitutionStatus.Infiltrating && inst.Status != InstitutionStatus.Recon)
            return (false, "Institution is not in an infiltratable state.");
        if (sw.AvailableAgents < waveSize) return (false, "Not enough available agents.");

        sw.DeployedAgents += waveSize;
        inst.AssignedAgents += waveSize;
        return (true, $"Infiltration wave of {waveSize} agents sent to {institutionId}.");
    }

    public static (bool, string) WithdrawAgents(ShadowWarState sw, string institutionId)
    {
        var inst = sw.GetInstitution(institutionId);
        if (inst == null) return (false, "Institution not found.");
        if (inst.AssignedAgents == 0) return (false, "No agents assigned here.");

        sw.DeployedAgents -= inst.AssignedAgents;
        inst.AssignedAgents = 0;
        if (inst.Status == InstitutionStatus.Recon || inst.Status == InstitutionStatus.Infiltrating)
            inst.Status = InstitutionStatus.Unlocked;
        return (true, "Agents withdrawn.");
    }

    public static (bool, string) AssignDefenders(ShadowWarState sw, string institutionId, int count)
    {
        var inst = sw.GetInstitution(institutionId);
        if (inst == null) return (false, "Institution not found.");
        if (inst.Status != InstitutionStatus.Investigated) return (false, "This institution is not under investigation.");
        if (sw.AvailableAgents < count) return (false, "Not enough available agents.");

        sw.DeployedAgents += count;
        inst.AssignedAgents += count;
        inst.InvestigationDefense = Math.Min(100, inst.InvestigationDefense + count * 10);
        return (true, $"Defenders assigned. Investigation defense raised to {(int)inst.InvestigationDefense}%.");
    }
}
