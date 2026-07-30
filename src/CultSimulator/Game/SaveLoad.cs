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

        // Migrate legacy global Occult state into the home coven.
        if (state.ExtensionData != null && state.ExtensionData.TryGetValue("Occult", out var legacyOccultJson))
        {
            // Extract global prestige fields before deserializing (they're no longer on OccultState).
            if (legacyOccultJson.TryGetProperty("EldritchFavor", out var favorVal))
                state.EldritchFavor = favorVal.GetDouble();
            if (legacyOccultJson.TryGetProperty("GrandSacrificeCount", out var countVal))
                state.GrandSacrificeCount = countVal.GetInt32();

            var legacy = legacyOccultJson.Deserialize<OccultState>(SaveLoad.JsonOptions);
            if (legacy != null)
            {
                var home = state.Covens.FirstOrDefault(c => c.Id == "skanor");
                if (home != null) home.Occult = legacy;
            }
            state.ExtensionData.Remove("Occult");
        }

        foreach (var c in state.Covens)
        {
            c.Buildings ??= new Dictionary<BuildingType, int>();
            c.Upgrades ??= new List<UpgradeId>();
            c.Occult ??= new OccultState();
            c.Occult.Minions ??= new List<Minion>();
            c.Occult.HighCouncil ??= new List<CovenMember>();
            c.Occult.MapNodes ??= new List<MapNodeState>();
            c.Occult.UnlockedTechs ??= new List<TechId>();
            c.Occult.SocketedArtifacts ??= new List<string>();
            c.Occult.OwnedArtifacts ??= new List<string>();
            c.Occult.Materials ??= new Dictionary<MaterialKind, int>();
            c.Occult.LeyLines ??= new List<string[]>();
        }
        state.Conversion ??= null;
        state.ActiveLocalCults ??= new List<LocalCultInstance>();
        state.ShadowWar ??= ShadowWarEngine.CreateInitialState();
        state.ShadowWar.Institutions ??= new List<InstitutionState>();
        // Ensure all institutions exist (for saves from before Shadow War)
        foreach (var def in ShadowWarData.Institutions)
            if (state.ShadowWar.GetInstitution(def.Id) == null)
                state.ShadowWar.Institutions.Add(new InstitutionState
                {
                    Id = def.Id,
                    Status = def.Prerequisites == null || def.Prerequisites.Length == 0
                        ? InstitutionStatus.Unlocked
                        : InstitutionStatus.Locked,
                    DefenseRemaining = def.Defense
                });
    }

    public static string SaveGame(GameState s) => JsonSerializer.Serialize(s, JsonOptions);
}
