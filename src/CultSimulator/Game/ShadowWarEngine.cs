namespace CultSimulator.Game;

public static class ShadowWarEngine
{
    public const double BaseAgentProduction = 0.1;
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

    public static double FaithMultiplierBonus(ShadowWarState sw)
    {
        double bonus = 0;
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status != InstitutionStatus.Controlled) continue;
            var def = ShadowWarData.Institution(inst.Id);
            if (def != null && def.Type == InstitutionType.Finance)
                bonus += def.RewardValue;
        }
        return bonus;
    }

    public static double ReconRiskMultiplier(ShadowWarState sw)
    {
        double mult = 1.0;
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status != InstitutionStatus.Controlled) continue;
            var def = ShadowWarData.Institution(inst.Id);
            if (def != null && def.Type == InstitutionType.Intelligence)
                mult -= def.RewardValue;
        }
        return Math.Max(0.3, mult);
    }

    public static double DetectionMultiplier(ShadowWarState sw)
    {
        double mult = 1.0;
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status != InstitutionStatus.Controlled) continue;
            var def = ShadowWarData.Institution(inst.Id);
            if (def != null && def.Type == InstitutionType.Media)
                mult -= def.RewardValue;
        }
        return Math.Max(0.3, mult);
    }

    public static int CovensInContinent(GameState state, WorldLocationService locations, string continentId)
    {
        return state.Covens.Count(c => c.Converted &&
            string.Equals(locations.Find(c.Id)?.Continent, continentId, StringComparison.OrdinalIgnoreCase));
    }

    public static double CovenContinentBonus(int covenCount) => covenCount switch
    {
        >= 4 => 0.40,
        3 => 0.30,
        2 => 0.20,
        _ => 0
    };

    public static List<string> ControlledInstitutions(ShadowWarState sw)
        => sw.Institutions.Where(i => i.Status == InstitutionStatus.Controlled).Select(i => i.Id).ToList();

    public static void Tick(ShadowWarState sw, GameState state, WorldLocationService locations, double deltaSec)
    {
        sw.TotalAgents += AgentProductionPerSec(sw, state) * deltaSec;

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        foreach (var inst in sw.Institutions)
        {
            if (inst.Status == InstitutionStatus.Recon)
            {
                inst.ReconProgress += 100.0 * deltaSec / ReconBaseTime;
                if (inst.ReconProgress >= 100)
                {
                    inst.ReconProgress = 100;
                    inst.Status = InstitutionStatus.Infiltrating;
                }
            }

            if (inst.Status == InstitutionStatus.Alerted && now >= inst.CooldownUntil)
            {
                var def = ShadowWarData.Institution(inst.Id);
                inst.Status = def?.Prerequisites != null && def.Prerequisites.Length > 0
                    ? InstitutionStatus.Locked
                    : InstitutionStatus.Unlocked;
                inst.Detection = 0;
                inst.ReconProgress = 0;
                inst.DefenseRemaining = def?.Defense ?? 0;
            }

            if (inst.Status == InstitutionStatus.Investigated)
            {
                inst.InvestigationDefense = Math.Max(0, inst.InvestigationDefense - InvestigationDecayPerSec * deltaSec);
                if (inst.InvestigationDefense <= 0)
                {
                    inst.Status = InstitutionStatus.Alerted;
                    inst.CooldownUntil = now + (long)(AlertedCooldownSec * 1000);
                    inst.AssignedAgents = 0;
                }
            }
        }

        sw.TotalControlled = sw.Institutions.Count(i => i.Status == InstitutionStatus.Controlled);
        if (sw.TotalControlled >= ShadowWarData.Institutions.Length && !sw.VictoryAchieved)
            sw.VictoryAchieved = true;
    }

    public static (bool, string) StartRecon(ShadowWarState sw, GameState state, WorldLocationService locations, string institutionId, int agentCount)
    {
        var inst = sw.GetInstitution(institutionId);
        if (inst == null) return (false, "Institution not found.");
        if (inst.Status != InstitutionStatus.Unlocked) return (false, "Recon not available for this institution.");
        if (sw.AvailableAgents < agentCount) return (false, "Not enough available agents.");
        var def = ShadowWarData.Institution(institutionId);
        if (def == null) return (false, "Institution definition not found.");

        double risk = def.ReconRisk * ReconRiskMultiplier(sw);
        sw.DeployedAgents += agentCount;
        inst.AssignedAgents = agentCount;
        inst.Status = InstitutionStatus.Recon;
        inst.ReconProgress = 0;

        if (Random.Shared.NextDouble() < risk)
        {
            sw.TotalAgents -= agentCount;
            sw.DeployedAgents -= agentCount;
            inst.AssignedAgents = 0;
            inst.Status = InstitutionStatus.Alerted;
            inst.CooldownUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)(AlertedCooldownSec * 1000);
            return (false, $"Recon failed! {agentCount} agent(s) lost.");
        }
        return (true, "Recon started. Gathering intelligence...");
    }

    public static (bool, string) SendInfiltrationWave(ShadowWarState sw, GameState state, WorldLocationService locations, string institutionId, int waveSize)
    {
        var inst = sw.GetInstitution(institutionId);
        if (inst == null) return (false, "Institution not found.");
        if (inst.Status != InstitutionStatus.Recon && inst.Status != InstitutionStatus.Infiltrating)
            return (false, "Infiltration not available for this institution.");
        if (sw.AvailableAgents < waveSize) return (false, "Not enough available agents.");
        var def = ShadowWarData.Institution(institutionId);
        if (def == null) return (false, "Institution definition not found.");

        sw.DeployedAgents += waveSize;
        inst.AssignedAgents += waveSize;

        double damage = waveSize * AgentStrength(sw, state) * 10;
        inst.DefenseRemaining = Math.Max(0, inst.DefenseRemaining - damage);
        inst.Detection += def.DetectionRate * DetectionMultiplier(sw) * waveSize;

        if (inst.Detection >= 100)
        {
            sw.TotalAgents -= inst.AssignedAgents;
            sw.DeployedAgents -= inst.AssignedAgents;
            inst.AssignedAgents = 0;
            inst.Status = InstitutionStatus.Alerted;
            inst.CooldownUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)(AlertedCooldownSec * 1000);
            return (false, "Detected! All assigned agents lost.");
        }

        if (inst.DefenseRemaining <= 0)
        {
            sw.DeployedAgents -= inst.AssignedAgents;
            inst.AssignedAgents = 0;
            inst.Status = InstitutionStatus.Controlled;
            sw.Heat = Math.Max(0, sw.Heat - 10);
            sw.TotalControlled = sw.Institutions.Count(i => i.Status == InstitutionStatus.Controlled);
            return (true, $"{def.Name} is now under your control! {def.RewardLabel}.");
        }

        inst.Status = InstitutionStatus.Infiltrating;
        return (true, $"Wave sent. Defense reduced to {Math.Ceiling(inst.DefenseRemaining)}. Detection at {(int)inst.Detection}%.");
    }

    public static (bool, string) WithdrawAgents(ShadowWarState sw, string institutionId)
    {
        var inst = sw.GetInstitution(institutionId);
        if (inst == null) return (false, "Institution not found.");
        if (inst.AssignedAgents == 0) return (false, "No agents assigned.");
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
