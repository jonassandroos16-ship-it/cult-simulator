namespace CultSimulator.Game;

public static class CultistHierarchy
{
    private static readonly string[] MinionNames = { "Vael", "Morgrith", "Sszark", "Nyx", "Dolgrim", "Cael", "Vesh", "Kraix", "Umbra", "Tholos", "Zeph", "Grel", "Ishara", "Pyrros", "Mallek", "Sevren", "Ourobor", "Nethis", "Kaelthas", "Dravos" };
    private static readonly string[] CouncilNames = { "Grand Harrower", "Eye of the Abyss", "Mouth of the Void", "Hand of Ruin", "Voice of the Deep", "Keeper of Seals" };

    public static int AcolyteCap(OccultState o)
    {
        int cap = OccultBalance.AcolyteCapBase;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "flesh_golem") cap += 50; }
        foreach (var m in o.Minions) if (m.Trait?.Id == "fleshspeaker") cap += 50;
        foreach (var c in o.HighCouncil) if (c.Trait?.Id == "fleshspeaker") cap += 50;
        return cap;
    }

    public static bool CanPromote(OccultState o) => o.Acolytes >= OccultBalance.PromoteAcolyteCost;

    public static Minion Promote(OccultState o)
    {
        o.Acolytes -= OccultBalance.PromoteAcolyteCost;
        var role = OccultData.PromotedRoles[Random.Shared.Next(OccultData.PromotedRoles.Length)];
        var trait = OccultData.Traits[Random.Shared.Next(OccultData.Traits.Length)];
        var minion = new Minion { Role = role, TraitId = trait.Id, Name = MinionNames[Random.Shared.Next(MinionNames.Length)] };
        o.Minions.Add(minion);
        return minion;
    }

    public static bool CanSacrifice(OccultState o, string minionId) => o.Minions.Any(m => m.Id == minionId);

    public static double Sacrifice(GameState state, string minionId)
    {
        var o = state.Occult;
        var minion = o.Minions.FirstOrDefault(m => m.Id == minionId);
        if (minion == null) return 0;
        o.Minions.Remove(minion);
        double faith = OccultBalance.SacrificeFaithBase * OccultBalance.SacrificeSermonMult * (minion.Trait?.FaithMult ?? 1.0);
        state.ActiveCoven.Faith += faith;
        o.LifetimeFaith += faith;
        return faith;
    }

    public static bool CanAppointCouncil(OccultState o, CouncilRole role)
    {
        if (o.HighCouncil.Any(c => c.Role == role)) return false;
        if (o.HighCouncil.Count >= 3) return false;
        if (role == CouncilRole.HighPriest && o.GrandSacrificeCount == 0 && !o.UnlockedTechs.Contains(TechId.AstralAnchor)) return false;
        return o.Minions.Count > 0;
    }

    public static bool AppointCouncil(OccultState o, CouncilRole role, string minionId)
    {
        if (!CanAppointCouncil(o, role)) return false;
        var minion = o.Minions.FirstOrDefault(m => m.Id == minionId);
        if (minion == null) return false;
        o.Minions.Remove(minion);
        o.HighCouncil.Add(new CovenMember { Role = role, Name = minion.Name, MinionId = minion.Id, TraitId = minion.TraitId, OriginalRole = minion.Role });
        return true;
    }

    public static bool RemoveCouncil(OccultState o, CouncilRole role)
    {
        var member = o.HighCouncil.FirstOrDefault(c => c.Role == role);
        if (member == null) return false;
        o.HighCouncil.Remove(member);
        if (!string.IsNullOrEmpty(member.MinionId))
            o.Minions.Add(new Minion { Id = member.MinionId, Name = member.Name, Role = member.OriginalRole, TraitId = member.TraitId });
        return true;
    }

    public static double RaidPowerMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions) if (m.Role == PromotedRole.Zealot) mult *= m.Trait?.RaidPowerMult ?? 1.0;
        if (o.HighCouncil.Any(c => c.Role == CouncilRole.Archon)) mult *= 1.5;
        return mult;
    }

    public static double SuspicionMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions) mult *= m.Trait?.SuspicionMult ?? 1.0;
        if (o.HighCouncil.Any(c => c.Role == CouncilRole.Inquisitor)) mult *= 0.8;
        return mult;
    }

    public static double FaithMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions) if (m.Role == PromotedRole.Scholar || m.Role == PromotedRole.Infiltrator) mult *= m.Trait?.FaithMult ?? 1.0;
        if (o.HighCouncil.Any(c => c.Role == CouncilRole.HighPriest)) mult *= 1.5;
        return mult;
    }

    public static double TapPowerMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions) if (m.Trait?.Id == "zealous") mult *= 1.3;
        return mult;
    }
}
