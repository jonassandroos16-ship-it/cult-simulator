namespace CultSimulator.Game;

public record AgentTypeDef(
    AgentType Type,
    string Name,
    string Icon,
    string Description,
    double Attack,
    double Defense,
    double Stealth,
    int AgentCost,
    double FaithRegen = 0,
    bool IsSupport = false);

public record BattleTheaterDef(
    string ContinentId,
    string Name,
    string Icon,
    string Description);
