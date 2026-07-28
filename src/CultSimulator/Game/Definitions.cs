namespace CultSimulator.Game;

public record BuildingDef(
    BuildingType Type,
    string Name,
    string Icon,
    int BaseCost,
    ResourceKind CostResource,
    double Growth,
    string EffectDescription);

public record UpgradeDef(
    UpgradeId Id,
    string Name,
    string Icon,
    int FaithCost,
    int GoldCost,
    string EffectDescription,
    int UnlockFollowers);

public record RankDef(string Name, int MinFollowers, string Color);

public record EventChoice(string Label, string Description, Action<CovenState> Apply);

public record EventDef(
    string Id,
    string Title,
    string Narrative,
    EventChoice ChoiceA,
    EventChoice ChoiceB);
