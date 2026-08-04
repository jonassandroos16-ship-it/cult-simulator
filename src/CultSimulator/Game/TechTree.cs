namespace CultSimulator.Game;

public static class TechTree
{
    public static bool IsUnlocked(OccultState o, TechDef def) => o.UnlockedTechs.Contains(def.Id);

    public static bool PrerequisitesMet(OccultState o, TechDef def)
    {
        if (def.Prerequisites == null) return true;
        foreach (var prereq in def.Prerequisites) if (!o.UnlockedTechs.Contains(prereq)) return false;
        return true;
    }

    public static bool CanUnlock(GameState state, TechDef def) =>
        !IsUnlocked(state.Occult, def) && PrerequisitesMet(state.Occult, def) && state.ActiveCoven.Faith >= def.FaithCost;

    public static bool Unlock(GameState state, TechId id)
    {
        var def = OccultData.Tech(id);
        if (!CanUnlock(state, def)) return false;
        state.ActiveCoven.Faith -= def.FaithCost;
        state.Occult.UnlockedTechs.Add(id);
        return true;
    }

    public static IEnumerable<TechDef> AvailableTechs(OccultState o) => OccultData.Techs.Where(t => !IsUnlocked(o, t) && PrerequisitesMet(o, t));
    public static IEnumerable<TechDef> LockedTechs(OccultState o) => OccultData.Techs.Where(t => !IsUnlocked(o, t) && !PrerequisitesMet(o, t));
    public static bool HasTech(OccultState o, TechId id) => o.UnlockedTechs.Contains(id);

    public static double SetBonusMult(OccultState o) => HasTech(o, TechId.ResonanceMastery) ? 2.0 : 1.0;
    public static double FaithRetentionPercent(OccultState o) => HasTech(o, TechId.MemoriesOfTheDeep) ? 0.10 : 0.0;
    public static double EldritchFavorContinentMult(OccultState o) => HasTech(o, TechId.TheStarEatersFeast) ? 2.0 : 1.0;
}
