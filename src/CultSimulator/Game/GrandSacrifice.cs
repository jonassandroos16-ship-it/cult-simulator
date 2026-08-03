namespace CultSimulator.Game;

public static class GrandSacrifice
{
    public static double CalculateFavor(GameState state)
    {
        double totalLifetime = state.Covens.Where(c => c.TakenOver).Sum(c => c.Occult.LifetimeFaith);
        if (totalLifetime < OccultBalance.FavorDivisor) return 0;
        double baseFavor = Math.Sqrt(totalLifetime / OccultBalance.FavorDivisor);
        return Math.Floor(baseFavor * ContinentMultiplier(state) * TechTree.EldritchFavorContinentMult(state.Occult));
    }

    public static double ContinentMultiplier(GameState state) => 1.0 + WorldMapSystem.ConqueredNodeCount(state) * 0.1;
    public static bool CanSacrifice(GameState state) => CalculateFavor(state) >= 1;

    public static double PerformSacrifice(GameState state)
    {
        double favor = CalculateFavor(state);
        if (favor < 1) return 0;

        double retainedFaithPercent = TechTree.FaithRetentionPercentDeep(state.Occult);
        bool keepHighPriest = TechTree.HasTech(state.Occult, TechId.AstralAnchor);
        bool keepAstralAnchor = keepHighPriest;
        bool keepAllTechs = TechTree.KeepAllTechs(state.Occult);

        var homeCoven = state.HomeCoven;
        double homeRetainedFaith = homeCoven.Faith * retainedFaithPercent;
        var oldHomeOccult = homeCoven.Occult;

        state.Covens.Clear();
        state.Covens.Add(new CovenState
        {
            Id = "skanor",
            TakenOver = true,
            Converted = true,
            BaseMultiplier = 1.0,
            Faith = homeRetainedFaith,
            Occult = new OccultState
            {
                HighCouncil = keepHighPriest ? oldHomeOccult.HighCouncil.Where(c => c.Role == CouncilRole.HighPriest).ToList() : new(),
                UnlockedTechs = keepAllTechs ? oldHomeOccult.UnlockedTechs.ToList() : (keepAstralAnchor ? oldHomeOccult.UnlockedTechs.Where(t => t == TechId.AstralAnchor).ToList() : new())
            }
        });

        state.ActiveCovenId = "skanor";
        state.RevealedFootholds.Clear();
        state.PendingContinentStory = null;
        state.Conversion = null;
        state.ActiveLocalCults.Clear();
        state.LocalCultBattles?.Clear();

        state.ShadowWar = ShadowWarEngine.CreateInitialState();
        state.RivalCults = RivalCultEngine.CreateInitialState();
        state.BattleSystem = BattleEngine.CreateInitialState();

        state.EldritchFavor += favor;
        state.GrandSacrificeCount++;
        return favor;
    }

    public static double GlobalProductionMult(GameState state) => 1.0 + state.EldritchFavor * 0.02;
    public static double ClickPowerBase(GameState state) => 1.0 + state.EldritchFavor * 0.05;
}
