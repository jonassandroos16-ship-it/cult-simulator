namespace CultSimulator.Game;

public record AgentTypeDef(
    AgentType Type,
    string Name,
    string Icon,
    string Description,
    double Attack,
    double Defense,
    double Stealth,
    int FaithCost);

public record BattleTheaterDef(
    string ContinentId,
    string Name,
    string Icon,
    string Description);
