namespace CultSimulator.Game;

/// <summary>
/// Redesigned Cauldron: crafts elixirs and forges artifacts using
/// Shadow War agents as currency instead of Materials (which were
/// tied to the removed Ley Line system). Each recipe costs a flat
/// number of agents from the player's agent pool.
/// </summary>
public static class Cauldron
{
    public static bool IsUnlocked(OccultState o) => TechTree.HasTech(o, TechId.TransmutationCrucible);

    public static bool CanCraft(GameState state, CauldronRecipeDef recipe)
    {
        if (!IsUnlocked(state.Occult)) return false;
        var sw = ShadowWarEngine.EnsureInitialized(state);
        return sw.AvailableAgents >= recipe.AgentCost;
    }

    public static (bool success, string? artifactId) Craft(GameState state, CauldronRecipeId id)
    {
        var recipe = OccultData.Recipe(id);
        var sw = ShadowWarEngine.EnsureInitialized(state);
        if (!CanCraft(state, recipe)) return (false, null);

        sw.TotalAgents -= recipe.AgentCost;

        if (recipe.IsPermanent)
        {
            var suit = id switch {
                CauldronRecipeId.BloodForge => ArtifactSuit.Blood,
                CauldronRecipeId.VoidForge => ArtifactSuit.Void,
                CauldronRecipeId.MindForge => ArtifactSuit.Mind,
                CauldronRecipeId.FleshForge => ArtifactSuit.Flesh,
                _ => ArtifactSuit.Blood
            };
            var unowned = OccultData.Artifacts.Where(a => a.Suit == suit && !Grimoire.OwnsArtifact(state.Occult, a.Id)).ToList();
            if (unowned.Count == 0) return (false, null);
            var chosen = unowned[Random.Shared.Next(unowned.Count)];
            Grimoire.AddArtifact(state.Occult, chosen.Id);
            return (true, chosen.Id);
        }
        ApplyElixir(state.Occult, id);
        return (true, null);
    }

    private static void ApplyElixir(OccultState o, CauldronRecipeId id)
    {
        o.ElixirTimer = OccultBalance.ElixirDurationSec;
        switch (id)
        {
            case CauldronRecipeId.CrimsonElixir: o.ElixirTapMult = 2.0; break;
            case CauldronRecipeId.VoidTincture: o.ElixirFaithMult = 1.5; break;
            case CauldronRecipeId.MindPhiltre: o.ElixirSuspicionMult = 0.5; break;
            case CauldronRecipeId.FleshBrew: o.Initiates += 100; break;
        }
    }

    public static void TickElixir(OccultState o, double deltaSec)
    {
        if (o.ElixirTimer <= 0) return;
        o.ElixirTimer -= deltaSec;
        if (o.ElixirTimer <= 0) { o.ElixirTimer = 0; o.ElixirTapMult = 1.0; o.ElixirFaithMult = 1.0; o.ElixirSuspicionMult = 1.0; }
    }
}
