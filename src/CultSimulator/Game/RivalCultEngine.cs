using System.Linq;

namespace CultSimulator.Game;

public static class RivalCultEngine
{
    public static RivalCultSystemState CreateInitialState()
    {
        var state = new RivalCultSystemState { Rivals = new() };
        foreach (var def in RivalCultData.Rivals)
        {
            state.Rivals.Add(new RivalCultState
            {
                Id = def.Id,
                Status = RivalCultStatus.Dormant,
                Power = 0,
                TerritoryControl = 0,
                NextActionAt = 0
            });
        }
        return state;
    }

    public static RivalCultSystemState EnsureInitialized(GameState state)
    {
        if (state.RivalCults == null)
            state.RivalCults = CreateInitialState();
        return state.RivalCults;
    }

    public static bool ShouldActivateForContinent(GameState state, WorldLocationService locations, string continentId)
    {
        int count = state.Covens.Count(c => c.Converted &&
            string.Equals(locations.Find(c.Id)?.Continent, continentId, StringComparison.OrdinalIgnoreCase));
        return count >= 2;
    }

    public static void ActivateForContinent(GameState state, string continentId)
    {
        var rs = EnsureInitialized(state);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var rival in rs.Rivals)
        {
            var def = RivalCultData.Find(rival.Id);
            if (def == null || def.PreferredTerritoryId != continentId) continue;
            if (rival.Status != RivalCultStatus.Dormant) continue;

            rival.Status = RivalCultStatus.Active;
            rival.Power = 20;
            rival.NextActionAt = now + (long)(Random.Shared.Next(30, 60) * 1000);
        }
    }

    public static void Tick(GameState state, WorldLocationService locations, double deltaSec)
    {
        var rs = EnsureInitialized(state);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var def in RivalCultData.Rivals)
        {
            var rival = rs.GetRival(def.Id);
            if (rival == null) continue;

            bool shouldActive = ShouldActivateForContinent(state, locations, def.PreferredTerritoryId);

            if (shouldActive && rival.Status == RivalCultStatus.Dormant)
            {
                ActivateForContinent(state, def.PreferredTerritoryId);
            }
            else if (!shouldActive && rival.Status != RivalCultStatus.Dormant)
            {
                rival.Status = RivalCultStatus.Dormant;
                rival.Power = 0;
                rival.ControlledInstitutions.Clear();
                rival.TerritoryControl = 0;
            }

            if (rival.Status == RivalCultStatus.Dormant) continue;

            rival.Power += def.GrowthRate * deltaSec * (1.0 + rival.TerritoryControl * 0.5);

            if (now >= rival.NextActionAt)
            {
                TakeAction(state, rival, def, now);
                rival.NextActionAt = now + (long)(Random.Shared.Next(20, 45) * 1000);
            }
        }
    }

    private static void TakeAction(GameState state, RivalCultState rival, RivalCultDef def, long now)
    {
        var sw = state.ShadowWarOrInit;

        var targetInst = FindTargetInstitution(sw, def, rival);
        if (targetInst != null)
        {
            AttackInstitution(sw, rival, def, targetInst);
            return;
        }

        rival.TerritoryControl = Math.Min(1.0, rival.TerritoryControl + 0.05);
        rival.Status = rival.TerritoryControl > 0.5 ? RivalCultStatus.Expanding : RivalCultStatus.Active;
    }

    private static InstitutionState? FindTargetInstitution(ShadowWarState sw, RivalCultDef def, RivalCultState rival)
    {
        var territoryInsts = ShadowWarData.InstitutionsForTerritory(def.PreferredTerritoryId);

        var playerControlled = territoryInsts
            .FirstOrDefault(i => sw.GetInstitution(i.Id)?.Status == InstitutionStatus.Controlled);
        if (playerControlled != null && Random.Shared.NextDouble() < def.Aggression)
        {
            rival.Status = RivalCultStatus.AtWar;
            return sw.GetInstitution(playerControlled.Id);
        }

        var unlocked = territoryInsts
            .Where(i => sw.GetInstitution(i.Id)?.Status == InstitutionStatus.Unlocked)
            .ToList();
        if (unlocked.Count > 0)
            return sw.GetInstitution(unlocked[Random.Shared.Next(unlocked.Count)].Id);

        return null;
    }

    private static void AttackInstitution(ShadowWarState sw, RivalCultState rival, RivalCultDef def, InstitutionState inst)
    {
        if (inst.Status == InstitutionStatus.Controlled)
        {
            double damage = rival.Power * 0.1 * def.AgentStrength;
            inst.InvestigationDefense = Math.Max(0, inst.InvestigationDefense - damage);
            if (inst.InvestigationDefense <= 0 && inst.Status == InstitutionStatus.Controlled)
            {
                inst.Status = InstitutionStatus.Investigated;
                inst.InvestigationDefense = 30;
                rival.ControlledInstitutions.Add(inst.Id);
            }
            rival.Power -= 5;
        }
        else if (inst.Status == InstitutionStatus.Unlocked)
        {
            if (Random.Shared.NextDouble() < 0.3 * def.AgentStrength)
            {
                inst.Status = InstitutionStatus.Alerted;
                inst.CooldownUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 20_000;
                rival.ControlledInstitutions.Add(inst.Id);
                rival.TerritoryControl = Math.Min(1.0, rival.TerritoryControl + 0.1);
            }
        }
    }

    public static int TotalRivalControlled(GameState state)
    {
        var rs = EnsureInitialized(state);
        return rs.Rivals.Where(r => r.Status != RivalCultStatus.Dormant).Sum(r => r.ControlledInstitutions.Count);
    }

    public static IReadOnlyList<(RivalCultDef def, RivalCultState state)> ActiveRivals(GameState state)
    {
        var rs = EnsureInitialized(state);
        return rs.Rivals
            .Where(r => r.Status != RivalCultStatus.Dormant)
            .Select(r => (RivalCultData.Find(r.Id)!, r))
            .ToList();
    }
}
