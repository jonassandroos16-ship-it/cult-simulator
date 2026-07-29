namespace CultSimulator.Game;

/// <summary>
/// Mutable state for an in-progress coven conversion (narrative siege).
/// Persisted as part of <see cref="GameState"/> so a conversion can be
/// paused and resumed across page reloads.
/// </summary>
public class ConversionState
{
    /// <summary>The rival coven being converted.</summary>
    public string CovenId { get; set; } = "";

    /// <summary>Index of the current step in the conversion sequence.</summary>
    public int CurrentStep { get; set; }

    /// <summary>Accumulated siege progress (0.0–1.0).</summary>
    public double Progress { get; set; }

    /// <summary>True after the final step is completed.</summary>
    public bool Completed { get; set; }

    /// <summary>Optional message from the last choice (shown as a brief outcome).</summary>
    public string? LastOutcome { get; set; }
}
