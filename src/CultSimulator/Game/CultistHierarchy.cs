namespace CultSimulator.Game;

/// <summary>
/// Cultist hierarchy logic: promoting Acolytes to named minions, sacrificing
/// minions for resources, and managing the High Council. Pure functions over
/// <see cref="OccultState"/> so it stays modular and testable.
/// </summary>
public static class CultistHierarchy
{
    private static readonly string[] MinionNames =
    {
        "Vael", "Morgrith", "Sszark", "Nyx", "Dolgrim", "Cael", "Vesh",
        "Kraix", "Umbra", "Tholos", "Zeph", "Grel", "Ishara", "Pyrros",
        "Mallek", "Sevren", "Ourobor", "Nethis", "Kaelthas", "Dravos"
    };

    private static readonly string[] CouncilNames =
    {
        "Grand Harrower", "Eye of the Abyss", "Mouth of the Void",
        "Hand of Ruin", "Voice of the Deep", "Keeper of Seals"
    };

    public static int AcolyteCap(OccultState o)
    {
        int cap = OccultBalance.AcolyteCapBase;
        foreach (var artifactId in o.SocketedArtifacts)
        {
            var def = OccultData.Artifact(artifactId);
            if (def != null && def.Id == "flesh_golem") cap += 50;
        }
        foreach (var m in o.Minions)
        {
            if (m.Trait?.Id == "fleshspeaker") cap += 50;
        }
        return cap;
    }

    public static bool CanPromote(OccultState o) =>
        o.Acolytes >= OccultBalance.PromoteAcolyteCost;

    public static Minion Promote(OccultState o)
    {
        o.Acolytes -= OccultBalance.PromoteAcolyteCost;
        var role = OccultData.PromotedRoles[Random.Shared.Next(OccultData.PromotedRoles.Length)];
        var trait = OccultData.Traits[Random.Shared.Next(OccultData.Traits.Length)];
        var name = MinionNames[Random.Shared.Next(MinionNames.Length)];

        var minion = new Minion
        {
            Role = role,
            TraitId = trait.Id,
            Name = name
        };
        o.Minions.Add(minion);
        return minion;
    }

    public static bool CanSacrifice(OccultState o, string minionId) =>
        o.Minions.Any(m => m.Id == minionId);

    public static (double devotion, double fk) Sacrifice(OccultState o, string minionId)
    {
        var minion = o.Minions.FirstOrDefault(m => m.Id == minionId);
        if (minion == null) return (0, 0);

        o.Minions.Remove(minion);

        double traitFkMult = minion.Trait?.FkMult ?? 1.0;
        double fk = OccultBalance.SacrificeFkBase * traitFkMult;
        double devotion = OccultBalance.SacrificeDevotionMult * traitFkMult;

        o.ForbiddenKnowledge += fk;
        o.Devotion += devotion;
        o.LifetimeDevotion += devotion;

        return (devotion, fk);
    }

    public static bool CanAppointCouncil(OccultState o, CouncilRole role)
    {
        if (o.HighCouncil.Any(c => c.Role == role)) return false;
        if (o.HighCouncil.Count >= 3) return false;
        if (role == CouncilRole.HighPriest && o.GrandSacrificeCount == 0
            && !o.UnlockedTechs.Contains(TechId.AstralAnchor)) return false;
        return o.Minions.Count > 0;
    }

    public static bool AppointCouncil(OccultState o, CouncilRole role, string minionId)
    {
        if (!CanAppointCouncil(o, role)) return false;
        var minion = o.Minions.FirstOrDefault(m => m.Id == minionId);
        if (minion == null) return false;

        o.Minions.Remove(minion);
        o.HighCouncil.Add(new CouncilMember
        {
            Role = role,
            Name = CouncilNames[Random.Shared.Next(CouncilNames.Length)]
        });
        return true;
    }

    public static bool RemoveCouncil(OccultState o, CouncilRole role)
    {
        var member = o.HighCouncil.FirstOrDefault(c => c.Role == role);
        if (member == null) return false;
        o.HighCouncil.Remove(member);
        return true;
    }

    public static double RaidPowerMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions)
        {
            if (m.Role == PromotedRole.Zealot)
                mult *= m.Trait?.RaidPowerMult ?? 1.0;
        }
        if (o.HighCouncil.Any(c => c.Role == CouncilRole.Archon)) mult *= 1.5;
        return mult;
    }

    public static double SuspicionMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions)
        {
            mult *= m.Trait?.SuspicionMult ?? 1.0;
        }
        if (o.HighCouncil.Any(c => c.Role == CouncilRole.Inquisitor)) mult *= 0.8;
        return mult;
    }

    public static double FkMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions)
        {
            if (m.Role == PromotedRole.Scholar || m.Role == PromotedRole.Infiltrator)
                mult *= m.Trait?.FkMult ?? 1.0;
        }
        if (o.HighCouncil.Any(c => c.Role == CouncilRole.HighPriest)) mult *= 1.5;
        return mult;
    }

    public static double TapPowerMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var m in o.Minions)
        {
            if (m.Trait?.Id == "zealous") mult *= 1.3;
        }
        return mult;
    }
}