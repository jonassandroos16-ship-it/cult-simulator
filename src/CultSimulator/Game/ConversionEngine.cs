namespace CultSimulator.Game;

/// <summary>
/// Pure functions that drive the narrative-siege conversion flow.
/// Works over <see cref="GameState"/> + <see cref="ConversionDef"/> data
/// so it stays modular and testable without the UI.
/// </summary>
public static class ConversionEngine
{
    /// <summary>True if a conversion is currently in progress.</summary>
    public static bool IsActive(GameState state) =>
        state.Conversion != null && !state.Conversion.Completed;

    /// <summary>The conversion definition for the active or requested coven.</summary>
    public static ConversionDef? DefinitionFor(string covenId) =>
        ConversionData.Find(covenId);

    /// <summary>True if the player meets the follower threshold to begin a conversion.</summary>
    public static bool CanStartConversion(GameState state, WorldLocationDef loc)
    {
        if (loc.Id == "skanor") return false;
        var coven = state.FindCoven(loc.Id);
        if (coven != null && coven.Converted) return false;
        return CovenProgress.TotalFollowers(state) >= loc.FollowersRequired;
    }

    /// <summary>Begins a new conversion sequence for the given coven.</summary>
    public static void StartConversion(GameState state, WorldLocationDef loc)
    {
        if (!CanStartConversion(state, loc)) return;
        var def = DefinitionFor(loc.Id);
        if (def == null) return;

        state.Conversion = new ConversionState
        {
            CovenId = loc.Id,
            CurrentStep = 0,
            Progress = 0.0,
            Completed = false,
            LastOutcome = null
        };
    }

    /// <summary>The current step definition, or null if the sequence is finished.</summary>
    public static ConversionStep? CurrentStep(GameState state)
    {
        if (state.Conversion == null || state.Conversion.Completed) return null;
        var def = DefinitionFor(state.Conversion.CovenId);
        if (def == null) return null;
        if (state.Conversion.CurrentStep >= def.Steps.Count) return null;
        return def.Steps[state.Conversion.CurrentStep];
    }

    /// <summary>
    /// Applies the player's choice for the current step, advances progress,
    /// and advances to the next step. If the final step is completed, the
    /// coven is marked as converted and the resource sacrifice is applied.
    /// Returns an optional outcome message.
    /// </summary>
    public static string? ApplyChoice(GameState state, ConversionChoice choice)
    {
        if (state.Conversion == null || state.Conversion.Completed) return null;
        var def = DefinitionFor(state.Conversion.CovenId);
        if (def == null) return null;
        if (state.Conversion.CurrentStep >= def.Steps.Count) return null;

        var step = def.Steps[state.Conversion.CurrentStep];
        var outcome = choice.Apply(state.HomeCoven);
        Clamp(state.HomeCoven);

        state.Conversion.Progress = Math.Min(1.0, state.Conversion.Progress + step.ProgressGain);
        state.Conversion.LastOutcome = outcome;
        state.Conversion.CurrentStep++;

        if (state.Conversion.CurrentStep >= def.Steps.Count)
            FinalizeConversion(state, def);

        return outcome;
    }

    /// <summary>
    /// Completes the conversion: applies the resource sacrifice to the home
    /// coven and marks the rival coven as converted. This replaces the old
    /// instant Takeover logic.
    /// </summary>
    private static void FinalizeConversion(GameState state, ConversionDef def)
    {
        var home = state.HomeCoven;
        home.Faith *= (1.0 - GameBalance.CovenTakeoverFaithPercent);
        home.Gold *= (1.0 - GameBalance.CovenTakeoverGoldPercent);
        home.Followers = (int)Math.Ceiling(home.Followers * (1.0 - GameBalance.CovenTakeoverFollowerPercent));

        var existing = state.FindCoven(def.CovenId);
        if (existing != null)
        {
            existing.Converted = true;
            existing.Followers = 0;
            existing.Faith = 0;
            existing.Gold = 0;
            existing.PreachCount = 0;
            existing.Buildings = new Dictionary<BuildingType, int>();
            existing.Upgrades = new List<UpgradeId>();
        }
        else
        {
            state.Covens.Add(new CovenState
            {
                Id = def.CovenId,
                Converted = true,
                Buildings = new Dictionary<BuildingType, int>(),
                Upgrades = new List<UpgradeId>()
            });
        }

        state.Conversion!.Completed = true;
    }

    /// <summary>Cancels an in-progress conversion, clearing the state without converting.</summary>
    public static void Cancel(GameState state)
    {
        state.Conversion = null;
    }

    /// <summary>Clears the conversion state after the player has seen the completion screen.</summary>
    public static void ClearCompleted(GameState state)
    {
        if (state.Conversion != null && state.Conversion.Completed)
            state.Conversion = null;
    }

    private static void Clamp(CovenState c)
    {
        if (c.Faith < 0) c.Faith = 0;
        if (c.Gold < 0) c.Gold = 0;
        if (c.Followers < 0) c.Followers = 0;
    }
}
