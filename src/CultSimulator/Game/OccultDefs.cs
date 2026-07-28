namespace CultSimulator.Game;

public record TechDef(
    TechId Id,
    string Name,
    string Icon,
    int FkCost,
    TechBranch Branch,
    string EffectDescription,
    TechId[]? Prerequisites = null);

public record ArtifactDef(
    string Id,
    string Name,
    string Icon,
    ArtifactSuit Suit,
    string EffectDescription);

public record MinionTraitDef(
    string Id,
    string Name,
    string Description,
    double RaidPowerMult,
    double SuspicionMult,
    double FkMult);

public record CauldronRecipeDef(
    CauldronRecipeId Id,
    string Name,
    string Icon,
    Dictionary<MaterialKind, int> Materials,
    string EffectDescription,
    bool IsPermanent);

public record MapNodeDef(
    string Id,
    string Name,
    string Icon,
    int DevotionCost,
    double ArmyPowerRequired,
    double FkPerSec,
    double SuspicionPerSec,
    Dictionary<MaterialKind, int>? Materials = null);
