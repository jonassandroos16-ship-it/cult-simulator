namespace CultSimulator.Game;

/// <summary>
/// Generates conversion definitions dynamically from coven event data
/// loaded by <see cref="WorldLocationService"/>. No hardcoded data —
/// every coven in manifest.json gets a conversion sequence derived
/// from its own events JSON.
/// </summary>
public class ConversionDataService
{
    private readonly WorldLocationService _locations;
    private Dictionary<string, ConversionDef>? _cache;

    public ConversionDataService(WorldLocationService locations) => _locations = locations;

    public IReadOnlyList<ConversionDef> All
    {
        get
        {
            EnsureCache();
            return _cache!.Values.ToList();
        }
    }

    public ConversionDef? Find(string covenId)
    {
        EnsureCache();
        return _cache!.GetValueOrDefault(covenId);
    }

    private void EnsureCache()
    {
        if (_cache != null) return;
        _cache = new Dictionary<string, ConversionDef>();

        foreach (var loc in _locations.Locations)
        {
            if (loc.Id == "skanor") continue;
            if (loc.Events == null || loc.Events.Count == 0) continue;

            var steps = BuildSteps(loc);
            if (steps.Count == 0) continue;

            var theme = $"{loc.CountryFlag} {loc.Name} — {loc.Era}";
            _cache[loc.Id] = new ConversionDef(loc.Id, theme, steps);
        }
    }

    private static List<ConversionStep> BuildSteps(WorldLocationDef loc)
    {
        const int maxSteps = 4;
        var events = loc.Events.Take(maxSteps).ToList();
        if (events.Count == 0) return new List<ConversionStep>();

        double gainPerStep = 1.0 / events.Count;
        var steps = new List<ConversionStep>();

        for (int i = 0; i < events.Count; i++)
        {
            var ev = events[i];
            var stepId = $"{loc.Id}_step_{i + 1}";
            var title = ev.Title;
            var narrative = ev.Narrative;
            var choiceA = ToConversionChoice(ev.ChoiceA);
            var choiceB = ToConversionChoice(ev.ChoiceB);
            steps.Add(new ConversionStep(stepId, title, narrative, choiceA, choiceB, gainPerStep));
        }

        return steps;
    }

    private static ConversionChoice ToConversionChoice(CovenEventChoiceData data)
    {
        return new ConversionChoice(
            data.Label,
            data.Description,
            coven => ApplyChoiceEffects(coven, data));
    }

    private static string? ApplyChoiceEffects(CovenState coven, CovenEventChoiceData data)
    {
        if (data.Random == null)
        {
            ApplyEffects(coven, data.Effects);
            return null;
        }

        bool won = Random.Shared.NextDouble() < data.Random.WinChance;
        if (won)
        {
            ApplyEffects(coven, data.Effects);
            return data.Random.WinMessage;
        }
        ApplyEffects(coven, data.Random.LossEffects ?? data.Effects);
        return data.Random.LossMessage;
    }

    private static void ApplyEffects(CovenState coven, CovenEventEffects? e)
    {
        if (e == null) return;
        coven.Followers += e.Followers + (int)(coven.Followers * e.FollowersPct);
        coven.Faith += e.Faith + (int)(coven.Faith * e.FaithPct);
        coven.Gold += e.Gold + (int)(coven.Gold * e.GoldPct);
    }
}
