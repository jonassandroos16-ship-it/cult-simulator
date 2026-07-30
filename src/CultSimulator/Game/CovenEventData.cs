using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CultSimulator.Game;

/// <summary>
/// Data-driven event definitions loaded from each coven's JSON file.
/// Each coven carries its own pool of events so the narrative stays local
/// and flavored to the coven's region (e.g. a rival cult near Skanör is
/// "Lund", while one near Malkin Tower is "Burnley").
/// </summary>
public class CovenEventChoiceData
{
    public string Label { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>Flat deltas applied to the active coven when this choice is taken.</summary>
    public CovenEventEffects? Effects { get; set; }

    /// <summary>
    /// A 50/50 gamble: if won, <see cref="Effects"/> apply; if lost,
    /// <see cref="LossEffects"/> apply (defaults to a mirror of Effects).
    /// When null the choice is deterministic.
    /// </summary>
    public CovenEventRandomOutcome? Random { get; set; }
}

public class CovenEventEffects
{
    public int Followers { get; set; }
    public int Faith { get; set; }
    public int Gold { get; set; }
}

public class CovenEventRandomOutcome
{
    public double WinChance { get; set; } = 0.5;
    public string? WinMessage { get; set; }
    public string? LossMessage { get; set; }
    public CovenEventEffects? LossEffects { get; set; }
}

public class CovenEventData
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Narrative { get; set; } = "";
    public CovenEventChoiceData ChoiceA { get; set; } = new();
    public CovenEventChoiceData ChoiceB { get; set; } = new();

    /// <summary>Converts the data-driven definition into a runtime <see cref="EventDef"/>.</summary>
    public EventDef ToEventDef()
    {
        return new EventDef(Id, Title, Narrative, ToChoice(ChoiceA), ToChoice(ChoiceB));
    }

    private static EventChoice ToChoice(CovenEventChoiceData data)
    {
        return new EventChoice(data.Label, data.Description, s => ApplyEffects(s, data));
    }

    private static string? ApplyEffects(CovenState s, CovenEventChoiceData data)
    {
        if (data.Random == null)
        {
            ApplyEffects(s, data.Effects);
            return null;
        }

        bool won = Random.Shared.NextDouble() < data.Random.WinChance;
        if (won)
        {
            ApplyEffects(s, data.Effects);
            return data.Random.WinMessage;
        }
        ApplyEffects(s, data.Random.LossEffects ?? data.Effects);
        return data.Random.LossMessage;
    }

    private static void ApplyEffects(CovenState s, CovenEventEffects? e)
    {
        if (e == null) return;
        s.Followers += e.Followers;
        s.Faith += e.Faith;
        s.Gold += e.Gold;
    }
}
