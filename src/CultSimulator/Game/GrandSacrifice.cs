namespace CultSimulator.Game;

/// <summary>
/// Grand Sacrifice (prestige) logic: calculating Eldritch Favor yield,
/// resetting the run, and applying meta-buffs.
/// Pure functions over <see cref="GameState"/>.
/// </summary>
public static class GrandSacrifice
{
    public static double CalculateFavor(GameState state)
    {
        var o = state.Occult;
        if (o.LifetimeDevotion < OccultBalance.FavorDivisor) return 0;
        double baseFavor = Math.Sqrt(o.LifetimeDevotion / OccultBalance.FavorDivisor);
        double continentMult = ContinentMultiplier(state);
        double techMult = TechTree.EldritchFavorContinentMult(o);
        return Math.Floor(baseFavor * continentMult * techMult);
    }

    public static double ContinentMultiplier(GameState state)
    {
        int conquered = WorldMapSystem.ConqueredNodeCount(state.Occult);
        return 1.0 + conquered * 0.1;
    }

    public static bool CanSacrifice(GameState state) =>
        CalculateFavor(state) >= 1;

    public static double PerformSacrifice(GameState state)
    {
        double favor = CalculateFavor(state);
        if (favor < 1) return 0;

        var o = state.Occult;
        double retainedFk = o.ForbiddenKnowledge * TechTree.FkRetentionPercent(o);
        bool keepHighPriest = TechTree.HasTech(o, TechId.AstralAnchor);

        state.Occult = new OccultState
        {
            EldritchFavor = o.EldritchFavor + favor,
            ForbiddenKnowledge = retainedFk,
            HighCouncil = keepHighPriest
                ? o.HighCouncil.Where(c => c.Role == CouncilRole.HighPriest).ToList()
                : new List<CouncilMember>(),
            UnlockedTechs = keepHighPriest
                ? o.UnlockedTechs.Where(t => t == TechId.AstralAnchor).ToList()
                : new List<TechId>()
        };

        state.Covens = new List<CovenState>
        {
            new CovenState { Id = "skanor", TakenOver = true }
        };
        state.ActiveCovenId = "skanor";

        return favor;
    }

    public static double GlobalProductionMult(GameState state) =>
        1.0 + state.Occult.EldritchFavor * 0.02;

    public static double ClickPowerBase(GameState state) =>
        1.0 + state.Occult.EldritchFavor * 0.05;
}