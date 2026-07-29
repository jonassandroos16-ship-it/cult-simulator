namespace CultSimulator.Game;

public static class OccultEngine
{
    public static double ClickPower(GameState state)
    {
        var o = state.Occult;
        double basePower = GrandSacrifice.ClickPowerBase(state) + o.SermonPowerLevel;
        double mult = CultistHierarchy.TapPowerMult(o) * Grimoire.TapPowerBonus(o) * o.ElixirTapMult;
        if (o.IsFrenzyActive) mult *= OccultBalance.FrenzyMultiplier;
        return basePower * mult;
    }

    public static double Tap(GameState state)
    {
        var o = state.Occult;
        double power = ClickPower(state);
        if (Grimoire.BloodVoidConversionActive(o))
            foreach (var node in o.MapNodes)
                if (node.Conquered && node.RaidTimer > 0) node.RaidTimer = Math.Max(0, node.RaidTimer - power * 0.5);
        double faith = power;
        state.ActiveCoven.Faith += faith;
        o.LifetimeFaith += faith;
        return faith;
    }

    public static int SermonPowerUpgradeCost(OccultState o) => (int)Math.Ceiling(OccultBalance.SermonCostBase * Math.Pow(OccultBalance.SermonCostGrowth, o.SermonPowerLevel));
    public static bool CanBuySermonPower(GameState state) => state.ActiveCoven.Faith >= SermonPowerUpgradeCost(state.Occult);
    public static bool BuySermonPower(GameState state) { int cost = SermonPowerUpgradeCost(state.Occult); if (state.ActiveCoven.Faith < cost) return false; state.ActiveCoven.Faith -= cost; state.Occult.SermonPowerLevel++; return true; }

    public static int AcolyteHireCost(GameState state) => (int)Math.Ceiling(OccultBalance.SermonCostBase * Math.Pow(OccultBalance.SermonCostGrowth, state.Occult.Acolytes / 10.0));
    public static bool CanHireAcolyte(GameState state) => state.ActiveCoven.Faith >= AcolyteHireCost(state) && state.Occult.Acolytes < CultistHierarchy.AcolyteCap(state.Occult);
    public static bool HireAcolyte(GameState state) { if (!CanHireAcolyte(state)) return false; state.ActiveCoven.Faith -= AcolyteHireCost(state); state.Occult.Acolytes++; return true; }

    public static double AcolyteFaithPerSec(GameState state)
    {
        var o = state.Occult;
        double baseRate = o.Acolytes * 0.1;
        double mult = GrandSacrifice.GlobalProductionMult(state) * Grimoire.GlobalProductionMult(o);
        if (TechTree.HasTech(o, TechId.SanguineAutomata)) baseRate += o.Acolytes * 0.05;
        return baseRate * mult;
    }

    public static double TotalFaithPerSec(GameState state)
    {
        var o = state.Occult;
        double total = AcolyteFaithPerSec(state);
        total *= GrandSacrifice.GlobalProductionMult(state);
        total *= WorldMapSystem.GreatSealMultiplier(o);
        return total;
    }

    public static double TotalMapFaithPerSec(GameState state)
    {
        var o = state.Occult;
        double baseFaith = WorldMapSystem.TotalFaithPerSec(o);
        baseFaith += o.Minions.Count(m => m.Role == PromotedRole.Scholar) * OccultBalance.ScholarFaithPerSec;
        baseFaith += o.Minions.Count(m => m.Role == PromotedRole.Infiltrator) * OccultBalance.InfiltratorFaithPerSec;
        baseFaith *= CultistHierarchy.FaithMult(o) * Grimoire.FaithBonus(o) * o.ElixirFaithMult;
        if (o.IsMassHysteriaActive) baseFaith *= 2.0;
        return baseFaith;
    }

    public static void Tick(GameState state, double deltaSec)
    {
        var o = state.Occult;
        double faith = TotalFaithPerSec(state) * deltaSec;
        state.ActiveCoven.Faith += faith; o.LifetimeFaith += faith;
        double mapFaith = TotalMapFaithPerSec(state) * deltaSec;
        state.ActiveCoven.Faith += mapFaith; o.LifetimeFaith += mapFaith;
        WorldMapSystem.TickSuspicion(o, deltaSec);
        WorldMapSystem.TickMaterials(o, deltaSec);
        Cauldron.TickElixir(o, deltaSec);
        if (o.FrenzyTimer > 0) o.FrenzyTimer = Math.Max(0, o.FrenzyTimer - deltaSec);
        if (o.MassHysteriaTimer > 0) o.MassHysteriaTimer = Math.Max(0, o.MassHysteriaTimer - deltaSec);
        if (TechTree.HasTech(o, TechId.AutophagousCult)) { int cap = CultistHierarchy.AcolyteCap(o); if (o.Acolytes > cap) { int excess = o.Acolytes - cap; o.Acolytes = cap; state.ActiveCoven.Faith += excess * OccultBalance.SacrificeSermonMult * 0.5; } }
        if (WorldMapSystem.IsRaidTriggered(o) && !TechTree.HasTech(o, TechId.InquisitorsBlindfold)) WorldMapSystem.ApplyRaid(o);
    }

    public static bool CanActivateFrenzy(OccultState o) => TechTree.HasTech(o, TechId.ExsanguinationEngine) && o.Minions.Count > 0 && !o.IsFrenzyActive;
    public static bool ActivateFrenzy(OccultState o) { if (!CanActivateFrenzy(o)) return false; o.Minions.RemoveAt(0); o.FrenzyTimer = OccultBalance.FrenzyDurationSec; return true; }
    public static bool CanActivateMassHysteria(OccultState o) => TechTree.HasTech(o, TechId.MassHysteria) && !o.IsMassHysteriaActive;
    public static bool ActivateMassHysteria(OccultState o) { if (!CanActivateMassHysteria(o)) return false; o.MassHysteriaTimer = OccultBalance.MassHysteriaDurationSec; return true; }
}
