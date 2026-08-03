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

    // Mind & Coercion remade — no Suspicion, all new effects
    public static double PreachBonusMult(OccultState o) => HasTech(o, TechId.PropagandaNetwork) ? 1.0 + OccultBalance.PropagandaPreachBonus : 1.0;
    public static double AcolyteFaithMult(OccultState o) => HasTech(o, TechId.CognitiveSedation) ? OccultBalance.CognitiveSedationMult : 1.0;
    public static double FollowerIncomeBonus(OccultState o) => HasTech(o, TechId.SubliminalBroadcast) ? 1.0 + OccultBalance.SubliminalIncomeBonus : 1.0;
    public static double AgentStrengthBonus(OccultState o) => HasTech(o, TechId.ZealotConditioning) ? 1.0 + OccultBalance.ZealotConditioningStrengthBonus : 1.0;
    public static double RecruitCostReduction(OccultState o) => HasTech(o, TechId.IndoctrinationRites) ? OccultBalance.IndoctrinationRecruitReduction : 0.0;
    public static double CollectiveTranceMult(OccultState o) => HasTech(o, TechId.CollectiveTrance) ? 1.0 + OccultBalance.CollectiveTranceFaithMult : 1.0;

    // Blood & Flesh new techs
    public static double SacrificeBonus(OccultState o) => HasTech(o, TechId.CrimsonTide) ? 1.0 + OccultBalance.CrimsonTideSacrificeBonus : 1.0;
    public static int FleshBindingCapBonus(OccultState o) => HasTech(o, TechId.FleshBinding) ? (int)OccultBalance.FleshBindingCapBonus : 0;
    public static double MarrowTransfusionMult(OccultState o) => HasTech(o, TechId.MarrowTransfusion) ? 1.0 + OccultBalance.MarrowTransfusionMult : 1.0;
    public static double FrenzyMultiplier(OccultState o) => HasTech(o, TechId.BloodApocalypse) ? OccultBalance.FrenzyApocalypseMultiplier : OccultBalance.FrenzyMultiplier;
    public static int FrenzyDuration(OccultState o) => HasTech(o, TechId.BloodApocalypse) ? OccultBalance.FrenzyApocalypseDurationSec : OccultBalance.FrenzyDurationSec;

    // Void & Astral new techs
    public static double SetBonusMult(OccultState o) => HasTech(o, TechId.ResonanceMastery) ? 2.0 : 1.0;
    public static double FaithRetentionPercent(OccultState o) => HasTech(o, TechId.MemoriesOfTheDeep) ? 0.10 : 0.0;
    public static double FaithRetentionPercentDeep(OccultState o) => HasTech(o, TechId.EchoesOfCreation) ? 0.25 : FaithRetentionPercent(o);
    public static double EldritchFavorContinentMult(OccultState o) => HasTech(o, TechId.TheStarEatersFeast) ? 2.0 : 1.0;
    public static double VoidTwinFaithBonus(OccultState o) => HasTech(o, TechId.VoidTwin) ? o.SocketedArtifacts.Count * 0.05 : 0.0;
    public static double AstralDistillationMult(OccultState o) => HasTech(o, TechId.AstralDistillation) ? 1.5 : 1.0;
    public static double ElderSignTapBonus(OccultState o) => HasTech(o, TechId.ElderSign) ? 2.0 : 1.0;
    public static double CosmicConvergenceMult(OccultState o) => HasTech(o, TechId.CosmicConvergence) ? 2.0 : 1.0;

    // Outer Gate
    public static bool KeepAllTechs(OccultState o) => HasTech(o, TechId.VoidHeart);
    public static double AscensionProtocolMult(OccultState o) => HasTech(o, TechId.AscensionProtocol) ? 2.0 : 1.0;
}
