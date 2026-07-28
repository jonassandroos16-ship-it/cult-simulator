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

public record RankDef(string Name, int MinFollowers, string Color, string FlavorText);

/// <summary>
/// A player-facing event choice. <see cref="Apply"/> mutates the coven state
/// and returns an optional outcome message shown in the generic popup
/// (null/empty means no popup is displayed).
/// </summary>
public record EventChoice(string Label, string Description, Func<CovenState, string?> Apply);

public record EventDef(
    string Id,
    string Title,
    string Narrative,
    EventChoice ChoiceA,
    EventChoice ChoiceB);