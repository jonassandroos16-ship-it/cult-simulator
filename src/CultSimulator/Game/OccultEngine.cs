namespace CultSimulator.Game;

public static class OccultEngine
{
    public static double ClickPower(GameState state)
    {
        var o = state.Occult;
        double basePower = GrandSacrifice.ClickPowerBase(state) + o.SermonPowerLevel;
        double mult = CultistHierarchy.TapPowerMult(o) * Grimoire.TapPowerBonus(o) * o.ElixirTapMult * GrandSacrifice.GlobalProductionMult(state);
        if (o.IsFrenzyActive) mult *= OccultBalance.FrenzyMultiplier;
        if (o.IsWhisperChoirActive) mult *= 3.0;
        return basePower * mult;
    }

    public static double ClickPowerForCoven(GameState state, CovenState coven)
    {
        var o = coven.Occult;
        double basePower = GrandSacrifice.ClickPowerBase(state) + o.SermonPowerLevel;
        double mult = CultistHierarchy.TapPowerMult(o) * Grimoire.TapPowerBonus(o) * o.ElixirTapMult * GrandSacrifice.GlobalProductionMult(state);
        if (o.IsFrenzyActive) mult *= OccultBalance.FrenzyMultiplier;
        if (o.IsWhisperChoirActive) mult *= 3.0;
        return basePower * mult;
    }

    public static double Tap(GameState state)
    {
        var coven = state.ActiveCoven;
        var o = coven.Occult;
        double power = ClickPower(state);
        if (Grimoire.BloodVoidConversionActive(o))
            foreach (var node in o.MapNodes)
                if (node.Conquered && node.RaidTimer > 0) node.RaidTimer = Math.Max(0, node.RaidTimer - power * 0.5);
        double faith = power;
        coven.Faith += faith;
        o.LifetimeFaith += faith;
        return faith;
    }

    public static int SermonPowerUpgradeCost(OccultState o) => (int)Math.Ceiling(OccultBalance.SermonCostBase * Math.Pow(OccultBalance.SermonCostGrowth, o.SermonPowerLevel));
    public static bool CanBuySermonPower(GameState state) => state.ActiveCoven.Faith >= SermonPowerUpgradeCost(state.Occult);
    public static bool BuySermonPower(GameState state) { int cost = SermonPowerUpgradeCost(state.Occult); if (state.ActiveCoven.Faith < cost) return false; state.ActiveCoven.Faith -= cost; state.Occult.SermonPowerLevel++; return true; }

    public static int AcolyteHireCost(GameState state) => (int)Math.Ceiling(OccultBalance.AcolyteCostBase * Math.Pow(OccultBalance.AcolyteCostGrowth, state.Occult.Acolytes));
    public static bool CanHireAcolyte(GameState state) => state.ActiveCoven.Faith >= AcolyteHireCost(state) && state.Occult.Acolytes < CultistHierarchy.AcolyteCap(state.Occult, state.ActiveCoven);
    public static bool HireAcolyte(GameState state) { if (!CanHireAcolyte(state)) return false; state.ActiveCoven.Faith -= AcolyteHireCost(state); state.Occult.Acolytes++; return true; }

    public static double AcolyteFaithPerSec(GameState state) => AcolyteFaithPerSec(state.Occult, GrandSacrifice.GlobalProductionMult(state));

    public static double AcolytePassivePerSec(GameState state) => AcolyteFaithPerSec(state);

    private static double AcolyteFaithPerSec(OccultState o, double globalMult)
    {
        double baseRate = o.Acolytes * 0.1;
        double mult = globalMult * Grimoire.GlobalProductionMult(o);
        if (TechTree.HasTech(o, TechId.SanguineAutomata)) baseRate += o.Acolytes * 0.05;
        return baseRate * mult;
    }

    public static double TotalFaithPerSec(GameState state) => TotalFaithPerSecForCoven(state, state.ActiveCoven);

    public static double TotalFaithPerSecForCoven(GameState state, CovenState coven)
    {
        var o = coven.Occult;
        double total = AcolyteFaithPerSecForCoven(o, state);
        total *= WorldMapSystem.GreatSealMultiplier(o);
        return total;
    }

    private static double AcolyteFaithPerSecForCoven(OccultState o, GameState state)
    {
        double baseRate = o.Acolytes * 0.1;
        double mult = GrandSacrifice.GlobalProductionMult(state) * Grimoire.GlobalProductionMult(o);
        if (TechTree.HasTech(o, TechId.SanguineAutomata)) baseRate += o.Acolytes * 0.05;
        return baseRate * mult;
    }

    public static double TotalMapFaithPerSec(GameState state) => TotalMapFaithPerSecForCoven(state.Occult, state);

    public static double TotalMapFaithPerSecForCoven(OccultState o, GameState state)
    {
        double baseFaith = WorldMapSystem.TotalFaithPerSec(o);
        baseFaith += o.Minions.Count(m => m.Role == PromotedRole.Scholar) * OccultBalance.ScholarFaithPerSec;
        baseFaith += o.Minions.Count(m => m.Role == PromotedRole.Infiltrator) * OccultBalance.InfiltratorFaithPerSec;
        baseFaith *= CultistHierarchy.FaithMult(o) * Grimoire.FaithBonus(o) * o.ElixirFaithMult * GrandSacrifice.GlobalProductionMult(state);
        if (o.IsMassHysteriaActive) baseFaith *= 2.0;
        if (o.IsCovenBlessingActive) baseFaith *= 2.0;
        return baseFaith;
    }

    public static void Tick(GameState state, double deltaSec) => Tick(state, state.ActiveCoven, deltaSec);

    public static void Tick(GameState state, CovenState coven, double deltaSec)
    {
        var o = coven.Occult;
        double faith = TotalFaithPerSecForCoven(state, coven) * deltaSec;
        coven.Faith += faith; o.LifetimeFaith += faith;
        double mapFaith = TotalMapFaithPerSecForCoven(o, state) * deltaSec;
        coven.Faith += mapFaith; o.LifetimeFaith += mapFaith;
        double armyGain = o.Minions.Count(m => m.Role == PromotedRole.Zealot) * OccultBalance.ZealotArmyPowerPerSec * deltaSec;
        armyGain += o.Acolytes * OccultBalance.AcolyteArmyPowerPerSec * deltaSec;
        o.ArmyPower += armyGain;
        WorldMapSystem.TickSuspicion(o, deltaSec);
        WorldMapSystem.TickMaterials(o, deltaSec);
        Cauldron.TickElixir(o, deltaSec);
        if (o.FrenzyTimer > 0) o.FrenzyTimer = Math.Max(0, o.FrenzyTimer - deltaSec);
        if (o.MassHysteriaTimer > 0) o.MassHysteriaTimer = Math.Max(0, o.MassHysteriaTimer - deltaSec);
        if (o.DarkVigilTimer > 0) o.DarkVigilTimer = Math.Max(0, o.DarkVigilTimer - deltaSec);
        if (o.WhisperChoirTimer > 0) o.WhisperChoirTimer = Math.Max(0, o.WhisperChoirTimer - deltaSec);
        if (o.CovenBlessingTimer > 0) o.CovenBlessingTimer = Math.Max(0, o.CovenBlessingTimer - deltaSec);
        if (TechTree.HasTech(o, TechId.AutophagousCult)) { int cap = CultistHierarchy.AcolyteCap(o, coven); if (o.Acolytes > cap) { int excess = o.Acolytes - cap; o.Acolytes = cap; coven.Faith += excess * OccultBalance.SacrificeSermonMult * 0.5; } }
        if (WorldMapSystem.IsRaidTriggered(o) && !TechTree.HasTech(o, TechId.InquisitorsBlindfold)) WorldMapSystem.ApplyRaid(o);
    }

    public static bool CanActivateFrenzy(OccultState o) => TechTree.HasTech(o, TechId.ExsanguinationEngine) && o.Minions.Count > 0 && !o.IsFrenzyActive;
    public static bool ActivateFrenzy(OccultState o) { if (!CanActivateFrenzy(o)) return false; o.Minions.RemoveAt(0); o.FrenzyTimer = OccultBalance.FrenzyDurationSec; return true; }
    public static bool CanActivateMassHysteria(OccultState o) => TechTree.HasTech(o, TechId.MassHysteria) && !o.IsMassHysteriaActive;
    public static bool ActivateMassHysteria(OccultState o) { if (!CanActivateMassHysteria(o)) return false; o.MassHysteriaTimer = OccultBalance.MassHysteriaDurationSec; return true; }

    public static bool CanSacrificeAcolyte(GameState state) => state.Occult.Acolytes > 0 && state.Occult.Suspicion > 0;
    public static bool SacrificeAcolyte(GameState state)
    {
        if (!CanSacrificeAcolyte(state)) return false;
        var coven = state.ActiveCoven;
        var o = coven.Occult;
        o.Acolytes--;
        o.Suspicion = Math.Max(0, o.Suspicion - OccultBalance.AcolyteSacrificeSuspicionReduction);
        double faith = OccultBalance.SacrificeFaithBase * 5;
        coven.Faith += faith;
        o.LifetimeFaith += faith;
        return true;
    }

    public static bool CanActivateBloodOffering(GameState state) => state.Occult.Acolytes >= 5 && state.Occult.Suspicion > 0;
    public static bool ActivateBloodOffering(GameState state)
    {
        if (!CanActivateBloodOffering(state)) return false;
        var coven = state.ActiveCoven;
        var o = coven.Occult;
        o.Acolytes -= 5;
        o.Suspicion = 0;
        double faith = OccultBalance.SacrificeFaithBase * 50;
        coven.Faith += faith;
        o.LifetimeFaith += faith;
        return true;
    }

    public static bool CanActivateDarkVigil(OccultState o) => o.Acolytes >= 3 && !o.IsDarkVigilActive;
    public static bool ActivateDarkVigil(OccultState o) { if (!CanActivateDarkVigil(o)) return false; o.Acolytes -= 3; o.DarkVigilTimer = OccultBalance.DarkVigilDurationSec; return true; }

    public static bool CanActivateWhisperChoir(OccultState o) => o.Acolytes >= 10 && !o.IsWhisperChoirActive;
    public static bool ActivateWhisperChoir(OccultState o) { if (!CanActivateWhisperChoir(o)) return false; o.Acolytes -= 10; o.WhisperChoirTimer = OccultBalance.WhisperChoirDurationSec; return true; }

    public static bool CanActivateCovenBlessing(OccultState o) => o.Acolytes >= 20 && !o.IsCovenBlessingActive;
    public static bool ActivateCovenBlessing(OccultState o) { if (!CanActivateCovenBlessing(o)) return false; o.Acolytes -= 20; o.CovenBlessingTimer = OccultBalance.CovenBlessingDurationSec; return true; }
}
