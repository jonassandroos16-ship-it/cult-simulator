namespace CultSimulator.Game;

/// <summary>
/// Tech tree logic for the Codex Necrotica. Handles unlock checks,
/// prerequisites, and purchasing techs with Forbidden Knowledge.
/// Pure functions over <see cref="OccultState"/>.
/// </summary>
public static class TechTree
{
    public static bool IsUnlocked(OccultState o, TechDef def) =>
        o.UnlockedTechs.Contains(def.Id);

    public static bool PrerequisitesMet(OccultState o, TechDef def)
    {
        if (def.Prerequisites == null) return true;
        foreach (var prereq in def.Prerequisites)
            if (!o.UnlockedTechs.Contains(prereq)) return false;
        return true;
    }

    public static bool CanUnlock(OccultState o, TechDef def) =>
        !IsUnlocked(o, def) && PrerequisitesMet(o, def) && o.ForbiddenKnowledge >= def.FkCost;

    public static bool Unlock(OccultState o, TechId id)
    {
        var def = OccultData.Tech(id);
        if (!CanUnlock(o, def)) return false;
        o.ForbiddenKnowledge -= def.FkCost;
        o.UnlockedTechs.Add(id);
        return true;
    }

    public static IEnumerable<TechDef> AvailableTechs(OccultState o) =>
        OccultData.Techs.Where(t => !IsUnlocked(o, t) && PrerequisitesMet(o, t));

    public static IEnumerable<TechDef> LockedTechs(OccultState o) =>
        OccultData.Techs.Where(t => !IsUnlocked(o, t) && !PrerequisitesMet(o, t));

    public static bool HasTech(OccultState o, TechId id) => o.UnlockedTechs.Contains(id);

    public static double SuspicionReductionMult(OccultState o)
    {
        double mult = 1.0;
        if (HasTech(o, TechId.WhispersInTheDark)) mult *= 0.85;
        return mult;
    }

    public static double SetBonusMult(OccultState o)
    {
        return HasTech(o, TechId.ResonanceMastery) ? 2.0 : 1.0;
    }

    public static double FkRetentionPercent(OccultState o)
    {
        return HasTech(o, TechId.MemoriesOfTheDeep) ? 0.10 : 0.0;
    }

    public static double EldritchFavorContinentMult(OccultState o)
    {
        return HasTech(o, TechId.TheStarEatersFeast) ? 2.0 : 1.0;
    }
}