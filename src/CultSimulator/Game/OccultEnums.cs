namespace CultSimulator.Game;

public enum TechId
{
    // Branch 1: Blood & Flesh (8)
    SanguineAutomata,
    OsmoticExtraction,
    AutophagousCult,
    ExsanguinationEngine,
    CrimsonTide,
    FleshBinding,
    MarrowTransfusion,
    BloodApocalypse,

    // Branch 2: Mind & Coercion (8) — remade
    PropagandaNetwork,
    CognitiveSedation,
    NeuralChoir,
    MassHysteria,
    SubliminalBroadcast,
    ZealotConditioning,
    IndoctrinationRites,
    CollectiveTrance,

    // Branch 3: Void & Astral (8)
    SecondSocket,
    TransmutationCrucible,
    ThirdSocket,
    ResonanceMastery,
    VoidTwin,
    AstralDistillation,
    ElderSign,
    CosmicConvergence,

    // Branch 4: The Outer Gate (6)
    MemoriesOfTheDeep,
    AstralAnchor,
    TheStarEatersFeast,
    EchoesOfCreation,
    VoidHeart,
    AscensionProtocol
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
