namespace CultSimulator.Game;

public record TechDef(
    TechId Id, string Name, string Icon, int FaithCost,
    TechBranch Branch, string EffectDescription,
    TechId[]? Prerequisites = null);

public record ArtifactDef(
    string Id, string Name, string Icon,
    ArtifactSuit Suit, string EffectDescription);

public record MinionTraitDef(
    string Id, string Name, string Description,
    double RaidPowerMult, double SuspicionMult, double FaithMult);

public record CauldronRecipeDef(
    CauldronRecipeId Id, string Name, string Icon,
    Dictionary<MaterialKind, int> Materials,
    string EffectDescription, bool IsPermanent);

public record MapNodeDef(
    string Id, string Name, string Icon,
    int FaithCost, double ArmyPowerRequired,
    double FaithPerSec, double SuspicionPerSec,
    Dictionary<MaterialKind, int>? Materials = null);
