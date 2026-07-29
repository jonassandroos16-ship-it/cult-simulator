namespace CultSimulator.Game;

/// <summary>
/// A single choice the player can make during a conversion step.
/// <see cref="Apply"/> mutates the home coven and returns an optional
/// outcome message (null = no popup).
/// </summary>
public record ConversionChoice(
    string Label,
    string Description,
    Func<CovenState, string?> Apply);

/// <summary>
/// One beat of a narrative siege. Each step presents a dilemma specific to
/// the rival coven's culture and history. The player picks a choice, which
/// advances the siege progress by <see cref="ProgressGain"/> and may cost
/// or grant resources. Some steps are risky — <see cref="RiskChance"/> is
/// the probability the bold choice backfires.
/// </summary>
public record ConversionStep(
    string Id,
    string Title,
    string Narrative,
    ConversionChoice ChoiceA,
    ConversionChoice ChoiceB,
    double ProgressGain,
    double RiskChance = 0.0);

/// <summary>
/// A full data-driven conversion sequence for one rival coven. Steps are
/// played in order; completing the final step marks the coven as converted.
/// Each coven has its own themed arc so every conquest feels distinct.
/// </summary>
public record ConversionDef(
    string CovenId,
    string Theme,
    IReadOnlyList<ConversionStep> Steps);
