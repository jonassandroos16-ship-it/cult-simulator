using System.Text.Json;
using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public static class SaveLoad
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

    public static GameState LoadGame(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return GameEngine.InitialState();
        try
        {
            var state = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
            if (state == null) return GameEngine.InitialState();
            Migrate(state);
            return state;
        }
        catch { return GameEngine.InitialState(); }
    }

    /// <summary>
    /// Migrates pre-story saves into the multi-coven model. The home coven
    /// keeps the old scalar followers/faith/gold; rival covens are created
    /// as not-taken-over. StoryShown is forced false so old saves see the
    /// intro story exactly once.
    /// </summary>
    private static void Migrate(GameState state)
    {
        state.Covens ??= new List<CovenState>();
        if (state.Covens.Count == 0)
        {
            state.Covens.Add(new CovenState { Id = "skanor", TakenOver = true });
            state.StoryShown = false;
        }
        if (string.IsNullOrEmpty(state.ActiveCovenId))
            state.ActiveCovenId = "skanor";
        foreach (var c in state.Covens)
        {
            c.Buildings ??= new Dictionary<BuildingType, int>();
            c.Upgrades ??= new List<UpgradeId>();
        }
    }

    public static string SaveGame(GameState s) =>
        JsonSerializer.Serialize(s, JsonOptions);
}
