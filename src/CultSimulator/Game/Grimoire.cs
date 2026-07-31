namespace CultSimulator.Game;

public static class Grimoire
{
    public static bool IsUnlocked(OccultState o) => o.UnlockedTechs.Contains(TechId.TransmutationCrucible);
    public static bool OwnsArtifact(OccultState o, string id) => o.OwnedArtifacts.Contains(id);
    public static IReadOnlyList<ArtifactDef> OwnedArtifacts(OccultState o) => OccultData.Artifacts.Where(a => o.OwnedArtifacts.Contains(a.Id)).ToList();
    public static IReadOnlyList<ArtifactDef> AvailableArtifacts(OccultState o) => OccultData.Artifacts.Where(a => !o.OwnedArtifacts.Contains(a.Id)).ToList();
    public static IReadOnlyList<ArtifactDef> SocketedArtifacts(OccultState o) => OccultData.Artifacts.Where(a => o.SocketedArtifacts.Contains(a.Id)).ToList();
    public static int UnlockedSocketCount(OccultState o) => o.UnlockedSocketCount;
    public static bool CanSocket(OccultState o) => o.SocketedArtifacts.Count < o.UnlockedSocketCount;
    public static void Socket(OccultState o, string artifactId) { if (CanSocket(o) && o.OwnedArtifacts.Contains(artifactId) && !o.SocketedArtifacts.Contains(artifactId)) o.SocketedArtifacts.Add(artifactId); }
    public static void Unsocket(OccultState o, string artifactId) { o.SocketedArtifacts.Remove(artifactId); }
    public static double ProductionMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "flesh_graft") mult += 0.15; }
        return mult;
    }
    public static double ClickPowerMult(OccultState o)
    {
        double mult = 1.0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "soul_lantern") mult += 0.25; }
        return mult;
    }
    public static double SuspicionReductionBonus(OccultState o)
    {
        double bonus = 0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "mind_tongue") bonus += 0.15; }
        return bonus;
    }
    public static int AcolyteCapBonus(OccultState o)
    {
        int bonus = 0;
        foreach (var artifactId in o.SocketedArtifacts) { var def = OccultData.Artifact(artifactId); if (def != null && def.Id == "flesh_golem") bonus += 50; }
        return bonus;
    }
}
