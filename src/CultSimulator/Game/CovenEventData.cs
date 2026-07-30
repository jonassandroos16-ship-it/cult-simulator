using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CultSimulator.Game;

/// <summary>
/// Data-driven event definitions loaded from each coven's JSON file.
/// Each coven carries its own pool of events so the narrative stays local
/// and flavored to the coven's region.
/// Effects scale with the player's current resources via percentage fields,
/// so a +15% Faith reward matters whether you have 100 or 100,000 faith.
/// </summary>
public class CovenEventChoiceData
{
    public string Label { get; set; } = "";
    public string Description { get; set; } = "";

    public CovenEventEffects? Effects { get; set; }

    public CovenEventRandomOutcome? Random { get; set; }
}

public class CovenEventEffects
{
    public int Followers { get; set; }
    public int Faith { get; set; }
    public int Gold { get; set; }

    /// <summary>Percentage of current followers to add/remove (0.15 = +15%).</summary>
    public double FollowersPct { get; set; }
    /// <summary>Percentage of current faith to add/remove (0.15 = +15%).</summary>
    public double FaithPct { get; set; }
    /// <summary>Percentage of current gold to add/remove (0.15 = +15%).</summary>
    public double GoldPct { get; set; }
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
        s.Followers += e.Followers + (int)(s.Followers * e.FollowersPct);
        s.Faith += e.Faith + (int)(s.Faith * e.FaithPct);
        s.Gold += e.Gold + (int)(s.Gold * e.GoldPct);
    }
}
