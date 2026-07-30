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

        double retainedFaithPercent = TechTree.FaithRetentionPercent(state.Occult);
        bool keepHighPriest = TechTree.HasTech(state.Occult, TechId.AstralAnchor);
        bool keepAstralAnchor = keepHighPriest;

        foreach (var coven in state.Covens)
        {
            if (!coven.TakenOver) continue;
            double retainedFaith = coven.Faith * retainedFaithPercent;
            var oldOccult = coven.Occult;
            coven.Occult = new OccultState
            {
                ArmyPower = 50,
                HighCouncil = keepHighPriest ? oldOccult.HighCouncil.Where(c => c.Role == CouncilRole.HighPriest).ToList() : new(),
                UnlockedTechs = keepAstralAnchor ? oldOccult.UnlockedTechs.Where(t => t == TechId.AstralAnchor).ToList() : new()
            };
            coven.Faith = retainedFaith;
            coven.Followers = 0;
            coven.Gold = 0;
            coven.Buildings = new Dictionary<BuildingType, int>();
            coven.Upgrades = new List<UpgradeId>();
        }

        state.ActiveCovenId = "skanor";
        state.EldritchFavor += favor;
        state.GrandSacrificeCount++;
        return favor;
    }

    public static double GlobalProductionMult(GameState state) => 1.0 + state.EldritchFavor * 0.02;
    public static double ClickPowerBase(GameState state) => 1.0 + state.EldritchFavor * 0.05;
}
