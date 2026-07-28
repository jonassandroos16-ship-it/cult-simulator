namespace CultSimulator.Game;

/// <summary>
/// Central engine for the occult systems. Integrates cultist hierarchy,
/// tech tree, grimoire, world map, cauldron, and grand sacrifice into the
/// core tap -> devotion -> FK -> favor loop. Pure functions over state.
/// </summary>
public static class OccultEngine
{
    public static double ClickPower(GameState state)
    {
        var o = state.Occult;
        double basePower = GrandSacrifice.ClickPowerBase(state) + o.ClickPowerLevel;
        double mult = CultistHierarchy.TapPowerMult(o)
            * Grimoire.TapPowerBonus(o)
            * o.ElixirTapMult;
        if (o.IsFrenzyActive) mult *= OccultBalance.FrenzyMultiplier;
        return basePower * mult;
    }

    public static double Tap(GameState state)
    {
        var o = state.Occult;
        double power = ClickPower(state);

        if (Grimoire.BloodVoidConversionActive(o))
        {
            foreach (var node in o.MapNodes)
            {
                if (node.Conquered && node.RaidTimer > 0)
                    node.RaidTimer = Math.Max(0, node.RaidTimer - power * 0.5);
            }
        }

        double devotion = power;
        o.Devotion += devotion;
        o.LifetimeDevotion += devotion;
        return devotion;
    }

    public static int ClickPowerUpgradeCost(OccultState o) =>
        (int)Math.Ceiling(OccultBalance.DevotionCostBase * Math.Pow(OccultBalance.DevotionCostGrowth, o.ClickPowerLevel));

    public static bool CanBuyClickPower(OccultState o) =>
        o.Devotion >= ClickPowerUpgradeCost(o);

    public static bool BuyClickPower(OccultState o)
    {
        int cost = ClickPowerUpgradeCost(o);
        if (o.Devotion < cost) return false;
        o.Devotion -= cost;
        o.ClickPowerLevel++;
        return true;
    }

    public static int AcolyteHireCost(OccultState o) =>
        (int)Math.Ceiling(OccultBalance.DevotionCostBase * Math.Pow(OccultBalance.DevotionCostGrowth, o.Acolytes / 10.0));

    public static bool CanHireAcolyte(OccultState o) =>
        o.Devotion >= AcolyteHireCost(o) && o.Acolytes < CultistHierarchy.AcolyteCap(o);

    public static bool HireAcolyte(OccultState o)
    {
        if (!CanHireAcolyte(o)) return false;
        o.Devotion -= AcolyteHireCost(o);
        o.Acolytes++;
        return true;
    }

    public static double AcolyteDevotionPerSec(OccultState o)
    {
        double baseRate = o.Acolytes * 0.1;
        double mult = GrandSacrifice.GlobalProductionMult(new GameState { Occult = o })
            * Grimoire.GlobalProductionMult(o);
        if (TechTree.HasTech(o, TechId.SanguineAutomata)) baseRate += o.Acolytes * 0.05;
        return baseRate * mult;
    }

    public static double TotalDevotionPerSec(GameState state)
    {
        var o = state.Occult;
        double total = AcolyteDevotionPerSec(o);
        total *= GrandSacrifice.GlobalProductionMult(state);
        total *= WorldMapSystem.GreatSealMultiplier(o);
        return total;
    }

    public static double TotalFkPerSec(GameState state)
    {
        var o = state.Occult;
        double baseFk = WorldMapSystem.TotalFkPerSec(o);
        int scholars = o.Minions.Count(m => m.Role == PromotedRole.Scholar);
        int infiltrators = o.Minions.Count(m => m.Role == PromotedRole.Infiltrator);
        baseFk += scholars * OccultBalance.ScholarFkPerSec;
        baseFk += infiltrators * OccultBalance.InfiltratorFkPerSec;
        baseFk *= CultistHierarchy.FkMult(o);
        baseFk *= Grimoire.FkBonus(o);
        baseFk *= o.ElixirFkMult;
        if (o.IsMassHysteriaActive) baseFk *= 2.0;
        return baseFk;
    }

    public static void Tick(GameState state, double deltaSec)
    {
        var o = state.Occult;

        double devotion = TotalDevotionPerSec(state) * deltaSec;
        o.Devotion += devotion;
        o.LifetimeDevotion += devotion;

        double fk = TotalFkPerSec(state) * deltaSec;
        o.ForbiddenKnowledge += fk;

        WorldMapSystem.TickSuspicion(o, deltaSec);
        WorldMapSystem.TickMaterials(o, deltaSec);
        Cauldron.TickElixir(o, deltaSec);

        if (o.FrenzyTimer > 0) o.FrenzyTimer = Math.Max(0, o.FrenzyTimer - deltaSec);
        if (o.MassHysteriaTimer > 0) o.MassHysteriaTimer = Math.Max(0, o.MassHysteriaTimer - deltaSec);

        if (TechTree.HasTech(o, TechId.AutophagousCult))
        {
            int cap = CultistHierarchy.AcolyteCap(o);
            if (o.Acolytes > cap)
            {
                int excess = o.Acolytes - cap;
                o.Acolytes = cap;
                o.Devotion += excess * OccultBalance.SacrificeDevotionMult * 0.5;
            }
        }

        if (WorldMapSystem.IsRaidTriggered(o) && !TechTree.HasTech(o, TechId.InquisitorsBlindfold))
        {
            WorldMapSystem.ApplyRaid(o);
        }
    }

    public static bool CanActivateFrenzy(OccultState o) =>
        TechTree.HasTech(o, TechId.ExsanguinationEngine) && o.Minions.Count > 0 && !o.IsFrenzyActive;

    public static bool ActivateFrenzy(OccultState o)
    {
        if (!CanActivateFrenzy(o)) return false;
        var minion = o.Minions[0];
        o.Minions.RemoveAt(0);
        o.FrenzyTimer = OccultBalance.FrenzyDurationSec;
        return true;
    }

    public static bool CanActivateMassHysteria(OccultState o) =>
        TechTree.HasTech(o, TechId.MassHysteria) && !o.IsMassHysteriaActive;

    public static bool ActivateMassHysteria(OccultState o)
    {
        if (!CanActivateMassHysteria(o)) return false;
        o.MassHysteriaTimer = OccultBalance.MassHysteriaDurationSec;
        return true;
    }
}