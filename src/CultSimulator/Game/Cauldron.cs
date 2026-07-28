namespace CultSimulator.Game;

/// <summary>
/// Cauldron crafting logic: checking material availability, consuming
/// materials, and producing elixirs or forged artifacts.
/// Pure functions over <see cref="OccultState"/>.
/// </summary>
public static class Cauldron
{
    public static bool IsUnlocked(OccultState o) =>
        TechTree.HasTech(o, TechId.TransmutationCrucible);

    public static bool HasMaterials(OccultState o, CauldronRecipeDef recipe)
    {
        foreach (var (material, amount) in recipe.Materials)
        {
            if (o.Materials.GetValueOrDefault(material) < amount) return false;
        }
        return true;
    }

    public static bool CanCraft(OccultState o, CauldronRecipeDef recipe) =>
        IsUnlocked(o) && HasMaterials(o, recipe);

    public static bool ConsumeMaterials(OccultState o, CauldronRecipeDef recipe)
    {
        if (!HasMaterials(o, recipe)) return false;
        foreach (var (material, amount) in recipe.Materials)
        {
            o.Materials[material] -= amount;
        }
        return true;
    }

    public static (bool success, string? artifactId) Craft(OccultState o, CauldronRecipeId id)
    {
        var recipe = OccultData.Recipe(id);
        if (!CanCraft(o, recipe)) return (false, null);
        ConsumeMaterials(o, recipe);

        if (recipe.IsPermanent)
        {
            var suit = id switch
            {
                CauldronRecipeId.BloodForge => ArtifactSuit.Blood,
                CauldronRecipeId.VoidForge => ArtifactSuit.Void,
                CauldronRecipeId.MindForge => ArtifactSuit.Mind,
                CauldronRecipeId.FleshForge => ArtifactSuit.Flesh,
                _ => ArtifactSuit.Blood
            };
            var candidates = OccultData.Artifacts.Where(a => a.Suit == suit).ToList();
            var unowned = candidates.Where(a => !Grimoire.OwnsArtifact(o, a.Id)).ToList();
            if (unowned.Count == 0) return (false, null);
            var chosen = unowned[Random.Shared.Next(unowned.Count)];
            Grimoire.AddArtifact(o, chosen.Id);
            return (true, chosen.Id);
        }

        ApplyElixir(o, id);
        return (true, null);
    }

    private static void ApplyElixir(OccultState o, CauldronRecipeId id)
    {
        o.ElixirTimer = OccultBalance.ElixirDurationSec;
        switch (id)
        {
            case CauldronRecipeId.CrimsonElixir:
                o.ElixirTapMult = 2.0;
                o.ElixirFkMult = 1.0;
                o.ElixirSuspicionMult = 1.0;
                break;
            case CauldronRecipeId.VoidTincture:
                o.ElixirTapMult = 1.0;
                o.ElixirFkMult = 1.5;
                o.ElixirSuspicionMult = 1.0;
                break;
            case CauldronRecipeId.MindPhiltre:
                o.ElixirTapMult = 1.0;
                o.ElixirFkMult = 1.0;
                o.ElixirSuspicionMult = 0.5;
                break;
            case CauldronRecipeId.FleshBrew:
                o.ElixirTapMult = 1.0;
                o.ElixirFkMult = 1.0;
                o.ElixirSuspicionMult = 1.0;
                o.Acolytes += 100;
                break;
        }
    }

    public static void TickElixir(OccultState o, double deltaSec)
    {
        if (o.ElixirTimer <= 0) return;
        o.ElixirTimer -= deltaSec;
        if (o.ElixirTimer <= 0)
        {
            o.ElixirTimer = 0;
            o.ElixirTapMult = 1.0;
            o.ElixirFkMult = 1.0;
            o.ElixirSuspicionMult = 1.0;
        }
    }
}