namespace CultSimulator.Game;

public record ArtifactDef(
    string Id, string Name, string Icon, ArtifactSuit Suit,
    string EffectDescription, double TapPowerBonus,
    double FaithBonus, double SuspicionReductionBonus,
    double GlobalProductionMult);

public record CauldronRecipeDef(
    CauldronRecipeId Id, string Name, string Icon,
    int AgentCost,
    string EffectDescription, bool IsPermanent);

public record MapNodeDef(
    string Id, string Name, string Icon,
    int FaithCost, double ArmyPowerRequired,
    double FaithPerSec, double SuspicionPerSec,
    double Latitude, double Longitude,
    string LocationName,
    string CovenId,
    Dictionary<MaterialKind, int>? Materials);
