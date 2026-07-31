namespace CultSimulator.Game;

/// <summary>
/// Pure functions that drive the narrative-siege conversion flow.
/// Works over <see cref="GameState"/> + <see cref="ConversionDef"/> data
/// so it stays modular and testable without the UI.
/// </summary>
public static class ConversionEngine
{
    public static bool IsActive(GameState state) =>
        state.Conversion != null && !state.Conversion.Completed;

    public static ConversionDef? DefinitionFor(ConversionDataService data, string covenId) =>
        data.Find(covenId);

    public static bool CanStartConversion(GameState state, WorldLocationDef loc)
    {
        if (loc.Id == "skanor") return false;
        var coven = state.FindCoven(loc.Id);
        if (coven != null && coven.Converted) return false;
        return CovenProgress.TotalFollowers(state) >= loc.FollowersRequired;
    }

    public static void StartConversion(GameState state, ConversionDataService data, WorldLocationDef loc)
    {
        if (!CanStartConversion(state, loc)) return;
        var def = data.Find(loc.Id);
        if (def == null) return;

        state.Conversion = new ConversionState
        {
            CovenId = loc.Id,
            CurrentStep = 0,
            Progress = 0.0,
            Completed = false,
            LastOutcome = null,
            BattlePhase = false,
            BattleWon = false
        };
    }

    public static ConversionStep? CurrentStep(GameState state, ConversionDataService data)
    {
        if (state.Conversion == null || state.Conversion.Completed) return null;
        if (state.Conversion.BattlePhase) return null;
        var def = data.Find(state.Conversion.CovenId);
        if (def == null) return null;
        if (state.Conversion.CurrentStep >= def.Steps.Count) return null;
        return def.Steps[state.Conversion.CurrentStep];
    }

    public static string? ApplyChoice(GameState state, ConversionDataService data, ConversionChoice choice)
    {
        if (state.Conversion == null || state.Conversion.Completed) return null;
        if (state.Conversion.BattlePhase) return null;
        var def = data.Find(state.Conversion.CovenId);
        if (def == null) return null;
        if (state.Conversion.CurrentStep >= def.Steps.Count) return null;

        var step = def.Steps[state.Conversion.CurrentStep];
        var outcome = choice.Apply(state.HomeCoven);
        Clamp(state.HomeCoven);

        state.Conversion.Progress = Math.Min(1.0, state.Conversion.Progress + step.ProgressGain);
        state.Conversion.LastOutcome = outcome;
        state.Conversion.CurrentStep++;

        if (state.Conversion.CurrentStep >= def.Steps.Count)
            EnterBattlePhase(state);

        return outcome;
    }

    private static void EnterBattlePhase(GameState state)
    {
        if (state.Conversion == null) return;
        state.Conversion.BattlePhase = true;
    }

    public static void OnBattleWon(GameState state, ConversionDataService data)
    {
        if (state.Conversion == null || !state.Conversion.BattlePhase) return;
        state.Conversion.BattleWon = true;
        var def = data.Find(state.Conversion.CovenId);
        if (def != null)
            FinalizeConversion(state, def);
    }

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

    public static void Cancel(GameState state)
    {
        state.Conversion = null;
    }

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
