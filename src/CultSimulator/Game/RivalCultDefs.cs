namespace CultSimulator.Game;

public record RivalCultDef(
    string Id,
    string Name,
    string Icon,
    RivalCultArchetype Archetype,
    string Description,
    string PreferredTerritoryId,
    double GrowthRate,
    double AgentStrength,
    double Aggression);
