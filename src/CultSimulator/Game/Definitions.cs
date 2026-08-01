namespace CultSimulator.Game;

public record UpgradeDef(
    UpgradeId Id,
    string Name,
    string Icon,
    int FaithCost,
    int GoldCost,
    int AgentCost,
    string EffectDescription,
    int UnlockFollowers);
