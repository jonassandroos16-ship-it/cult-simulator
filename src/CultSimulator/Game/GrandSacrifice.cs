namespace CultSimulator.Game;

public static class GrandSacrifice
{
    public static double CalculateFavor(GameState state)
    {
        var o = state.Occult;
        if (o.LifetimeFaith < OccultBalance.FavorDivisor) return 0;
        double baseFavor = Math.Sqrt(o.LifetimeFaith / OccultBalance.FavorDivisor);
        return Math.Floor(baseFavor * ContinentMultiplier(state) * TechTree.EldritchFavorContinentMult(o));
    }

    public static double ContinentMultiplier(GameState state) => 1.0 + WorldMapSystem.ConqueredNodeCount(state) * 0.1;
    public static bool CanSacrifice(GameState state) => CalculateFavor(state) >= 1;

    public static double PerformSacrifice(GameState state)
    {
        double favor = CalculateFavor(state);
        if (favor < 1) return 0;
        var o = state.Occult;
        double retainedFaith = state.ActiveCoven.Faith * TechTree.FaithRetentionPercent(o);
        bool keepHighPriest = TechTree.HasTech(o, TechId.AstralAnchor);
        state.Occult = new OccultState
        {
            EldritchFavor = o.EldritchFavor + favor,
            HighCouncil = keepHighPriest ? o.HighCouncil.Where(c => c.Role == CouncilRole.HighPriest).ToList() : new(),
            UnlockedTechs = keepHighPriest ? o.UnlockedTechs.Where(t => t == TechId.AstralAnchor).ToList() : new()
        };
        state.Covens = new List<CovenState> { new CovenState { Id = "skanor", TakenOver = true, Faith = retainedFaith } };
        state.ActiveCovenId = "skanor";
        state.Occult.GrandSacrificeCount = o.GrandSacrificeCount + 1;
        return favor;
    }

    public static double GlobalProductionMult(GameState state) => 1.0 + state.Occult.EldritchFavor * 0.02;
    public static double ClickPowerBase(GameState state) => 1.0 + state.Occult.EldritchFavor * 0.05;
}
