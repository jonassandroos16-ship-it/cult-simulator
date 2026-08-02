namespace CultSimulator.Game;

public enum TechId
{
    // Branch 1: Blood & Flesh
    SanguineAutomata,
    OsmoticExtraction,
    AutophagousCult,
    ExsanguinationEngine,

    // Branch 2: Mind & Coercion
    WhispersInTheDark,
    InquisitorsBlindfold,
    ShadowTactics,
    MassHysteria,

    // Branch 3: Void & Astral
    SecondSocket,
    TransmutationCrucible,
    ThirdSocket,
    ResonanceMastery,

    // Branch 4: The Outer Gate
    MemoriesOfTheDeep,
    AstralAnchor,
    TheStarEatersFeast
}

public enum TechBranch { BloodFlesh, MindCoercion, VoidAstral, OuterGate }

public enum ArtifactSuit { Blood, Void, Mind, Flesh }

public enum MinionTier { Acolyte, Promoted, HighCouncil }

public enum PromotedRole { Zealot, Scholar, Infiltrator, Mage }

public enum CouncilRole { Inquisitor, Archon, HighPriest }

public enum NodeStance { Harvest, Veil }

public enum CauldronRecipeId
{
    CrimsonElixir,
    VoidTincture,
    MindPhiltre,
    FleshBrew,
    WarElixir,
    BloodForge,
    VoidForge,
    MindForge,
    FleshForge
}

public enum MaterialKind { GraveDust, DemonBile, AstralMercury }
