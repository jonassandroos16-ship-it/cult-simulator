using System.Text.Json;
using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public static class SaveLoad
{
    public static readonly JsonSerializerOptions JsonOptions = new() { Converters = { new JsonStringEnumConverter() }, WriteIndented = false };

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
        catch
        {
            // Deserialization failed — do NOT wipe progress. Return a fresh
            // initial state but the caller is expected to have kept the raw
            // JSON so it can be recovered. Returning InitialState here is a
            // last resort; the GameService wraps this to preserve the blob.
            return GameEngine.InitialState();
        }
    }

    /// <summary>
    /// Attempts to load a save; if it fails, tries the backup key before
    /// giving up and starting fresh. This prevents progress loss from a
    /// single corrupted write.
    /// </summary>
    public static (GameState state, bool loaded) LoadGameWithBackup(string? primary, string? backup)
    {
        if (!string.IsNullOrWhiteSpace(primary))
        {
            try
            {
                var s = JsonSerializer.Deserialize<GameState>(primary, JsonOptions);
                if (s != null) { Migrate(s); return (s, true); }
            }
            catch { /* fall through to backup */ }
        }

        if (!string.IsNullOrWhiteSpace(backup))
        {
            try
            {
                var s = JsonSerializer.Deserialize<GameState>(backup, JsonOptions);
                if (s != null) { Migrate(s); return (s, true); }
            }
            catch { /* fall through to fresh */ }
        }

        return (GameEngine.InitialState(), false);
    }

    private static void Migrate(GameState state)
    {
        state.Covens ??= new List<CovenState>();
        if (state.Covens.Count == 0) { state.Covens.Add(new CovenState { Id = "skanor", Converted = true }); state.StoryShown = false; }
        if (string.IsNullOrEmpty(state.ActiveCovenId)) state.ActiveCovenId = "skanor";
        if (state.LastSavedAt == 0) state.LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        foreach (var c in state.Covens)
        {
            c.Buildings ??= new Dictionary<BuildingType, int>();
            c.Upgrades ??= new List<UpgradeId>();
        }
        state.Occult ??= new OccultState();
        state.Occult.Minions ??= new List<Minion>();
        state.Occult.HighCouncil ??= new List<CovenMember>();
        state.Occult.MapNodes ??= new List<MapNodeState>();
        state.Occult.UnlockedTechs ??= new List<TechId>();
        state.Occult.SocketedArtifacts ??= new List<string>();
        state.Occult.OwnedArtifacts ??= new List<string>();
        state.Occult.Materials ??= new Dictionary<MaterialKind, int>();
        state.Occult.LeyLines ??= new List<string[]>();
        state.Occult.ShadowAgents ??= new List<ShadowAgent>();
        state.Conversion ??= null;
        state.ActiveLocalCults ??= new List<LocalCultInstance>();
    }

    public static string SaveGame(GameState s) => JsonSerializer.Serialize(s, JsonOptions);
}