namespace CultSimulator.Game;

public static class GrandSacrifice
{
    public static double CalculateFavor(GameState state)
    {
        double totalLifetime = state.TotalLifetimeFaith;
        if (totalLifetime < OccultBalance.FavorDivisor) return 0;
        double baseFavor = Math.Sqrt(totalLifetime / OccultBalance.FavorDivisor);
        return Math.Floor(baseFavor * ContinentMultiplier(state) * TechTree.EldritchFavorContinentMult(state.Occult));
    }

    public static double ContinentMultiplier(GameState state)
    {
        int continents = state.Covens.Count(c => c.TakenOver && c.Id != "skanor");
        return 1.0 + continents * 0.15;
    }

    public static double GlobalProductionMult(GameState state)
    {
        return 1.0 + state.EldritchFavor * 0.02;
    }

    public static double ClickPowerBase(GameState state)
    {
        return 1.0 + state.EldritchFavor * 0.1;
    }

    public static bool CanSacrifice(GameState state) => CalculateFavor(state) >= 1;

    public static double FavorDivisor => OccultBalance.FavorDivisor;

    public static double PerformSacrifice(GameState state)
    {
        double favor = CalculateFavor(state);
        if (favor < 1) return 0;

        double retainedFaith = TechTree.RetainedFaithOnSacrifice(state.Occult);

        state.EldritchFavor += favor;
        state.GrandSacrificeCount++;
        state.TotalLifetimeFaith = 0;

        var newCovens = new List<CovenState>
        {
            new CovenState
            {
                Id = "skanor",
                TakenOver = true,
                Converted = true,
                BaseMultiplier = 1.0,
                Faith = retainedFaith,
                Occult = new OccultState { GrandSacrificeCount = state.GrandSacrificeCount }
            }
        };

        state.Covens = newCovens;
        state.ActiveCovenId = "skanor";
        state.ShadowWar = ShadowWarEngine.CreateInitialState();
        state.BattleSystem = BattleEngine.CreateInitialState();
        state.RivalCults = RivalCultEngine.CreateInitialState();
        state.LocalCultBattles = new List<LocalCultBattleState>();
        state.ActiveLocalCults = new List<LocalCultInstance>();
        state.RevealedFootholds = new List<string>();
        state.Conversion = null;
        state.PendingContinentStory = null;

        return favor;
    }
}
