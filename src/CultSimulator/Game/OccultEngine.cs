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

    public static int InitiateHireCost(GameState state) => (int)Math.Ceiling(OccultBalance.InitiateCostBase * Math.Pow(OccultBalance.InitiateCostGrowth, state.Occult.Initiates));
    public static bool CanHireInitiate(GameState state) => state.ActiveCoven.Faith >= InitiateHireCost(state) && state.Occult.Initiates < CultistHierarchy.InitiateCap(state.Occult, state.ActiveCoven);
    public static bool HireInitiate(GameState state) { if (!CanHireInitiate(state)) return false; state.ActiveCoven.Faith -= InitiateHireCost(state); state.Occult.Initiates++; return true; }

    // Legacy aliases so existing callers compile
    public static int AcolyteHireCost(GameState state) => InitiateHireCost(state);
    public static bool CanHireAcolyte(GameState state) => CanHireInitiate(state);
    public static bool HireAcolyte(GameState state) => HireInitiate(state);

    public static double InitiateFaithPerSec(GameState state) => InitiateFaithPerSec(state.Occult, GrandSacrifice.GlobalProductionMult(state));
    public static double AcolyteFaithPerSec(GameState state) => InitiateFaithPerSec(state);
    public static double AcolytePassivePerSec(GameState state) => InitiateFaithPerSec(state);

    private static double InitiateFaithPerSec(OccultState o, double globalMult)
    {
        double baseRate = o.Initiates * 0.1;
        double mult = globalMult * Grimoire.GlobalProductionMult(o);
        if (TechTree.HasTech(o, TechId.SanguineAutomata)) baseRate += o.Initiates * 0.05;
        return baseRate * mult;
    }

    public static double TotalFaithPerSec(GameState state)
    {
        var o = state.Occult;
        double total = InitiateFaithPerSec(state);
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
        if (o.IsCovenBlessingActive) baseFaith *= 2.0;
        return baseFaith;
    }

    public static void Tick(GameState state, double deltaSec)
    {
        var o = state.Occult;
        double faith = TotalFaithPerSec(state) * deltaSec;
        state.ActiveCoven.Faith += faith; o.LifetimeFaith += faith;
        double mapFaith = TotalMapFaithPerSec(state) * deltaSec;
        state.ActiveCoven.Faith += mapFaith; o.LifetimeFaith += mapFaith;
        double armyGain = o.Minions.Count(m => m.Role == PromotedRole.Zealot) * OccultBalance.ZealotArmyPowerPerSec * deltaSec;
        armyGain += o.Initiates * OccultBalance.InitiateArmyPowerPerSec * deltaSec;
        o.ArmyPower += armyGain;
        WorldMapSystem.TickSuspicion(o, deltaSec);
        WorldMapSystem.TickMaterials(o, deltaSec);
        Cauldron.TickElixir(o, deltaSec);
        if (o.FrenzyTimer > 0) o.FrenzyTimer = Math.Max(0, o.FrenzyTimer - deltaSec);
        if (o.MassHysteriaTimer > 0) o.MassHysteriaTimer = Math.Max(0, o.MassHysteriaTimer - deltaSec);
        if (o.DarkVigilTimer > 0) o.DarkVigilTimer = Math.Max(0, o.DarkVigilTimer - deltaSec);
        if (o.WhisperChoirTimer > 0) o.WhisperChoirTimer = Math.Max(0, o.WhisperChoirTimer - deltaSec);
        if (o.CovenBlessingTimer > 0) o.CovenBlessingTimer = Math.Max(0, o.CovenBlessingTimer - deltaSec);
        if (TechTree.HasTech(o, TechId.AutophagousCult)) { int cap = CultistHierarchy.InitiateCap(o, state.ActiveCoven); if (o.Initiates > cap) { int excess = o.Initiates - cap; o.Initiates = cap; state.ActiveCoven.Faith += excess * OccultBalance.SacrificeSermonMult * 0.5; } }
        if (WorldMapSystem.IsRaidTriggered(o) && !TechTree.HasTech(o, TechId.InquisitorsBlindfold)) WorldMapSystem.ApplyRaid(o);
    }

    public static bool CanActivateFrenzy(OccultState o) => TechTree.HasTech(o, TechId.ExsanguinationEngine) && o.Minions.Count > 0 && !o.IsFrenzyActive;
    public static bool ActivateFrenzy(OccultState o) { if (!CanActivateFrenzy(o)) return false; o.Minions.RemoveAt(0); o.FrenzyTimer = OccultBalance.FrenzyDurationSec; return true; }
    public static bool CanActivateMassHysteria(OccultState o) => TechTree.HasTech(o, TechId.MassHysteria) && !o.IsMassHysteriaActive;
    public static bool ActivateMassHysteria(OccultState o) { if (!CanActivateMassHysteria(o)) return false; o.MassHysteriaTimer = OccultBalance.MassHysteriaDurationSec; return true; }

    public static bool CanSacrificeInitiate(GameState state) => state.Occult.Initiates > 0 && state.Occult.Suspicion > 0;
    public static bool CanSacrificeAcolyte(GameState state) => CanSacrificeInitiate(state);
    public static bool SacrificeInitiate(GameState state)
    {
        if (!CanSacrificeInitiate(state)) return false;
        var o = state.Occult;
        o.Initiates--;
        o.Suspicion = Math.Max(0, o.Suspicion - OccultBalance.InitiateSacrificeSuspicionReduction);
        double faith = OccultBalance.SacrificeFaithBase * 5;
        state.ActiveCoven.Faith += faith;
        o.LifetimeFaith += faith;
        return true;
    }
    public static bool SacrificeAcolyte(GameState state) => SacrificeInitiate(state);

    public static bool CanActivateBloodOffering(GameState state) => state.Occult.Initiates >= 5 && state.Occult.Suspicion > 0;
    public static bool ActivateBloodOffering(GameState state)
    {
        if (!CanActivateBloodOffering(state)) return false;
        var o = state.Occult;
        o.Initiates -= 5;
        o.Suspicion = 0;
        double faith = OccultBalance.SacrificeFaithBase * 50;
        state.ActiveCoven.Faith += faith;
        o.LifetimeFaith += faith;
        return true;
    }

    public static bool CanActivateDarkVigil(OccultState o) => o.Initiates >= 3 && !o.IsDarkVigilActive;
    public static bool ActivateDarkVigil(OccultState o) { if (!CanActivateDarkVigil(o)) return false; o.Initiates -= 3; o.DarkVigilTimer = OccultBalance.DarkVigilDurationSec; return true; }

    public static bool CanActivateWhisperChoir(OccultState o) => o.Initiates >= 10 && !o.IsWhisperChoirActive;
    public static bool ActivateWhisperChoir(OccultState o) { if (!CanActivateWhisperChoir(o)) return false; o.Initiates -= 10; o.WhisperChoirTimer = OccultBalance.WhisperChoirDurationSec; return true; }

    public static bool CanActivateCovenBlessing(OccultState o) => o.Initiates >= 20 && !o.IsCovenBlessingActive;
    public static bool ActivateCovenBlessing(OccultState o) { if (!CanActivateCovenBlessing(o)) return false; o.Initiates -= 20; o.CovenBlessingTimer = OccultBalance.CovenBlessingDurationSec; return true; }

    public static double TotalFaithPerSecForCoven(GameState state, CovenState coven)
    {
        var o = coven.Occult;
        double total = o.Initiates * 0.1 * GrandSacrifice.GlobalProductionMult(state) * Grimoire.GlobalProductionMult(o);
        if (TechTree.HasTech(o, TechId.SanguineAutomata)) total += o.Initiates * 0.05;
        total *= WorldMapSystem.GreatSealMultiplier(o);
        return total;
    }

    public static double TotalMapFaithPerSecForCoven(OccultState o, GameState state)
    {
        double baseFaith = WorldMapSystem.TotalFaithPerSec(o);
        baseFaith += o.Minions.Count(m => m.Role == PromotedRole.Scholar) * OccultBalance.ScholarFaithPerSec;
        baseFaith += o.Minions.Count(m => m.Role == PromotedRole.Infiltrator) * OccultBalance.InfiltratorFaithPerSec;
        baseFaith *= CultistHierarchy.FaithMult(o) * Grimoire.FaithBonus(o) * o.ElixirFaithMult;
        if (o.IsMassHysteriaActive) baseFaith *= 2.0;
        if (o.IsCovenBlessingActive) baseFaith *= 2.0;
        return baseFaith;
    }

}
