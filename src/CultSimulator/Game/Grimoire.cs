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
        double convergence = TechTree.CosmicConvergenceMult(o);
        if (counts.GetValueOrDefault(ArtifactSuit.Blood) >= 3) bonus += OccultBalance.Blood3TapBonus * setMult * convergence;
        foreach (var artifactId in o.SocketedArtifacts)
        {
            var def = OccultData.Artifact(artifactId);
            if (def == null) continue;
            if (def.Id == "blood_chalice") bonus += 0.15 * convergence;
            if (def.Id == "blood_altar") bonus += 0.10 * convergence;
            if (def.Id == "void_cloak") bonus += 0.10 * convergence;
            if (def.Id == "flesh_muscle") bonus += 0.20 * convergence;
        }
        bonus *= TechTree.ElderSignTapBonus(o);
        return bonus;
    }

    public static bool BloodVoidConversionActive(OccultState o) =>
        SocketedSuitCounts(o).GetValueOrDefault(ArtifactSuit.Blood) >= 2 && SocketedSuitCounts(o).GetValueOrDefault(ArtifactSuit.Void) >= 1;

    public static double FaithBonus(OccultState o)
    {
        double bonus = 1.0;
        double convergence = TechTree.CosmicConvergenceMult(o);
        foreach (var artifactId in o.SocketedArtifacts)
        {
            var def = OccultData.Artifact(artifactId);
            if (def == null) continue;
            if (def.Id == "void_orb") bonus += 0.10 * convergence;
            if (def.Id == "void_crown") bonus += 0.15 * convergence;
            if (def.Id == "mind_spiral") bonus += 0.20 * convergence;
        }
        bonus += TechTree.VoidTwinFaithBonus(o);
        return bonus;
    }

    public static double GlobalProductionMult(OccultState o)
    {
        double mult = 1.0;
        double convergence = TechTree.CosmicConvergenceMult(o);
        foreach (var artifactId in o.SocketedArtifacts)
        {
            var def = OccultData.Artifact(artifactId);
            if (def == null) continue;
            if (def.Id == "flesh_graft") mult += 0.15 * convergence;
            if (def.Id == "flesh_seed") mult += 0.05 * convergence;
        }
        return mult;
    }

    public static int AcolyteCapBonus(OccultState o)
    {
        int bonus = 0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && (def.Id == "flesh_golem" || def.Id == "flesh_heart")) bonus += def.Id == "flesh_heart" ? 100 : 50; }
        return bonus;
    }
}
