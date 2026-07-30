namespace CultSimulator.Game;

public record InstitutionDef(
    string Id,
    string Name,
    string TerritoryId,
    InstitutionType Type,
    InstitutionTier Tier,
    double Defense,
    double DetectionRate,
    double ReconRisk,
    string RewardLabel,
    double RewardValue,
    string Description,
    string[]? Prerequisites,
    double Latitude = 0,
    double Longitude = 0);

public record TerritoryDef(
    string Id,
    string Name,
    string Icon,
    string BonusLabel,
    double FaithMultiplier,
    double AgentMultiplier,
    string[] InstitutionIds);
