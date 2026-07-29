using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class Grimoire
{
    public static bool CanSocket(OccultState o) => o.SocketedArtifacts.Count < o.UnlockedSocketCount;

    public static bool Socket(OccultState o, string artifactId)
    {
        if (!o.OwnedArtifacts.Contains(artifactId) || o.SocketedArtifacts.Contains(artifactId) || !CanSocket(o)) return false;
        o.OwnedArtifacts.Remove(artifactId);
        o.SocketedArtifacts.Add(artifactId);
        return true;
    }

    public static bool Unsocket(OccultState o, string artifactId)
    {
        if (!o.SocketedArtifacts.Contains(artifactId)) return false;
        o.SocketedArtifacts.Remove(artifactId);
        o.OwnedArtifacts.Add(artifactId);
        return true;
    }

    public static bool OwnsArtifact(OccultState o, string artifactId) => o.OwnedArtifacts.Contains(artifactId) || o.SocketedArtifacts.Contains(artifactId);

    public static void AddArtifact(OccultState o, string artifactId) { if (!OwnsArtifact(o, artifactId)) o.OwnedArtifacts.Add(artifactId); }

    public static IReadOnlyDictionary<ArtifactSuit, int> SocketedSuitCounts(OccultState o)
    {
        var counts = new Dictionary<ArtifactSuit, int>();
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null) counts[def.Suit] = counts.GetValueOrDefault(def.Suit) + 1; }
        return counts;
    }

    public static bool HasSetBonus(OccultState o, ArtifactSuit suit) => SocketedSuitCounts(o).GetValueOrDefault(suit) >= 3;

    public static double TapPowerBonus(OccultState o)
    {
        double bonus = 1.0;
        var counts = SocketedSuitCounts(o);
        double setMult = TechTree.SetBonusMult(o);
        if (counts.GetValueOrDefault(ArtifactSuit.Blood) >= 3) bonus += OccultBalance.Blood3TapBonus * setMult;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "blood_chalice") bonus += 0.15; }
        return bonus;
    }

    public static bool BloodVoidConversionActive(OccultState o) =>
        SocketedSuitCounts(o).GetValueOrDefault(ArtifactSuit.Blood) >= 2 && SocketedSuitCounts(o).GetValueOrDefault(ArtifactSuit.Void) >= 1;

    public static double FaithBonus(OccultState o)
    {
        double bonus = 1.0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "void_orb") bonus += 0.10; }
        return bonus;
    }

    public static double SuspicionReductionBonus(OccultState o)
    {
        double bonus = 1.0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "void_cloak") bonus -= 0.10; }
        return bonus;
    }

    public static double GlobalProductionMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null) { if (def.Id == "flesh_graft") mult += 0.15; if (def.Id == "flesh_seed") mult += 0.05; } }
        return mult;
    }

    public static int AcolyteCapBonus(OccultState o)
    {
        int bonus = 0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "flesh_golem") bonus += 50; }
        return bonus;
    }

    public static double GreatSealMult(OccultState o)
    {
        double mult = OccultBalance.GreatSealMultiplier;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "mind_crown") mult += 0.30; }
        return mult;
    }
}
