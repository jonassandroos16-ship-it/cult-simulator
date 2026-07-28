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
            state.Buildings ??= new Dictionary<BuildingType, int>();
            state.Upgrades ??= new List<UpgradeId>();
            return state;
        }
        catch { return GameEngine.InitialState(); }
    }

    public static string SaveGame(GameState s) =>
        JsonSerializer.Serialize(s, JsonOptions);
}
